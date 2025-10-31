using System.Text;
using Etabs.Automation.Abstractions;
using Etabs.Automation.Walls.Models;

namespace Etabs.Automation.Walls.Services;

/// <summary>
/// Implementation of walls service using IEtabsGateway abstraction.
/// </summary>
public class WallsService : IWallsService
{
    private readonly IEtabsGateway _gateway;

    public WallsService(IEtabsGateway gateway)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    public bool UpsertWall(WallSpec spec, out string report)
    {
        var sb = new StringBuilder();

        try
        {
            // Validate
            if (!ValidateSpec(spec, out var errors))
            {
                report = $"Validation failed:\n" + string.Join("\n", errors);
                return false;
            }

            _gateway.BeginUndo($"Upsert Wall {spec.Name}");

            try
            {
                // 1. Ensure material exists
                sb.AppendLine($"Ensuring material '{spec.MaterialName}'...");
                var actualMaterialName = _gateway.EnsureConcreteMaterial(spec.MaterialName, 25.0); // Default strength
                sb.AppendLine($"  Material ready: {actualMaterialName}");

                // 2. Ensure wall section exists
                sb.AppendLine($"Ensuring wall section '{spec.SectionName}'...");
                if (!_gateway.SetWallSection(spec.SectionName, spec.ThicknessMm, actualMaterialName))
                {
                    sb.AppendLine($"  ERROR: Failed to create/update section");
                    report = sb.ToString();
                    return false;
                }
                sb.AppendLine($"  Section ready: {spec.ThicknessMm}mm thick");

                // 3. Create or update wall object
                bool isUpdate = _gateway.WallExists(spec.Name);
                if (isUpdate)
                {
                    sb.AppendLine($"Updating existing wall '{spec.Name}'...");

                    // Delete existing and recreate (simpler than modifying geometry)
                    if (!_gateway.DeleteWall(spec.Name))
                    {
                        sb.AppendLine($"  WARNING: Could not delete existing wall");
                    }
                }
                else
                {
                    sb.AppendLine($"Creating new wall '{spec.Name}'...");
                }

                if (!_gateway.CreateWallByCoords(
                    spec.XCoords.Length,
                    spec.XCoords,
                    spec.YCoords,
                    spec.ZCoords,
                    spec.SectionName,
                    spec.StoryName,
                    out string wallName))
                {
                    sb.AppendLine($"  ERROR: Failed to create wall");
                    report = sb.ToString();
                    return false;
                }

                sb.AppendLine($"  Wall created: {wallName}");
                spec.Name = wallName; // Update with actual name

                // 4. Set local axis orientation
                if (Math.Abs(spec.LocalAxisAngleDeg) > 0.001)
                {
                    sb.AppendLine($"Setting local axis angle to {spec.LocalAxisAngleDeg}°...");
                    if (!_gateway.SetLocalAxisAngle(spec.Name, spec.LocalAxisAngleDeg))
                    {
                        sb.AppendLine($"  WARNING: Failed to set orientation");
                    }
                    else
                    {
                        sb.AppendLine($"  Orientation set successfully");
                    }
                }

                // 5. Assign pier label
                if (!string.IsNullOrWhiteSpace(spec.PierLabel))
                {
                    sb.AppendLine($"Assigning pier label '{spec.PierLabel}'...");
                    if (!_gateway.AssignPier(spec.Name, spec.PierLabel))
                    {
                        sb.AppendLine($"  WARNING: Failed to assign pier");
                    }
                    else
                    {
                        sb.AppendLine($"  Pier assigned successfully");
                    }
                }

                // 6. Assign spandrel label
                if (!string.IsNullOrWhiteSpace(spec.SpandrelLabel))
                {
                    sb.AppendLine($"Assigning spandrel label '{spec.SpandrelLabel}'...");
                    if (!_gateway.AssignSpandrel(spec.Name, spec.SpandrelLabel))
                    {
                        sb.AppendLine($"  WARNING: Failed to assign spandrel");
                    }
                    else
                    {
                        sb.AppendLine($"  Spandrel assigned successfully");
                    }
                }

                // 7. Add openings
                if (spec.Openings.Any())
                {
                    sb.AppendLine($"Adding {spec.Openings.Count} opening(s)...");
                    foreach (var opening in spec.Openings)
                    {
                        if (!_gateway.CreateOpening(
                            spec.Name,
                            opening.XCoords.Length,
                            opening.XCoords,
                            opening.YCoords,
                            opening.ZCoords,
                            out string openingName))
                        {
                            sb.AppendLine($"  WARNING: Failed to create opening '{opening.Name}'");
                        }
                        else
                        {
                            sb.AppendLine($"  Opening created: {openingName}");
                        }
                    }
                }

                // Mark results as dirty
                _gateway.SetResultsDirty();

                sb.AppendLine($"\n✓ Wall '{spec.Name}' {(isUpdate ? "updated" : "created")} successfully");
                report = sb.ToString();
                return true;
            }
            finally
            {
                _gateway.EndUndo();
                _gateway.RefreshView();
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"\n✗ ERROR: {ex.Message}");
            report = sb.ToString();
            return false;
        }
    }

