using Etabs.Automation.Walls.Models;

namespace Etabs.Automation.Walls.Services;

/// <summary>
/// Service interface for wall creation and management operations.
/// </summary>
public interface IWallsService
{
    /// <summary>
    /// Creates or updates a single wall. Idempotent operation.
    /// </summary>
    /// <param name="spec">Wall specification</param>
    /// <param name="report">Output: detailed operation report</param>
    /// <returns>True if successful</returns>
    bool UpsertWall(WallSpec spec, out string report);

    /// <summary>
    /// Creates or updates multiple walls in a single undo group.
    /// </summary>
    /// <param name="specs">List of wall specifications</param>
    /// <param name="report">Output: detailed operation report</param>
    /// <returns>True if all operations succeeded</returns>
    bool UpsertMany(IEnumerable<WallSpec> specs, out string report);

    /// <summary>
    /// Sets the local axis orientation for a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="angleDegrees">Rotation angle in degrees</param>
    /// <param name="report">Output: operation report</param>
    /// <returns>True if successful</returns>
    bool SetOrientation(string wallName, double angleDegrees, out string report);

    /// <summary>
    /// Assigns a pier label to a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="pierLabel">Pier label</param>
    /// <param name="report">Output: operation report</param>
    /// <returns>True if successful</returns>
    bool AssignPier(string wallName, string pierLabel, out string report);

    /// <summary>
    /// Assigns a spandrel label to a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="spandrelLabel">Spandrel label</param>
    /// <param name="report">Output: operation report</param>
    /// <returns>True if successful</returns>
    bool AssignSpandrel(string wallName, string spandrelLabel, out string report);

    /// <summary>
    /// Adds openings to a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall</param>
    /// <param name="openings">List of opening specifications</param>
    /// <param name="report">Output: operation report</param>
    /// <returns>True if successful</returns>
    bool AddOpenings(string wallName, IEnumerable<OpeningSpec> openings, out string report);

    /// <summary>
    /// Validates a wall specification without creating it.
    /// </summary>
    /// <param name="spec">Wall specification to validate</param>
    /// <param name="errors">Output: validation errors</param>
    /// <returns>True if valid</returns>
    bool ValidateSpec(WallSpec spec, out List<string> errors);

    /// <summary>
    /// Validates multiple wall specifications.
    /// </summary>
    /// <param name="specs">Wall specifications to validate</param>
    /// <param name="errors">Output: validation errors</param>
    /// <returns>True if all are valid</returns>
    bool ValidateSpecs(IEnumerable<WallSpec> specs, out List<string> errors);

    /// <summary>
    /// Deletes a wall.
    /// </summary>
    /// <param name="wallName">Name of the wall to delete</param>
    /// <param name="report">Output: operation report</param>
    /// <returns>True if successful</returns>
    bool DeleteWall(string wallName, out string report);

    /// <summary>
    /// Gets all available story names from the model.
    /// </summary>
    /// <returns>Array of story names</returns>
    string[] GetStoryNames();
}
