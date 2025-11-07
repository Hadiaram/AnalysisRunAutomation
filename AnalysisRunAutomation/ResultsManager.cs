using ETABSv1;
using ETABS_Plugin.Models;

namespace ETABS_Plugin
{
    /// <summary>
    /// Manager for extracting analysis results from ETABS
    /// Handles result setup, extraction, and formatting
    /// </summary>
    public class ResultsManager
    {
        private readonly cSapModel _model;
        private readonly cAnalysisResults _results;

        public ResultsManager(cSapModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _results = model.Results;
        }

        #region Setup Methods

        /// <summary>
        /// Select specific load cases for result extraction
        /// </summary>
        public int SelectCasesForOutput(params string[] caseNames)
        {
            try
            {
                // Deselect all first
                int ret = _results.Setup.DeselectAllCasesAndCombosForOutput();
                if (ret != 0)
                {
                    Console.WriteLine($"Warning: DeselectAll returned {ret}");
                    return ret;
                }

                // Select each specified case
                foreach (string caseName in caseNames)
                {
                    ret = _results.Setup.SetCaseSelectedForOutput(caseName);
                    if (ret != 0)
                    {
                        Console.WriteLine($"⚠ Warning: Failed to select case '{caseName}' (code: {ret})");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Selected case: {caseName}");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error selecting cases: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Select specific load combinations for result extraction
        /// </summary>
        public int SelectCombosForOutput(params string[] comboNames)
        {
            try
            {
                // Deselect all first
                int ret = _results.Setup.DeselectAllCasesAndCombosForOutput();
                if (ret != 0)
                {
                    Console.WriteLine($"Warning: DeselectAll returned {ret}");
                    return ret;
                }

                // Select each specified combo
                foreach (string comboName in comboNames)
                {
                    ret = _results.Setup.SetComboSelectedForOutput(comboName);
                    if (ret != 0)
                    {
                        Console.WriteLine($"⚠ Warning: Failed to select combo '{comboName}' (code: {ret})");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Selected combo: {comboName}");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error selecting combos: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Select both cases and combos for result extraction
        /// </summary>
        public int SelectCasesAndCombosForOutput(string[] caseNames, string[] comboNames)
        {
            try
            {
                // Deselect all first
                int ret = _results.Setup.DeselectAllCasesAndCombosForOutput();
                if (ret != 0) return ret;

                // Select cases
                if (caseNames != null)
                {
                    foreach (string caseName in caseNames)
                    {
                        ret = _results.Setup.SetCaseSelectedForOutput(caseName);
                        if (ret == 0)
                            Console.WriteLine($"✓ Selected case: {caseName}");
                    }
                }

                // Select combos
                if (comboNames != null)
                {
                    foreach (string comboName in comboNames)
                    {
                        ret = _results.Setup.SetComboSelectedForOutput(comboName);
                        if (ret == 0)
                            Console.WriteLine($"✓ Selected combo: {comboName}");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error selecting cases/combos: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Set whether multi-valued combos (envelopes, SRSS) return component results
        /// true = return results for each component case
        /// false = return single enveloped result (default)
        /// </summary>
        public int SetMultiValuedComboOption(bool returnComponents)
        {
            try
            {
                // Convert bool to int: true = 1, false = 0
                int comboOption = returnComponents ? 1 : 0;

                int ret = _results.Setup.SetOptionMultiValuedCombo(comboOption);
                if (ret == 0)
                {
                    Console.WriteLine($"✓ Multi-valued combo option set to: {(returnComponents ? "Component Results" : "Enveloped Results")}");
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error setting multi-valued combo option: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Check if analysis has been run and results are available
        /// </summary>
        public bool AreResultsAvailable(params string[] mustCoverCases)
        {
            int n = 0;
            string[] caseNames = null;
            int[] status = null;

            // 1) Query status for all load cases
            int ret = _model.Analyze.GetCaseStatus(ref n, ref caseNames, ref status);
            if (ret != 0 || n == 0 || caseNames == null || status == null)
                return false;

            // 2) Build a quick lookup of finished cases (status == 4)
            var finished = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < n; i++)
                if (status[i] == 4) finished.Add(caseNames[i]);

            // 3) If the caller requires specific cases, ensure all of them are finished
            if (mustCoverCases != null && mustCoverCases.Length > 0)
                return mustCoverCases.All(c => finished.Contains(c));

            // Otherwise: results exist if at least one case is finished
            return finished.Count > 0;
        }


        #endregion

        #region Base Reactions

        /// <summary>
        /// Extract base reaction results (total reactions at all supports)
        /// </summary>
        public List<BaseReactionResult> GetBaseReactions()
        {
            var results = new List<BaseReactionResult>();

            try
            {
                int n = 0;

                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;

                double[] fx = null, fy = null, fz = null;
                double[] mx = null, my = null, mz = null;

                // Scalars (NOT arrays)
                double gx = 0, gy = 0, gz = 0;

                // Per-result centroid arrays (9 arrays total)
                double[] xCFx = null, yCFx = null, zCFx = null; // centroids contributing to FX
                double[] xCFy = null, yCFy = null, zCFy = null; // centroids contributing to FY
                double[] xCFz = null, yCFz = null, zCFz = null; // centroids contributing to FZ

                int ret = _results.BaseReactWithCentroid(
                    ref n,
                    ref loadCase, ref stepType, ref stepNum,
                    ref fx, ref fy, ref fz,
                    ref mx, ref my, ref mz,
                    ref gx, ref gy, ref gz,
                    ref xCFx, ref yCFx, ref zCFx,
                    ref xCFy, ref yCFy, ref zCFy,
                    ref xCFz, ref yCFz, ref zCFz
                );

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving base reactions with centroid. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < n; i++)
                {
                    results.Add(new BaseReactionResult
                    {
                        LoadCase = loadCase[i],
                        StepType = stepType[i],
                        StepNum = stepNum[i],

                        FX = fx[i],
                        FY = fy[i],
                        FZ = fz[i],
                        MX = mx[i],
                        MY = my[i],
                        MZ = mz[i],

                        // Overall centroid of translational reactions (scalars – same for all rows)
                        GX = gx,
                        GY = gy,
                        GZ = gz,

                        // Per-row centroids per force component
                        XCentroidForFX = xCFx[i],
                        YCentroidForFX = yCFx[i],
                        ZCentroidForFX = zCFx[i],
                        XCentroidForFY = xCFy[i],
                        YCentroidForFY = yCFy[i],
                        ZCentroidForFY = zCFy[i],
                        XCentroidForFZ = xCFz[i],
                        YCentroidForFZ = yCFz[i],
                        ZCentroidForFZ = zCFz[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {n} base reaction results (with centroids).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetBaseReactions: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Story Drifts

        /// <summary>
        /// Extract story drift results
        /// </summary>
        public List<StoryDriftResult> GetStoryDrifts()
        {
            var results = new List<StoryDriftResult>();

            try
            {
                int numberResults = 0;
                string[] story = null;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                string[] direction = null;
                double[] drift = null;
                string[] label = null;
                double[] x = null, y = null, z = null;

                int ret = _results.StoryDrifts(
                    ref numberResults,
                    ref story,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref direction,
                    ref drift,
                    ref label,
                    ref x, ref y, ref z);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving story drifts. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new StoryDriftResult
                    {
                        Story = story[i],
                        LoadCase = loadCase[i],
                        StepType = stepType[i],
                        StepNum = stepNum[i],
                        Direction = direction[i],
                        Drift = drift[i],
                        Label = label[i],
                        X = x[i],
                        Y = y[i],
                        Z = z[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} story drift results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetStoryDrifts: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Get maximum story drift across all stories and directions
        /// </summary>
        public StoryDriftResult GetMaximumDrift(List<StoryDriftResult> drifts)
        {
            if (drifts == null || !drifts.Any())
                return null;

            return drifts.OrderByDescending(d => Math.Abs(d.Drift)).FirstOrDefault();
        }

        #endregion

        #region Joint Displacements

        /// <summary>
        /// Extract joint displacement results
        /// </summary>
        /// <param name="jointName">Specific joint name or "All" for all joints</param>
        /// <param name="itemType">Object, Group, or Selection</param>
        public List<JointDisplacementResult> GetJointDisplacements(
            string jointName = "All",
            eItemTypeElm itemType = eItemTypeElm.ObjectElm)
        {
            var results = new List<JointDisplacementResult>();

            try
            {
                int numberResults = 0;
                string[] obj = null;
                string[] elm = null;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] u1 = null, u2 = null, u3 = null;
                double[] r1 = null, r2 = null, r3 = null;

                int ret = _results.JointDispl(
                    jointName,
                    itemType,
                    ref numberResults,
                    ref obj,
                    ref elm,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref u1, ref u2, ref u3,
                    ref r1, ref r2, ref r3);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving joint displacements. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new JointDisplacementResult
                    {
                        Joint = obj[i],
                        LoadCase = loadCase[i],
                        StepType = stepType[i],
                        StepNum = stepNum[i],
                        U1 = u1[i],
                        U2 = u2[i],
                        U3 = u3[i],
                        R1 = r1[i],
                        R2 = r2[i],
                        R3 = r3[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} joint displacement results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetJointDisplacements: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Joint Reactions

        /// <summary>
        /// Extract joint reaction results (support reactions at restrained joints)
        /// </summary>
        /// <param name="jointName">Specific joint name or "All" for all joints</param>
        public List<JointReactionResult> GetJointReactions(
            string jointName = "All",
            eItemTypeElm itemType = eItemTypeElm.ObjectElm)
        {
            var results = new List<JointReactionResult>();

            try
            {
                int numberResults = 0;
                string[] obj = null;
                string[] elm = null;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] f1 = null, f2 = null, f3 = null;
                double[] m1 = null, m2 = null, m3 = null;

                int ret = _results.JointReact(
                    jointName,
                    itemType,
                    ref numberResults,
                    ref obj,
                    ref elm,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref f1, ref f2, ref f3,
                    ref m1, ref m2, ref m3);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving joint reactions. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new JointReactionResult
                    {
                        Joint = obj[i],
                        LoadCase = loadCase[i],
                        StepType = stepType[i],
                        StepNum = stepNum[i],
                        F1 = f1[i],
                        F2 = f2[i],
                        F3 = f3[i],
                        M1 = m1[i],
                        M2 = m2[i],
                        M3 = m3[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} joint reaction results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetJointReactions: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Pier Forces

        /// <summary>
        /// Extract pier force results (integrated wall forces per pier label)
        /// Critical for shear wall design - provides P-M diagram inputs
        /// </summary>
        public List<PierForceResult> GetPierForces()
        {
            var results = new List<PierForceResult>();

            try
            {
                int numberResults = 0;
                string[] storyName = null;
                string[] pierName = null;
                string[] loadCase = null;
                string[] location = null;
                double[] p = null, v2 = null, v3 = null;
                double[] t = null, m2 = null, m3 = null;

                int ret = _results.PierForce(
                    ref numberResults,
                    ref storyName,
                    ref pierName,
                    ref loadCase,
                    ref location,
                    ref p, ref v2, ref v3,
                    ref t, ref m2, ref m3);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving pier forces. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new PierForceResult
                    {
                        Story = storyName[i],
                        PierName = pierName[i],
                        LoadCase = loadCase[i],
                        Location = location[i],
                        P = p[i],
                        V2 = v2[i],
                        V3 = v3[i],
                        T = t[i],
                        M2 = m2[i],
                        M3 = m3[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} pier force results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetPierForces: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Get pier forces for a specific pier across all stories
        /// </summary>
        public List<PierForceResult> GetPierForcesByName(string pierName, List<PierForceResult> allResults = null)
        {
            var pierForces = allResults ?? GetPierForces();
            return pierForces.Where(p => p.PierName == pierName).OrderBy(p => p.Story).ToList();
        }

        #endregion

        #region Spandrel Forces

        /// <summary>
        /// Extract spandrel force results (horizontal wall segments, coupling beams)
        /// </summary>
        public List<SpandrelForceResult> GetSpandrelForces()
        {
            var results = new List<SpandrelForceResult>();

            try
            {
                int numberResults = 0;
                string[] storyName = null;
                string[] spandrelName = null;
                string[] loadCase = null;
                string[] location = null;
                double[] p = null, v2 = null, v3 = null;
                double[] t = null, m2 = null, m3 = null;

                int ret = _results.SpandrelForce(
                    ref numberResults,
                    ref storyName,
                    ref spandrelName,
                    ref loadCase,
                    ref location,
                    ref p, ref v2, ref v3,
                    ref t, ref m2, ref m3);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving spandrel forces. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new SpandrelForceResult
                    {
                        Story = storyName[i],
                        SpandrelName = spandrelName[i],
                        LoadCase = loadCase[i],
                        Location = location[i],
                        P = p[i],
                        V2 = v2[i],
                        V3 = v3[i],
                        T = t[i],
                        M2 = m2[i],
                        M3 = m3[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} spandrel force results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetSpandrelForces: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Frame Forces

        /// <summary>
        /// Extract frame force results (beams, columns, braces)
        /// </summary>
        /// <param name="frameName">Specific frame name or "All" for all frames</param>
        /// <param name="itemType">Object, Element, Group, or Selection</param>
        public List<FrameForceResult> GetFrameForces(
            string frameName = "All",
            eItemTypeElm itemType = eItemTypeElm.ObjectElm)
        {
            var results = new List<FrameForceResult>();

            try
            {
                int numberResults = 0;
                string[] obj = null;
                double[] objSta = null;
                string[] elm = null;
                double[] elmSta = null;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] p = null, v2 = null, v3 = null;
                double[] t = null, m2 = null, m3 = null;

                int ret = _results.FrameForce(
                    frameName,
                    itemType,
                    ref numberResults,
                    ref obj,
                    ref objSta,
                    ref elm,
                    ref elmSta,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref p, ref v2, ref v3,
                    ref t, ref m2, ref m3);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving frame forces. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new FrameForceResult
                    {
                        Frame = obj[i],
                        Station = objSta[i],
                        Element = elm[i],
                        ElementStation = elmSta[i],
                        LoadCase = loadCase[i],
                        StepType = stepType[i],
                        StepNum = stepNum[i],
                        P = p[i],
                        V2 = v2[i],
                        V3 = v3[i],
                        T = t[i],
                        M2 = m2[i],
                        M3 = m3[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} frame force results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetFrameForces: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Area Forces

        /// <summary>
        /// Extract area/shell force results (detailed finite element forces)
        /// WARNING: This can return very large datasets for meshed walls/slabs
        /// </summary>
        /// <param name="areaName">Specific area name or "All" for all areas</param>
        /// <param name="itemType">Object, Element, Group, or Selection</param>
        public List<AreaForceResult> GetAreaForces(
            string areaName = "All",
            eItemTypeElm itemType = eItemTypeElm.ObjectElm)
        {
            var results = new List<AreaForceResult>();

            try
            {
                int numberResults = 0;
                string[] obj = null;
                string[] elm = null;
                string[] pointElm = null;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;

                double[] f11 = null, f22 = null, f12 = null;
                double[] fMax = null, fMin = null, fAngle = null, fvm = null;
                double[] m11 = null, m22 = null, m12 = null;
                double[] mMax = null, mMin = null, mAngle = null;
                double[] v13 = null, v23 = null, vMax = null, vAngle = null;

                int ret = _results.AreaForceShell(
                    areaName,
                    itemType,
                    ref numberResults,
                    ref obj,
                    ref elm,
                    ref pointElm,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref f11, ref f22, ref f12,
                    ref fMax, ref fMin, ref fAngle, ref fvm,
                    ref m11, ref m22, ref m12,
                    ref mMax, ref mMin, ref mAngle,
                    ref v13, ref v23, ref vMax, ref vAngle);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving area forces. Return code: {ret}");
                    return results;
                }

                for (int i = 0; i < numberResults; i++)
                {
                    results.Add(new AreaForceResult
                    {
                        Area = obj[i],
                        Element = elm[i],
                        PointElm = pointElm[i],
                        LoadCase = loadCase[i],
                        StepType = stepType[i],
                        StepNum = stepNum[i],
                        F11 = f11[i],
                        F22 = f22[i],
                        F12 = f12[i],
                        FMax = fMax[i],
                        FMin = fMin[i],
                        FAngle = fAngle[i],
                        FVM = fvm[i],
                        M11 = m11[i],
                        M22 = m22[i],
                        M12 = m12[i],
                        MMax = mMax[i],
                        MMin = mMin[i],
                        MAngle = mAngle[i],
                        V13 = v13[i],
                        V23 = v23[i],
                        VMax = vMax[i],
                        VAngle = vAngle[i]
                    });
                }

                Console.WriteLine($"✓ Extracted {numberResults} area force results");

                if (numberResults > 1000)
                {
                    Console.WriteLine($"⚠ Large dataset extracted ({numberResults} results). Consider filtering by specific areas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetAreaForces: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Modal Results

        /// <summary>
        /// Extract modal period and frequency results with mass participation ratios
        /// </summary>
        public List<ModalResult> GetModalResults()
        {
            var results = new List<ModalResult>();

            try
            {
                // --- 1) Modal periods/summary ---
                int numberResults = 0;
                string[] loadCase = null;
                string[] stepType = null;
                double[] stepNum = null;
                double[] period = null;
                double[] frequency = null;
                double[] circFreq = null;
                double[] eigenvalue = null;

                int ret = _results.ModalPeriod(
                    ref numberResults,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref period,
                    ref frequency,
                    ref circFreq,
                    ref eigenvalue);

                if (ret != 0)
                {
                    Console.WriteLine($"❌ Error retrieving modal periods. Return code: {ret}");
                    return results;
                }

                // Build initial list and a lookup (case, mode) -> index
                var indexByCaseMode = new Dictionary<(string Case, int Mode), int>((IDictionary<(string Case, int Mode), int>)StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < numberResults; i++)
                {
                    int mode = (int)stepNum[i];
                    results.Add(new ModalResult
                    {
                        Mode = mode,
                        LoadCase = loadCase[i],
                        Period = period[i],
                        Frequency = frequency[i],
                        CircFreq = circFreq[i],
                        Eigenvalue = eigenvalue[i]
                    });
                    indexByCaseMode[(loadCase[i], mode)] = i;
                }

                // --- 2) Mass participation ratios (REQUIRES period parameter) ---
                numberResults = 0;
                loadCase = null;
                stepType = null;
                stepNum = null;

                // required by signature (appears after stepNum)
                double[] period2 = null;

                double[] ux = null, uy = null, uz = null;
                double[] sumUx = null, sumUy = null, sumUz = null;
                double[] rx = null, ry = null, rz = null;
                double[] sumRx = null, sumRy = null, sumRz = null;

                ret = _results.ModalParticipatingMassRatios(
                    ref numberResults,
                    ref loadCase,
                    ref stepType,
                    ref stepNum,
                    ref period2,            // <-- this was missing
                    ref ux, ref uy, ref uz,
                    ref sumUx, ref sumUy, ref sumUz,
                    ref rx, ref ry, ref rz,
                    ref sumRx, ref sumRy, ref sumRz);

                if (ret != 0)
                {
                    Console.WriteLine($"⚠ Warning: Could not retrieve mass participation ratios. Return code: {ret}");
                }
                else
                {
                    // Merge by (case, mode) to be robust against ordering differences
                    for (int i = 0; i < numberResults; i++)
                    {
                        int mode = (int)stepNum[i];
                        if (indexByCaseMode.TryGetValue((loadCase[i], mode), out int idx))
                        {
                            results[idx].UX = ux[i];
                            results[idx].UY = uy[i];
                            results[idx].UZ = uz[i];
                            results[idx].RX = rx[i];
                            results[idx].RY = ry[i];
                            results[idx].RZ = rz[i];
                            results[idx].SumUX = sumUx[i];
                            results[idx].SumUY = sumUy[i];
                            results[idx].SumUZ = sumUz[i];
                            results[idx].SumRX = sumRx[i];
                            results[idx].SumRY = sumRy[i];
                            results[idx].SumRZ = sumRz[i];
                        }
                    }
                }

                Console.WriteLine($"✓ Extracted {results.Count} modal results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in GetModalResults: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Check if sufficient modes capture the required mass participation
        /// </summary>
        /// <param name="results">Modal results</param>
        /// <param name="requiredParticipation">Required participation (e.g., 0.90 for 90%)</param>
        /// <param name="direction">"X", "Y", or "Z"</param>
        public bool CheckMassParticipation(List<ModalResult> results, double requiredParticipation = 0.90, string direction = "X")
        {
            if (results == null || !results.Any())
                return false;

            var lastMode = results.OrderBy(r => r.Mode).Last();

            double cumulative = direction.ToUpper() switch
            {
                "X" => lastMode.SumUX,
                "Y" => lastMode.SumUY,
                "Z" => lastMode.SumUZ,
                _ => 0
            };

            bool sufficient = cumulative >= requiredParticipation;

            Console.WriteLine($"Mass participation in {direction}: {cumulative * 100:F1}% " +
                            $"({(sufficient ? "✓ Sufficient" : "⚠ Insufficient")})");

            return sufficient;
        }

        #endregion

        #region Comprehensive Extraction

        /// <summary>
        /// Extract all common results in one call
        /// </summary>
        /// <param name="includeModal">Whether to include modal results</param>
        /// <param name="includeAreaForces">Whether to include detailed area forces (can be large)</param>
        /// <param name="includeFrameForces">Whether to include frame forces</param>
        /// <param name="includeJointDisplacements">Whether to include joint displacements</param>
        /// <param name="includeJointReactions">Whether to include joint reactions</param>
        public AnalysisResults GetAllResults(
            bool includeModal = true,
            bool includeAreaForces = false,
            bool includeFrameForces = false,
            bool includeJointDisplacements = false,
            bool includeJointReactions = false)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("    EXTRACTING ANALYSIS RESULTS");
            Console.WriteLine("========================================\n");

            var allResults = new AnalysisResults();

            // Get current units
            eUnits units = eUnits.kN_m_C;
            _model.GetPresentUnits();
            allResults.Units = units.ToString().Replace("_", ", ");

            // Get model filename if available
            try
            {
                string modelPath = _model.GetModelFilename(true);
                allResults.ModelName = System.IO.Path.GetFileNameWithoutExtension(modelPath);
            }
            catch { }

            // Extract core results (always included)
            Console.WriteLine("Extracting base reactions...");
            allResults.BaseReactions = GetBaseReactions();

            Console.WriteLine("Extracting story drifts...");
            allResults.StoryDrifts = GetStoryDrifts();

            Console.WriteLine("Extracting pier forces...");
            allResults.PierForces = GetPierForces();

            Console.WriteLine("Extracting spandrel forces...");
            allResults.SpandrelForces = GetSpandrelForces();

            // Optional results
            if (includeModal)
            {
                Console.WriteLine("Extracting modal results...");
                allResults.ModalResults = GetModalResults();
            }

            if (includeFrameForces)
            {
                Console.WriteLine("Extracting frame forces...");
                allResults.FrameForces = GetFrameForces();
            }

            if (includeAreaForces)
            {
                Console.WriteLine("Extracting area forces (this may take a while)...");
                allResults.AreaForces = GetAreaForces();
            }

            if (includeJointDisplacements)
            {
                Console.WriteLine("Extracting joint displacements...");
                allResults.JointDisplacements = GetJointDisplacements();
            }

            if (includeJointReactions)
            {
                Console.WriteLine("Extracting joint reactions...");
                allResults.JointReactions = GetJointReactions();
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("    RESULTS EXTRACTION COMPLETE");
            Console.WriteLine("========================================\n");

            return allResults;
        }

        #endregion

        #region Result Summary and Reporting

        /// <summary>
        /// Print a comprehensive summary of extracted results
        /// </summary>
        public void PrintResultsSummary(AnalysisResults results)
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ANALYSIS RESULTS SUMMARY                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"Model: {results.ModelName ?? "Unknown"}");
            Console.WriteLine($"Extraction Time: {results.ExtractionTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Units: {results.Units}\n");

            // Base Reactions
            if (results.BaseReactions.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine("BASE REACTIONS:");
                Console.WriteLine("─────────────────────────────────────────────────────────────");

                foreach (var reaction in results.BaseReactions)
                {
                    Console.WriteLine($"\n  Load Case: {reaction.LoadCase}");
                    Console.WriteLine($"    FX = {reaction.FX,10:F2} kN    MX = {reaction.MX,10:F2} kN·m");
                    Console.WriteLine($"    FY = {reaction.FY,10:F2} kN    MY = {reaction.MY,10:F2} kN·m");
                    Console.WriteLine($"    FZ = {reaction.FZ,10:F2} kN    MZ = {reaction.MZ,10:F2} kN·m");
                }
                Console.WriteLine();
            }

            // Story Drifts
            if (results.StoryDrifts.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine("STORY DRIFTS:");
                Console.WriteLine("─────────────────────────────────────────────────────────────");

                var driftsByCase = results.StoryDrifts.GroupBy(d => d.LoadCase);

                foreach (var caseGroup in driftsByCase)
                {
                    Console.WriteLine($"\n  Load Case: {caseGroup.Key}");

                    var maxDriftX = caseGroup.Where(d => d.Direction == "X").OrderByDescending(d => Math.Abs(d.Drift)).FirstOrDefault();
                    var maxDriftY = caseGroup.Where(d => d.Direction == "Y").OrderByDescending(d => Math.Abs(d.Drift)).FirstOrDefault();

                    if (maxDriftX != null)
                    {
                        Console.WriteLine($"    Max Drift X: {maxDriftX.Drift * 100,8:F4}% at {maxDriftX.Story}");
                    }

                    if (maxDriftY != null)
                    {
                        Console.WriteLine($"    Max Drift Y: {maxDriftY.Drift * 100,8:F4}% at {maxDriftY.Story}");
                    }
                }
                Console.WriteLine();
            }

            // Pier Forces Summary
            if (results.PierForces.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine("PIER FORCES (Sample - First 5 results per case):");
                Console.WriteLine("─────────────────────────────────────────────────────────────");

                var piersByCase = results.PierForces.GroupBy(p => p.LoadCase);

                foreach (var caseGroup in piersByCase)
                {
                    Console.WriteLine($"\n  Load Case: {caseGroup.Key}");

                    foreach (var pier in caseGroup.Take(5))
                    {
                        Console.WriteLine($"    {pier.PierName} @ {pier.Story} ({pier.Location}):");
                        Console.WriteLine($"      P  = {pier.P,10:F1} kN    M2 = {pier.M2,10:F1} kN·m");
                        Console.WriteLine($"      V2 = {pier.V2,10:F1} kN    M3 = {pier.M3,10:F1} kN·m");
                        Console.WriteLine($"      V3 = {pier.V3,10:F1} kN    T  = {pier.T,10:F1} kN·m");
                    }

                    if (caseGroup.Count() > 5)
                    {
                        Console.WriteLine($"    ... and {caseGroup.Count() - 5} more pier results for this case");
                    }
                }
                Console.WriteLine();
            }

            // Spandrel Forces Summary
            if (results.SpandrelForces.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"SPANDREL FORCES: {results.SpandrelForces.Count} results extracted");
                Console.WriteLine("─────────────────────────────────────────────────────────────\n");
            }

            // Modal Results
            if (results.ModalResults.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine("MODAL ANALYSIS:");
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"\n  {"Mode",-6} {"Period",-10} {"Freq",-10} {"UX%",-8} {"UY%",-8} {"ΣUX%",-8} {"ΣUY%",-8}");
                Console.WriteLine("  " + new string('─', 60));

                foreach (var mode in results.ModalResults.Take(12))
                {
                    Console.WriteLine($"  {mode.Mode,-6} {mode.Period,-10:F4} {mode.Frequency,-10:F4} " +
                                    $"{mode.UX * 100,-8:F2} {mode.UY * 100,-8:F2} " +
                                    $"{mode.SumUX * 100,-8:F1} {mode.SumUY * 100,-8:F1}");
                }

                if (results.ModalResults.Count > 12)
                {
                    Console.WriteLine($"\n  ... and {results.ModalResults.Count - 12} more modes");
                }

                // Check mass participation adequacy
                Console.WriteLine();
                CheckMassParticipation(results.ModalResults, 0.90, "X");
                CheckMassParticipation(results.ModalResults, 0.90, "Y");
                Console.WriteLine();
            }

            // Summary counts for other results
            if (results.FrameForces.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"FRAME FORCES: {results.FrameForces.Count} results extracted");
                Console.WriteLine("─────────────────────────────────────────────────────────────\n");
            }

            if (results.AreaForces.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"AREA FORCES: {results.AreaForces.Count} results extracted");
                Console.WriteLine("─────────────────────────────────────────────────────────────\n");
            }

            if (results.JointDisplacements.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"JOINT DISPLACEMENTS: {results.JointDisplacements.Count} results extracted");
                Console.WriteLine("─────────────────────────────────────────────────────────────\n");
            }

            if (results.JointReactions.Any())
            {
                Console.WriteLine("─────────────────────────────────────────────────────────────");
                Console.WriteLine($"JOINT REACTIONS: {results.JointReactions.Count} results extracted");
                Console.WriteLine("─────────────────────────────────────────────────────────────\n");
            }

            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  END OF SUMMARY                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        }

        /// <summary>
        /// Print simplified summary with just counts
        /// </summary>
        public void PrintQuickSummary(AnalysisResults results)
        {
            Console.WriteLine("\n=== Quick Results Summary ===");
            Console.WriteLine(results.GetSummary());
            Console.WriteLine("============================\n");
        }

        #endregion
    }
}