    public bool UpsertMany(IEnumerable<WallSpec> specs, out string report)
    {
        var sb = new StringBuilder();
        var specList = specs.ToList();

        if (!specList.Any())
        {
            report = "No walls to create";
            return true;
        }

        sb.AppendLine($"Processing {specList.Count} wall(s)...\n");

        _gateway.BeginUndo($"Upsert {specList.Count} Walls");

        try
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var spec in specList)
            {
                if (UpsertWall(spec, out string wallReport))
                {
                    successCount++;
                    sb.AppendLine(wallReport);
                    sb.AppendLine();
                }
                else
                {
                    failCount++;
                    sb.AppendLine($"✗ Failed to process wall '{spec.Name}':");
                    sb.AppendLine(wallReport);
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"Summary: {successCount} succeeded, {failCount} failed");
            report = sb.ToString();
            return failCount == 0;
        }
        finally
        {
            _gateway.EndUndo();
            _gateway.RefreshView();
        }
    }

    public bool SetOrientation(string wallName, double angleDegrees, out string report)
    {
        try
        {
            if (!_gateway.WallExists(wallName))
            {
                report = $"Wall '{wallName}' does not exist";
                return false;
            }

            _gateway.BeginUndo($"Set Orientation {wallName}");

            try
            {
                if (!_gateway.SetLocalAxisAngle(wallName, angleDegrees))
                {
                    report = $"Failed to set orientation for wall '{wallName}'";
                    return false;
                }

                _gateway.SetResultsDirty();
                report = $"Orientation set to {angleDegrees}° for wall '{wallName}'";
                return true;
            }
            finally
            {
                _gateway.EndUndo();
                _gateway.RefreshView();
            }
        }
        catch (Exception ex)
        {
            report = $"ERROR: {ex.Message}";
            return false;
        }
    }

    public bool AssignPier(string wallName, string pierLabel, out string report)
    {
        try
        {
            if (!_gateway.WallExists(wallName))
            {
                report = $"Wall '{wallName}' does not exist";
                return false;
            }

            _gateway.BeginUndo($"Assign Pier {wallName}");

            try
            {
                if (!_gateway.AssignPier(wallName, pierLabel))
                {
                    report = $"Failed to assign pier '{pierLabel}' to wall '{wallName}'";
                    return false;
                }

                report = $"Pier '{pierLabel}' assigned to wall '{wallName}'";
                return true;
            }
            finally
            {
                _gateway.EndUndo();
                _gateway.RefreshView();
            }
        }
        catch (Exception ex)
        {
            report = $"ERROR: {ex.Message}";
            return false;
        }
    }

    public bool AssignSpandrel(string wallName, string spandrelLabel, out string report)
    {
        try
        {
            if (!_gateway.WallExists(wallName))
            {
                report = $"Wall '{wallName}' does not exist";
                return false;
            }

            _gateway.BeginUndo($"Assign Spandrel {wallName}");

            try
            {
                if (!_gateway.AssignSpandrel(wallName, spandrelLabel))
                {
                    report = $"Failed to assign spandrel '{spandrelLabel}' to wall '{wallName}'";
                    return false;
                }

                report = $"Spandrel '{spandrelLabel}' assigned to wall '{wallName}'";
                return true;
            }
            finally
            {
                _gateway.EndUndo();
                _gateway.RefreshView();
            }
        }
        catch (Exception ex)
        {
            report = $"ERROR: {ex.Message}";
            return false;
        }
    }

    public bool AddOpenings(string wallName, IEnumerable<OpeningSpec> openings, out string report)
    {
        var sb = new StringBuilder();
        var openingList = openings.ToList();

        try
        {
            if (!_gateway.WallExists(wallName))
            {
                report = $"Wall '{wallName}' does not exist";
                return false;
            }

            if (!openingList.Any())
            {
                report = "No openings to add";
                return true;
            }

            _gateway.BeginUndo($"Add Openings to {wallName}");

            try
            {
                int successCount = 0;
                int failCount = 0;

                foreach (var opening in openingList)
                {
                    if (!opening.Validate(out var errors))
                    {
                        sb.AppendLine($"✗ Invalid opening '{opening.Name}': {string.Join(", ", errors)}");
                        failCount++;
                        continue;
                    }

                    if (!_gateway.CreateOpening(
                        wallName,
                        opening.XCoords.Length,
                        opening.XCoords,
                        opening.YCoords,
                        opening.ZCoords,
                        out string openingName))
                    {
                        sb.AppendLine($"✗ Failed to create opening '{opening.Name}'");
                        failCount++;
                    }
                    else
                    {
                        sb.AppendLine($"✓ Opening created: {openingName}");
                        successCount++;
                    }
                }

                _gateway.SetResultsDirty();
                sb.AppendLine($"\nSummary: {successCount} succeeded, {failCount} failed");
                report = sb.ToString();
                return failCount == 0;
            }
            finally
            {
                _gateway.EndUndo();
                _gateway.RefreshView();
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"\nERROR: {ex.Message}");
            report = sb.ToString();
            return false;
        }
    }

    public bool ValidateSpec(WallSpec spec, out List<string> errors)
    {
        return spec.Validate(out errors);
    }

    public bool ValidateSpecs(IEnumerable<WallSpec> specs, out List<string> errors)
    {
        errors = new List<string>();
        bool allValid = true;

        foreach (var spec in specs)
        {
            if (!spec.Validate(out var specErrors))
            {
                errors.Add($"Wall '{spec.Name}':");
                errors.AddRange(specErrors.Select(e => $"  - {e}"));
                allValid = false;
            }
        }

        return allValid;
    }

    public bool DeleteWall(string wallName, out string report)
    {
        try
        {
            if (!_gateway.WallExists(wallName))
            {
                report = $"Wall '{wallName}' does not exist";
                return false;
            }

            _gateway.BeginUndo($"Delete Wall {wallName}");

            try
            {
                if (!_gateway.DeleteWall(wallName))
                {
                    report = $"Failed to delete wall '{wallName}'";
                    return false;
                }

                _gateway.SetResultsDirty();
                report = $"Wall '{wallName}' deleted successfully";
                return true;
            }
            finally
            {
                _gateway.EndUndo();
                _gateway.RefreshView();
            }
        }
        catch (Exception ex)
        {
            report = $"ERROR: {ex.Message}";
            return false;
        }
    }

    public string[] GetStoryNames()
    {
        return _gateway.GetStoryNames();
    }
}
