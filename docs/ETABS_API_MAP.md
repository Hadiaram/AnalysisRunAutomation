# ETABS API Method Reference for Walls Feature

This document maps the ETABS API v1 methods used by the Walls feature, organized by functionality.

---

## API Wrapper: `IEtabsGateway` → `EtabsGateway`

The `EtabsGateway` class wraps raw ETABS API calls and centralizes:
- Unit conversions (mm ↔ m)
- Selection save/restore
- Results dirty flag tracking

All wall operations go through this gateway to ensure consistency.

---

## 1. Material Operations

### Create/Ensure Concrete Material

```csharp
// Add or verify concrete material exists
_model.PropMaterial.AddMaterial(ref name, eMatType.Concrete, region, standard, grade);
_model.PropMaterial.SetOConcrete_1(name, fc, isLightweight, fcsfactor, sstype, ssHysType, strainAtfc, strainUltimate);
```

**Parameters:**
- `fc`: Concrete compressive strength (MPa)
- Default density: 2400 kg/m³

**Usage in Gateway:**
- `EnsureConcreteMaterial(materialName, strengthMPa)`

---

## 2. Wall Section Properties

### Create/Update Wall Section

```csharp
_model.PropArea.SetWall(
    sectionName,
    eWallPropType.Specified,
    eShellType.ShellThin,
    materialName,
    thickness
);
```

**Parameters:**
- `sectionName`: e.g., "WALL_200mm_C30"
- `thickness`: in meters (converted from mm)
- `materialName`: concrete material

**Usage in Gateway:**
- `SetWallSection(sectionName, thicknessMm, materialName)`
- Converts `thicknessMm` → meters automatically

### Get Wall Section Properties

```csharp
_model.PropArea.GetWall(
    sectionName,
    ref wallType,
    ref shellType,
    ref matProp,
    ref thickness,
    ref color,
    ref notes,
    ref guid
);
```

**Returns:**
- `thickness` in meters (converted to mm in gateway)
- `matProp`: material name

**Usage in Gateway:**
- `GetWallSection(sectionName, out thicknessMm, out materialName)`

### Check if Section Exists

```csharp
_model.PropArea.GetNameList(ref numSections, ref sectionNames);
```

**Usage in Gateway:**
- `WallSectionExists(sectionName)` → checks if name is in list

---

## 3. Wall Area Objects (Geometry)

### Create Wall by Coordinates

```csharp
_model.AreaObj.AddByCoord(
    numPoints,
    ref x,  // meters
    ref y,  // meters
    ref z,  // meters
    ref name,
    propName,  // section property name
    userGUID
);
```

**Parameters:**
- `numPoints`: typically 4 for rectangular walls
- `x, y, z`: arrays of coordinates (meters)
- Returns `name`: actual wall object name assigned by ETABS

**Usage in Gateway:**
- `CreateWallByCoords(numPoints, x, y, z, sectionName, storyName, out wallName)`

**Follow-up:**
```csharp
// Ensure property is assigned
_model.AreaObj.SetProperty(wallName, sectionName);
```

### Get Wall Corner Points

```csharp
_model.AreaObj.GetPoints(wallName, ref numPoints, ref pointNames);
```

Then for each point:
```csharp
_model.PointObj.GetCoordCartesian(pointName, ref x, ref y, ref z);
```

**Usage in Gateway:**
- `GetWallPoints(wallName, out x, out y, out z)`

### Delete Wall

```csharp
_model.AreaObj.Delete(wallName);
```

**Usage in Gateway:**
- `DeleteWall(wallName)` → for idempotent updates

---

## 4. Orientation (Local Axes)

### Set Local Axis Angle

```csharp
_model.AreaObj.SetLocalAxes(wallName, angleDegrees);
```

**Parameters:**
- `angleDegrees`: rotation angle β

**Usage in Gateway:**
- `SetLocalAxisAngle(wallName, angleDegrees)`

### Get Local Axis Angle

```csharp
_model.AreaObj.GetLocalAxes(wallName, ref angle, ref advanced);
```

**Usage in Gateway:**
- `GetLocalAxisAngle(wallName, out angleDegrees)`

### Get Design Orientation

```csharp
_model.AreaObj.GetDesignOrientation(wallName, ref orientation);
```

**Returns:**
- `orientation`: `eWallDesignOrientation` (Horizontal, Vertical, VerticalFlipped)

**Usage in Gateway:**
- `GetDesignOrientation(wallName, out isVertical, out isFlipped)`

---

## 5. Pier and Spandrel Labels

### Assign Pier

```csharp
_model.AreaObj.SetPier(wallName, pierLabel);
```

**Usage in Gateway:**
- `AssignPier(wallName, pierLabel)`

### Assign Spandrel

```csharp
_model.AreaObj.SetSpandrel(wallName, spandrelLabel);
```

**Usage in Gateway:**
- `AssignSpandrel(wallName, spandrelLabel)`

### Get Pier/Spandrel

```csharp
_model.AreaObj.GetPier(wallName, ref pierLabel);
_model.AreaObj.GetSpandrel(wallName, ref spandrelLabel);
```

**Usage in Gateway:**
- `GetPier(wallName, out pierLabel)`
- `GetSpandrel(wallName, out spandrelLabel)`

---

## 6. Openings

### Mark Area as Opening

```csharp
_model.AreaObj.SetOpening(wallName, true);
```

**Usage in Gateway:**
- `SetOpening(wallName, isOpening)`

### Create Opening within Wall

