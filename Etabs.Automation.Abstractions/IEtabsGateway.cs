namespace Etabs.Automation.Abstractions;

/// <summary>
/// Abstraction over ETABS API for wall and area object operations.
/// Centralizes units (kN-m-C standard), undo grouping, selection management, and DB-table quirks.
/// </summary>
public interface IEtabsGateway
{
    /// <summary>
    /// Begins an undo group. All subsequent operations will be grouped into a single undo operation.
    /// </summary>
    /// <param name="groupName">Name of the undo group</param>
    void BeginUndo(string groupName);

    /// <summary>
    /// Ends the current undo group.
    /// </summary>
    void EndUndo();

    /// <summary>
    /// Saves the current user selection so it can be restored later.
    /// </summary>
    void SaveSelection();

    /// <summary>
    /// Restores the previously saved user selection.
    /// </summary>
    void RestoreSelection();

    /// <summary>
    /// Refreshes the ETABS view to display the latest changes.
    /// </summary>
    void RefreshView();

    // ====================
    // Material operations
    // ====================

    /// <summary>
    /// Gets or creates a concrete material with specified strength.
    /// </summary>
    /// <param name="materialName">Desired material name (may be modified by ETABS)</param>
    /// <param name="strengthMPa">Concrete compressive strength in MPa</param>
    /// <returns>Actual material name created/found</returns>
    string EnsureConcreteMaterial(string materialName, double strengthMPa);

    // ====================
    // Wall section (property) operations
    // ====================

    /// <summary>
    /// Creates or updates a wall area section property.
    /// </summary>
    /// <param name="sectionName">Name of the wall section</param>
    /// <param name="thicknessMm">Thickness in millimeters</param>
    /// <param name="materialName">Material name</param>
    /// <returns>True if successful</returns>
    bool SetWallSection(string sectionName, double thicknessMm, string materialName);

    /// <summary>
    /// Checks if a wall section property exists.
    /// </summary>
    /// <param name="sectionName">Name of the wall section</param>
    /// <returns>True if the section exists</returns>
    bool WallSectionExists(string sectionName);

    /// <summary>
    /// Gets wall section properties.
    /// </summary>
    /// <param name="sectionName">Name of the wall section</param>
    /// <param name="thicknessMm">Output: thickness in millimeters</param>
    /// <param name="materialName">Output: material name</param>
    /// <returns>True if successful</returns>
    bool GetWallSection(string sectionName, out double thicknessMm, out string materialName);

    // ====================
    // Wall area object operations
    // ====================

    /// <summary>
    /// Creates a new wall area object by coordinates.
    /// </summary>
    /// <param name="numPoints">Number of corner points</param>
    /// <param name="x">X coordinates (meters)</param>
    /// <param name="y">Y coordinates (meters)</param>
    /// <param name="z">Z coordinates (meters)</param>
    /// <param name="sectionName">Wall section property name</param>
    /// <param name="storyName">Story name for the wall</param>
    /// <param name="wallName">Output: name of the created wall</param>
    /// <returns>True if successful</returns>
    bool CreateWallByCoords(int numPoints, double[] x, double[] y, double[] z,
        string sectionName, string storyName, out string wallName);

    /// <summary>
    /// Checks if an area object (wall) exists.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <returns>True if the wall exists</returns>
    bool WallExists(string wallName);

    /// <summary>
    /// Updates the property (section) assigned to a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="sectionName">New section property name</param>
    /// <returns>True if successful</returns>
    bool SetWallProperty(string wallName, string sectionName);

    /// <summary>
    /// Gets the corner points of a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="x">Output: X coordinates</param>
    /// <param name="y">Output: Y coordinates</param>
    /// <param name="z">Output: Z coordinates</param>
    /// <returns>True if successful</returns>
    bool GetWallPoints(string wallName, out double[] x, out double[] y, out double[] z);

    /// <summary>
    /// Deletes a wall area object.
    /// </summary>
    /// <param name="wallName">Name of the wall to delete</param>
    /// <returns>True if successful</returns>
    bool DeleteWall(string wallName);

    // ====================
    // Orientation operations
    // ====================

    /// <summary>
    /// Sets the local axis rotation angle (β) for a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="angleDegrees">Rotation angle in degrees</param>
    /// <returns>True if successful</returns>
    bool SetLocalAxisAngle(string wallName, double angleDegrees);

    /// <summary>
    /// Gets the local axis rotation angle for a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="angleDegrees">Output: rotation angle in degrees</param>
    /// <returns>True if successful</returns>
    bool GetLocalAxisAngle(string wallName, out double angleDegrees);

    /// <summary>
    /// Gets the design orientation for a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="isVertical">Output: true if vertical</param>
    /// <param name="isFlipped">Output: true if flipped</param>
    /// <returns>True if successful</returns>
    bool GetDesignOrientation(string wallName, out bool isVertical, out bool isFlipped);

    // ====================
    // Pier and Spandrel operations
    // ====================

    /// <summary>
    /// Assigns a pier label to a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="pierLabel">Pier label name</param>
    /// <returns>True if successful</returns>
    bool AssignPier(string wallName, string pierLabel);

    /// <summary>
    /// Assigns a spandrel label to a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="spandrelLabel">Spandrel label name</param>
    /// <returns>True if successful</returns>
    bool AssignSpandrel(string wallName, string spandrelLabel);

    /// <summary>
    /// Gets the pier assignment for a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="pierLabel">Output: pier label</param>
    /// <returns>True if successful</returns>
    bool GetPier(string wallName, out string pierLabel);

    /// <summary>
    /// Gets the spandrel assignment for a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="spandrelLabel">Output: spandrel label</param>
    /// <returns>True if successful</returns>
    bool GetSpandrel(string wallName, out string spandrelLabel);

    // ====================
    // Opening operations
    // ====================

    /// <summary>
    /// Sets whether an area object is an opening.
    /// </summary>
    /// <param name="wallName">Name of the area object</param>
    /// <param name="isOpening">True to make it an opening</param>
    /// <returns>True if successful</returns>
    bool SetOpening(string wallName, bool isOpening);

    /// <summary>
    /// Creates an opening within a wall by defining a sub-region.
    /// </summary>
    /// <param name="parentWallName">Name of the parent wall</param>
    /// <param name="numPoints">Number of corner points for the opening</param>
    /// <param name="x">X coordinates of opening corners</param>
    /// <param name="y">Y coordinates of opening corners</param>
    /// <param name="z">Z coordinates of opening corners</param>
    /// <param name="openingName">Output: name of the created opening</param>
    /// <returns>True if successful</returns>
    bool CreateOpening(string parentWallName, int numPoints, double[] x, double[] y, double[] z,
        out string openingName);

    // ====================
    // Story operations
    // ====================

    /// <summary>
    /// Gets all story names in the model.
    /// </summary>
    /// <returns>Array of story names</returns>
    string[] GetStoryNames();

    /// <summary>
    /// Gets the elevation of a story.
    /// </summary>
    /// <param name="storyName">Name of the story</param>
    /// <param name="elevationM">Output: elevation in meters</param>
    /// <returns>True if successful</returns>
    bool GetStoryElevation(string storyName, out double elevationM);

    // ====================
    // Utility operations
    // ====================

    /// <summary>
    /// Sets a flag indicating that the model geometry has changed and analysis results may be outdated.
    /// </summary>
    void SetResultsDirty();

    /// <summary>
    /// Checks if analysis results are dirty (outdated).
    /// </summary>
    /// <returns>True if results are dirty</returns>
    bool AreResultsDirty();
}
