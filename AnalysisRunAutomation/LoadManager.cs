using System;
using System.Collections.Generic;
using ETABSv1;

namespace ETABS_Plugin
{
    public class LoadManager
    {
        private cSapModel _SapModel;

        public LoadManager(cSapModel sapModel)
        {
            _SapModel = sapModel;
        }

        #region Load Patterns

        /// <summary>
        /// Creates standard load patterns with self-weight multipliers
        /// </summary>
        public bool CreateStandardLoadPatterns()
        {
            var loadPatterns = new Dictionary<string, Tuple<eLoadPatternType, double, string>>
    {
        {"DEAD", new Tuple<eLoadPatternType, double, string>(eLoadPatternType.Dead, 1.0, "PLUGIN_DEAD")},
        {"LIVE", new Tuple<eLoadPatternType, double, string>(eLoadPatternType.Live, 0.0, "PLUGIN_LIVE")},
        {"WIND", new Tuple<eLoadPatternType, double, string>(eLoadPatternType.Wind, 0.0, "PLUGIN_WIND")},
        {"SEISMIC", new Tuple<eLoadPatternType, double, string>(eLoadPatternType.Quake, 0.0, "PLUGIN_SEISMIC")}
    };

            int created = 0;
            int skipped = 0;

            foreach (var pattern in loadPatterns)
            {
                try
                {
                    string patternName = pattern.Value.Item3;

                    // Check if pattern already exists
                    if (LoadPatternExists(patternName))
                    {
                        skipped++;
                        continue; // Skip existing pattern
                    }

                    // Delete if exists (redundant but safe)
                    try { _SapModel.LoadPatterns.Delete(patternName); } catch { }

                    int result = _SapModel.LoadPatterns.Add(
                        patternName,
                        pattern.Value.Item1,
                        pattern.Value.Item2,
                        true
                    );

                    if (result != 0)
                    {
                        System.Windows.Forms.MessageBox.Show($"Failed to create {patternName}, error: {result}");
                        return false;
                    }
                    created++;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Exception: {ex.Message}");
                    return false;
                }
            }

            if (skipped > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Load patterns: {created} created, {skipped} already existed",
                    "Load Pattern Creation",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            return true;
        }
        /// <summary>
        /// Creates additional load patterns (temperature, construction loads, etc.)
        /// </summary>
        public bool CreateAdditionalLoadPatterns()
        {
            var additionalPatterns = new Dictionary<string, Tuple<eLoadPatternType, double>>
            {
                {"TEMP", new Tuple<eLoadPatternType, double>(eLoadPatternType.Temperature, 0.0)},
                {"CONSTR", new Tuple<eLoadPatternType, double>(eLoadPatternType.Construction, 0.0)},
                {"SUPERDEAD", new Tuple<eLoadPatternType, double>(eLoadPatternType.SuperDead, 0.0)}
            };

            foreach (var pattern in additionalPatterns)
            {
                try
                {
                    int result = _SapModel.LoadPatterns.Add(
                        pattern.Key,
                        pattern.Value.Item1,
                        pattern.Value.Item2,
                        false  // Don't auto-create analysis case for these
                    );

                    if (result != 0) return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Load Combinations

        /// <summary>
        /// Creates ultimate limit state combinations (general approach)
        /// </summary>
        public bool CreateUltimateLoadCombinations()
        {
            var ultimateCombos = new Dictionary<string, Dictionary<string, double>>
    {
        {"ULS-1", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.35}, {"PLUGIN_LIVE", 1.5}}},
        {"ULS-2", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.2}, {"PLUGIN_LIVE", 1.6}}},
        {"ULS-WIND-1", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.2}, {"PLUGIN_WIND", 1.6}}},
        {"ULS-WIND-2", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.0}, {"PLUGIN_WIND", 1.4}, {"PLUGIN_LIVE", 0.5}}},
        {"ULS-SEISMIC", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.0}, {"PLUGIN_SEISMIC", 1.0}, {"PLUGIN_LIVE", 0.3}}}
    };

            return CreateLoadCombinations(ultimateCombos, 0);
        }

        /// <summary>
        /// Creates serviceability limit state combinations
        /// </summary>
        public bool CreateServiceabilityLoadCombinations()
        {
            var serviceCombos = new Dictionary<string, Dictionary<string, double>>
            {
                {"SLS-RARE", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.0}, {"PLUGIN_LIVE", 1.0}}},
                {"SLS-FREQUENT", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.0}, {"PLUGIN_LIVE", 0.7}}},
                {"SLS-QUASI", new Dictionary<string, double> {{"DEAD", 1.0}, {"LIVE", 0.6}}},
                {"SLS-WIND", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.0}, {"PLUGIN_WIND", 1.0}}},
                {"SLS-SEISMIC", new Dictionary<string, double> {{"PLUGIN_DEAD", 1.0}, {"PLUGIN_SEISMIC", 1.0}}}
            };

            return CreateLoadCombinations(serviceCombos, 0); // 0 = Linear additive
        }

        /// <summary>
        /// Creates envelope combinations for design
        /// </summary>
        public bool CreateEnvelopeCombinations()
        {
            try
            {
                // ULS Envelope - combines all ultimate combinations
                var ulsEnvelope = new Dictionary<string, double>
                {
                    {"ULS-1", 1.0},
                    {"ULS-2", 1.0},
                    {"ULS-WIND-1", 1.0},
                    {"ULS-WIND-2", 1.0},
                    {"ULS-SEISMIC", 1.0}
                };

                var slsEnvelope = new Dictionary<string, double>
                {
                    {"SLS-RARE", 1.0},
                    {"SLS-FREQUENT", 1.0},
                    {"SLS-QUASI", 1.0},
                    {"SLS-WIND", 1.0},
                    {"SLS-SEISMIC", 1.0}
                };

                var envelopes = new Dictionary<string, Dictionary<string, double>>
                {
                    {"ULS-ENVELOPE", ulsEnvelope},
                    {"SLS-ENVELOPE", slsEnvelope}
                };

                return CreateLoadCombinations(envelopes, 1, true); // 1 = Envelope, true = reference combos
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Generic method to create load combinations using the actual ETABS API
        /// </summary>
        private bool CreateLoadCombinations(Dictionary<string, Dictionary<string, double>> combinations, int comboType = 0, bool referenceCombos = false)
        {
            int created = 0;
            int skipped = 0;

            foreach (var combo in combinations)
            {
                try
                {
                    // Check if combination already exists
                    if (LoadComboExists(combo.Key))
                    {
                        skipped++;
                        continue; // Skip existing combination
                    }

                    // Delete if exists (redundant but safe)
                    try { _SapModel.RespCombo.Delete(combo.Key); } catch { }

                    // Create the combination with the specified type
                    int result = _SapModel.RespCombo.Add(combo.Key, comboType);

                    if (result != 0)
                    {
                        System.Windows.Forms.MessageBox.Show($"FAILED to create combo {combo.Key}, error: {result}");
                        return false;
                    }

                    // Add each load case/combination to this combination using SetCaseList
                    foreach (var loadCase in combo.Value)
                    {
                        eCNameType nameType = referenceCombos ? eCNameType.LoadCombo : eCNameType.LoadCase;
                        result = _SapModel.RespCombo.SetCaseList(
                            combo.Key,
                            ref nameType,
                            loadCase.Key,
                            loadCase.Value
                        );

                        if (result != 0)
                        {
                            System.Windows.Forms.MessageBox.Show($"FAILED SetCaseList for {loadCase.Key} in {combo.Key}, error: {result}");
                            return false;
                        }
                    }
                    created++;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Exception creating combination {combo.Key}: {ex.Message}");
                    return false;
                }
            }

            if (skipped > 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Load combinations: {created} created, {skipped} already existed",
                    "Load Combination Creation",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            return true;
        }
        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets list of existing load patterns
        /// </summary>
        public string[] GetExistingLoadPatterns()
        {
            try
            {
                int n = 0;
                string[] names = new string[0];
                _SapModel.LoadPatterns.GetNameList(ref n, ref names);
                return names;
            }
            catch (Exception ex)
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Gets list of existing load combinations
        /// </summary>
        public string[] GetExistingLoadCombinations()
        {
            try
            {
                int n = 0;
                string[] combos = new string[0];
                _SapModel.RespCombo.GetNameList(ref n, ref combos);
                return combos;
            }
            catch (Exception ex)
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Deletes specific load patterns and combinations by name
        /// </summary>
        public bool DeleteLoadItems(string[] patterns = null, string[] combinations = null)
        {
            try
            {
                // Delete specified combinations
                if (combinations != null)
                {
                    foreach (var combo in combinations)
                    {
                        _SapModel.RespCombo.Delete(combo);
                    }
                }

                // Delete specified patterns
                if (patterns != null)
                {
                    foreach (var pattern in patterns)
                    {
                        _SapModel.LoadPatterns.Delete(pattern);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Main Creation Method

        /// <summary>
        /// Creates complete set of standard loads and combinations
        /// </summary>
        public bool CreateAllStandardLoads()
        {
            try
            {
                bool overallSuccess = true;

                System.Windows.Forms.MessageBox.Show("Testing all load creation methods...");

                // Test each method independently - don't return early
                bool patterns = CreateStandardLoadPatterns();
                if (!patterns)
                {
                    System.Windows.Forms.MessageBox.Show("FAILED: CreateStandardLoadPatterns");
                    overallSuccess = false;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("SUCCESS: CreateStandardLoadPatterns");
                }

                bool ultimate = CreateUltimateLoadCombinations();
                if (!ultimate)
                {
                    System.Windows.Forms.MessageBox.Show("FAILED: CreateUltimateLoadCombinations");
                    overallSuccess = false;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("SUCCESS: CreateUltimateLoadCombinations");
                }

                bool serviceability = CreateServiceabilityLoadCombinations();
                if (!serviceability)
                {
                    System.Windows.Forms.MessageBox.Show("FAILED: CreateServiceabilityLoadCombinations");
                    overallSuccess = false;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("SUCCESS: CreateServiceabilityLoadCombinations");
                }

                bool envelope = CreateEnvelopeCombinations();
                if (!envelope)
                {
                    System.Windows.Forms.MessageBox.Show("FAILED: CreateEnvelopeCombinations");
                    overallSuccess = false;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("SUCCESS: CreateEnvelopeCombinations");
                }

                return overallSuccess;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"EXCEPTION in CreateAllStandardLoads: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Existence Checks

        /// <summary>
        /// Checks if a load pattern already exists
        /// </summary>
        private bool LoadPatternExists(string patternName)
        {
            try
            {
                int numPatterns = 0;
                string[] patternNames = Array.Empty<string>();
                _SapModel.LoadPatterns.GetNameList(ref numPatterns, ref patternNames);

                foreach (var name in patternNames)
                {
                    if (name.Equals(patternName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a load combination already exists
        /// </summary>
        private bool LoadComboExists(string comboName)
        {
            try
            {
                int numCombos = 0;
                string[] comboNames = Array.Empty<string>();
                _SapModel.RespCombo.GetNameList(ref numCombos, ref comboNames);

                foreach (var name in comboNames)
                {
                    if (name.Equals(comboName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}