1. Create a smaller area object inside the wall boundary
2. Mark it as an opening

```csharp
_model.AreaObj.AddByCoord(numPoints, ref x, ref y, ref z, ref name, "None", "");
_model.AreaObj.SetOpening(name, true);
```

**Usage in Gateway:**
- `CreateOpening(parentWallName, numPoints, x, y, z, out openingName)`

---

## 7. Story Operations

### Get All Stories

```csharp
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
```

**Returns:**
- `storyNames`: array of story names
- `elevations`: in program units (typically meters)

**Usage in Gateway:**
- `GetStoryNames()` → string[]

### Get Story Elevation

```csharp
_model.Story.GetStory(
    storyName,
    ref elevation,
    ref height,
    ref isMasterStory,
    ref similarToStory,
    ref spliceAbove,
    ref spliceHeight,
    ref color
);
```

**Usage in Gateway:**
- `GetStoryElevation(storyName, out elevationM)`

---

## 8. View and Selection Management

### Refresh View

```csharp
_model.View.RefreshView(0, false);
```

**Usage in Gateway:**
- `RefreshView()` → called after batch operations

### Save/Restore Selection

```csharp
// Save
_model.SelectObj.GetSelected(ref numberItems, ref itemType, ref objectNames);

// Restore
_model.SelectObj.ClearSelection();
foreach (var item in savedSelection)
{
    _model.SelectObj.Selected(eItemType.Object, item, true);
}
```

**Usage in Gateway:**
- `SaveSelection()` / `RestoreSelection()`
- Useful for preserving user selection after DB queries

---

## Typical Call Sequence for Creating a Wall

### Scenario: User clicks "Create Wall" button

1. **Validate inputs** (`WallsService.ValidateSpec`)
2. **Begin undo group** (placeholder in v1)
3. **Ensure material exists** (`EnsureConcreteMaterial`)
   - Check with `PropMaterial.GetNameList`
   - Create with `PropMaterial.AddMaterial` + `SetOConcrete_1`
4. **Ensure wall section exists** (`SetWallSection`)
   - `PropArea.SetWall` (creates or updates)
5. **Delete existing wall if updating** (for idempotency)
   - `AreaObj.Delete(wallName)`
6. **Create new wall object** (`CreateWallByCoords`)
   - `AreaObj.AddByCoord`
   - `AreaObj.SetProperty` to assign section
7. **Set local axis orientation** (optional)
   - `AreaObj.SetLocalAxes(wallName, angle)`
8. **Assign pier/spandrel** (optional)
   - `AreaObj.SetPier(wallName, pierLabel)`
   - `AreaObj.SetSpandrel(wallName, spandrelLabel)`
9. **Create openings** (optional)
   - For each opening: `AreaObj.AddByCoord` + `SetOpening(true)`
10. **Mark results dirty** (internal flag)
11. **End undo group** (placeholder)
12. **Refresh view** (`View.RefreshView`)

---

## Units Convention

**Standard Units (kN-m-C):**
- Force: kN
- Length: m
- Temperature: Celsius

**UI accepts millimeters:**
- User enters thickness in mm
- Gateway converts to m before calling `PropArea.SetWall`

**Coordinate System:**
- X, Y, Z in meters
- Wall orientation defined by corner point order and local axis angle β

---

## Error Handling

All ETABS API calls return `int`:
- `0` = success
- Non-zero = error

The gateway wraps calls in try-catch and returns `bool`:
```csharp
public bool SetWallSection(string sectionName, double thicknessMm, string materialName)
{
    try
    {
        double thicknessM = thicknessMm / 1000.0;
        int ret = _model.PropArea.SetWall(...);
        return ret == 0;
    }
    catch
    {
        return false;
    }
}
```

---

## Future Enhancements

### Shear Wall Design API (Not Yet Implemented)

```csharp
// Set design preferences
_model.DesignShearWall.SetCode(code);
_model.DesignShearWall.SetPreference(...);

// Set overwrites
_model.DesignShearWall.SetOverwrite(wallName, item, value, itemType);

// Get design results
_model.DesignShearWall.GetDesignResults(wallName, ...);
```

These will be exposed through IEtabsGateway in a future update.

---

## References

- **ETABS API Documentation**: Installed with ETABS (typically in `C:\Program Files\Computers and Structures\ETABS 22\CSI ETABS API Documentation.chm`)
- **CSiAPIv1 DLL**: `ETABSv1.dll` (managed .NET Standard 2.0 binding)
- **Sample code**: ETABS installation includes C# API examples

---

## Summary Table

| Feature | Key ETABS API Methods | Gateway Method |
|---------|----------------------|----------------|
| Material | `PropMaterial.AddMaterial`, `SetOConcrete_1` | `EnsureConcreteMaterial` |
| Wall Section | `PropArea.SetWall`, `GetWall` | `SetWallSection`, `GetWallSection` |
| Wall Object | `AreaObj.AddByCoord`, `SetProperty` | `CreateWallByCoords` |
| Orientation | `AreaObj.SetLocalAxes`, `GetLocalAxes` | `SetLocalAxisAngle`, `GetLocalAxisAngle` |
| Pier/Spandrel | `AreaObj.SetPier`, `SetSpandrel` | `AssignPier`, `AssignSpandrel` |
| Openings | `AreaObj.AddByCoord`, `SetOpening` | `CreateOpening` |
| Stories | `Story.GetStories_2`, `GetStory` | `GetStoryNames`, `GetStoryElevation` |
