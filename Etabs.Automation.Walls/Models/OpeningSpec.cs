namespace Etabs.Automation.Walls.Models;

/// <summary>
/// Specification for an opening within a wall.
/// </summary>
public class OpeningSpec
{
    /// <summary>
    /// Unique identifier/name for this opening
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// X coordinates of opening corner points (meters)
    /// </summary>
    public double[] XCoords { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Y coordinates of opening corner points (meters)
    /// </summary>
    public double[] YCoords { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Z coordinates of opening corner points (meters)
    /// </summary>
    public double[] ZCoords { get; set; } = Array.Empty<double>();

    /// <summary>
    /// Validates the opening specification.
    /// </summary>
    /// <param name="errors">Output: list of validation error messages</param>
    /// <returns>True if valid</returns>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Opening name is required");

        if (XCoords.Length < 3)
            errors.Add("At least 3 corner points are required");

        if (XCoords.Length != YCoords.Length || XCoords.Length != ZCoords.Length)
            errors.Add("Coordinate arrays must have the same length");

        return errors.Count == 0;
    }

    /// <summary>
    /// Creates a rectangular opening specification.
    /// </summary>
    /// <param name="name">Opening name</param>
    /// <param name="x0">Starting X coordinate (meters)</param>
    /// <param name="y0">Starting Y coordinate (meters)</param>
    /// <param name="z0">Bottom Z coordinate (meters)</param>
    /// <param name="widthM">Opening width (meters)</param>
    /// <param name="heightM">Opening height (meters)</param>
    /// <param name="isAlongX">True if opening is in a wall along X axis</param>
    /// <returns>Opening specification</returns>
    public static OpeningSpec CreateRectangular(
        string name,
        double x0, double y0, double z0,
        double widthM, double heightM,
        bool isAlongX = true)
    {
        double[] xCoords, yCoords, zCoords;

        if (isAlongX)
        {
            xCoords = new[] { x0, x0 + widthM, x0 + widthM, x0 };
            yCoords = new[] { y0, y0, y0, y0 };
            zCoords = new[] { z0, z0, z0 + heightM, z0 + heightM };
        }
        else
        {
            xCoords = new[] { x0, x0, x0, x0 };
            yCoords = new[] { y0, y0 + widthM, y0 + widthM, y0 };
            zCoords = new[] { z0, z0, z0 + heightM, z0 + heightM };
        }

        return new OpeningSpec
        {
            Name = name,
            XCoords = xCoords,
            YCoords = yCoords,
            ZCoords = zCoords
        };
    }
}
