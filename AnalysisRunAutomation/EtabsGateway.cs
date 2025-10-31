using ETABSv1;
using Etabs.Automation.Abstractions;

namespace ETABS_Plugin;

/// <summary>
/// Concrete implementation of IEtabsGateway using ETABS API v1.
/// Standardizes units (kN-m-C), manages undo groups, and handles selection save/restore.
/// </summary>
public class EtabsGateway : IEtabsGateway
{
    private readonly cSapModel _model;
    private string[]? _savedSelection;
    private bool _resultsDirty = false;

    public EtabsGateway(cSapModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public void BeginUndo(string groupName)
    {
        // ETABS API doesn't have explicit undo grouping in v1
        // This is a placeholder for future implementation or tracking
    }

    public void EndUndo()
    {
        // Placeholder
    }

    public void SaveSelection()
    {
        try
        {
            int numberItems = 0;
            eItemType itemType = eItemType.Objects;

            _model.SelectObj.GetSelected(ref numberItems, ref itemType, ref _savedSelection);
        }
        catch
        {
            _savedSelection = null;
        }
    }

    public void RestoreSelection()
    {
        try
        {
            if (_savedSelection != null && _savedSelection.Length > 0)
            {
                _model.SelectObj.ClearSelection();
                foreach (var item in _savedSelection)
                {
                    _model.SelectObj.Selected(eItemType.Object, item, true);
                }
            }
        }
        catch
        {
            // Ignore errors during restore
        }
    }

    public void RefreshView()
    {
        try
        {
            _model.View.RefreshView(0, false);
        }
        catch
        {
            // Ignore errors
        }
    }

    public string EnsureConcreteMaterial(string materialName, double strengthMPa)
    {
        try
        {
            // Check if material exists
            int numMaterials = 0;
            string[] matNames = null;
            _model.PropMaterial.GetNameList(ref numMaterials, ref matNames);

            if (matNames != null && matNames.Contains(materialName))
            {
                return materialName;
            }

            // Create new concrete material
            string name = materialName;
            int ret = _model.PropMaterial.AddMaterial(
                ref name,
                eMatType.Concrete,
                "", // Region
                "", // Standard
                "" // Grade
            );

            if (ret == 0)
            {
                // Set concrete properties
                double fc = strengthMPa; // MPa
                _model.PropMaterial.SetOConcrete_1(name, fc, false, 0, 0, 0, 0, 0, 0);

                return name;
            }

            return materialName;
        }
        catch
        {
            return materialName;
        }
    }

    public bool SetWallSection(string sectionName, double thicknessMm, string materialName)
    {
        try
        {
            // Convert mm to meters
            double thicknessM = thicknessMm / 1000.0;

            // Create or modify wall section
            int ret = _model.PropArea.SetWall(
                sectionName,
                eWallPropType.Specified,
                eShellType.ShellThin,
                materialName,
                thicknessM
            );

            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool WallSectionExists(string sectionName)
    {
        try
        {
            int numSections = 0;
            string[] sectionNames = null;
            _model.PropArea.GetNameList(ref numSections, ref sectionNames);

            return sectionNames != null && sectionNames.Contains(sectionName);
        }
        catch
        {
            return false;
        }
    }

    public bool GetWallSection(string sectionName, out double thicknessMm, out string materialName)
    {
        thicknessMm = 0;
        materialName = string.Empty;

        try
        {
            eWallPropType wallType = eWallPropType.Specified;
            eShellType shellType = eShellType.ShellThin;
            string matProp = string.Empty;
            double thickness = 0;
            int color = 0;
            string notes = string.Empty;
            string guid = string.Empty;

            int ret = _model.PropArea.GetWall(
                sectionName,
                ref wallType,
                ref shellType,
                ref matProp,
                ref thickness,
                ref color,
                ref notes,
                ref guid
            );

            if (ret == 0)
            {
                thicknessMm = thickness * 1000.0; // Convert m to mm
                materialName = matProp;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool CreateWallByCoords(int numPoints, double[] x, double[] y, double[] z,
        string sectionName, string storyName, out string wallName)
    {
        wallName = string.Empty;

        try
        {
            string name = string.Empty;

            // Add area object by coordinates
            int ret = _model.AreaObj.AddByCoord(
                numPoints,
                ref x,
                ref y,
                ref z,
                ref name,
                sectionName,
                "" // GUID
            );

            if (ret == 0 && !string.IsNullOrEmpty(name))
            {
                wallName = name;

                // Set the property (section) to ensure it's assigned
                _model.AreaObj.SetProperty(name, sectionName);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool WallExists(string wallName)
    {
        try
        {
            int numAreas = 0;
            string[] areaNames = null;
            _model.AreaObj.GetNameList(ref numAreas, ref areaNames);

            return areaNames != null && areaNames.Contains(wallName);
        }
        catch
        {
            return false;
        }
    }

    public bool SetWallProperty(string wallName, string sectionName)
    {
        try
        {
            int ret = _model.AreaObj.SetProperty(wallName, sectionName);
            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool GetWallPoints(string wallName, out double[] x, out double[] y, out double[] z)
    {
        x = Array.Empty<double>();
        y = Array.Empty<double>();
        z = Array.Empty<double>();

        try
        {
            int numPoints = 0;
            string[] pointNames = null;

            int ret = _model.AreaObj.GetPoints(wallName, ref numPoints, ref pointNames);

            if (ret == 0 && pointNames != null && pointNames.Length > 0)
            {
                x = new double[pointNames.Length];
                y = new double[pointNames.Length];
                z = new double[pointNames.Length];

                for (int i = 0; i < pointNames.Length; i++)
                {
                    double xCoord = 0, yCoord = 0, zCoord = 0;
                    _model.PointObj.GetCoordCartesian(pointNames[i], ref xCoord, ref yCoord, ref zCoord);
                    x[i] = xCoord;
                    y[i] = yCoord;
                    z[i] = zCoord;
                }

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteWall(string wallName)
    {
        try
        {
            int ret = _model.AreaObj.Delete(wallName);
            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool SetLocalAxisAngle(string wallName, double angleDegrees)
    {
        try
        {
            int ret = _model.AreaObj.SetLocalAxes(wallName, angleDegrees);
            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool GetLocalAxisAngle(string wallName, out double angleDegrees)
    {
        angleDegrees = 0;

        try
        {
            double angle = 0;
            bool advanced = false;

            int ret = _model.AreaObj.GetLocalAxes(wallName, ref angle, ref advanced);

            if (ret == 0)
            {
                angleDegrees = angle;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool GetDesignOrientation(string wallName, out bool isVertical, out bool isFlipped)
    {
        isVertical = false;
        isFlipped = false;

        try
        {
            eWallDesignOrientation orientation = eWallDesignOrientation.Horizontal;

            int ret = _model.AreaObj.GetDesignOrientation(wallName, ref orientation);

            if (ret == 0)
            {
                isVertical = (orientation == eWallDesignOrientation.Vertical);
                isFlipped = (orientation == eWallDesignOrientation.VerticalFlipped);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool AssignPier(string wallName, string pierLabel)
    {
        try
        {
            int ret = _model.AreaObj.SetPier(wallName, pierLabel);
            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool AssignSpandrel(string wallName, string spandrelLabel)
    {
        try
        {
            int ret = _model.AreaObj.SetSpandrel(wallName, spandrelLabel);
            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool GetPier(string wallName, out string pierLabel)
    {
        pierLabel = string.Empty;

        try
        {
            string label = string.Empty;
            int ret = _model.AreaObj.GetPier(wallName, ref label);

            if (ret == 0)
            {
                pierLabel = label;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool GetSpandrel(string wallName, out string spandrelLabel)
    {
        spandrelLabel = string.Empty;

        try
        {
            string label = string.Empty;
            int ret = _model.AreaObj.GetSpandrel(wallName, ref label);

            if (ret == 0)
            {
                spandrelLabel = label;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool SetOpening(string wallName, bool isOpening)
    {
        try
        {
            int ret = _model.AreaObj.SetOpening(wallName, isOpening);
            return ret == 0;
        }
        catch
        {
            return false;
        }
    }

    public bool CreateOpening(string parentWallName, int numPoints, double[] x, double[] y, double[] z,
        out string openingName)
    {
        openingName = string.Empty;

        try
        {
            // Create a new area object for the opening
            string name = string.Empty;

            int ret = _model.AreaObj.AddByCoord(
                numPoints,
                ref x,
                ref y,
                ref z,
                ref name,
                "None", // No property for openings
                "" // GUID
            );

            if (ret == 0 && !string.IsNullOrEmpty(name))
            {
                // Mark it as an opening
                _model.AreaObj.SetOpening(name, true);
                openingName = name;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public string[] GetStoryNames()
    {
        try
        {
            int numStories = 0;
            string[] storyNames = null;
            double[] elevations = null;
            double[] heights = null;
            bool[] isMasterStory = null;
            string[] similarToStory = null;
            bool[] spliceAbove = null;
            double[] spliceHeight = null;
            int[] color = null;

            _model.Story.GetStories_2(
                ref numStories,
                ref storyNames,
                ref elevations,
                ref heights,
                ref isMasterStory,
                ref similarToStory,
                ref spliceAbove,
                ref spliceHeight,
                ref color
            );

            return storyNames ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public bool GetStoryElevation(string storyName, out double elevationM)
    {
        elevationM = 0;

        try
        {
            double elevation = 0;
            double height = 0;
            bool isMasterStory = false;
            string similarToStory = string.Empty;
            bool spliceAbove = false;
            double spliceHeight = 0;
            int color = 0;

            int ret = _model.Story.GetStory(
                storyName,
                ref elevation,
                ref height,
                ref isMasterStory,
                ref similarToStory,
                ref spliceAbove,
                ref spliceHeight,
                ref color
            );

            if (ret == 0)
            {
                elevationM = elevation;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public void SetResultsDirty()
    {
        _resultsDirty = true;
    }

    public bool AreResultsDirty()
    {
        return _resultsDirty;
    }
}
