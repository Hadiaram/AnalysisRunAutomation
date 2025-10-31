# Smoke Test Guide for Walls Feature

This guide provides a minimal scenario to verify the end-to-end functionality of the Walls feature in the ETABS Automation Plugin.

---

## Prerequisites

1. **ETABS 22 (64-bit)** installed on Windows
2. **.NET 8 SDK** installed
3. **Plugin built** and registered in ETABS
4. **ETABS model** open (can be blank or existing)

---

## Build and Register the Plugin

### Step 1: Build the Solution

```bash
cd /path/to/AnalysisRunAutomation
dotnet build -c Release -p:Platform=x64
```

**Expected Output:**
- `AnalysisRunAutomation\bin\Release\net8.0-windows\x64\ETABS_Plugin.dll`
- `AnalysisRunAutomation\bin\Release\net8.0-windows\x64\Etabs.Automation.Walls.dll`
- `AnalysisRunAutomation\bin\Release\net8.0-windows\x64\Etabs.Automation.Abstractions.dll`

### Step 2: Register the Host DLL in ETABS

1. Open ETABS 22
2. Go to **Tools → Add-Ins → Manage Add-Ins…**
3. Click **Add…**
4. Browse to `ETABS_Plugin.dll` in the Release output folder
5. Ensure only `ETABS_Plugin.dll` is registered (do NOT register Walls.dll separately)
6. Click **OK** to save

**Verify:**
- The plugin appears in the list of registered add-ins
- ETABS shows no errors during registration

---

## Smoke Test Scenario

### Scenario: Create a Simple Rectangular Wall

**Goal:** Create a wall, set its orientation, assign a pier label, and verify in ETABS.

---

### Test 1: Launch the Plugin

1. In ETABS, open a new blank model (or any existing model)
2. Go to **Tools → Add-Ins → ETABS_Plugin** (or whatever name was registered)
3. The plugin window should appear

**Expected:**
- Window title: "ETABS Automation Plugin"
- Tabs visible: **Setup | Walls | Analysis | Logs**
- Logs tab shows: "=== ETABS Automation Plugin Loaded ==="

**✓ PASS** if the plugin window opens without errors.

---

### Test 2: Switch to Walls Tab

1. Click on the **Walls** tab

**Expected:**
- Wall creation form is visible with the following sections:
  - **Wall Definition** (Name, Story)
  - **Wall Section Properties** (Section Name, Thickness, Material)
  - **Geometry (meters)** (X₀, Y₀, Z₀, Length, Height, Direction)
  - **Orientation** (Local Axis β angle)
  - **Design Labels** (Pier, Spandrel)
  - **Create/Update Wall** button

**✓ PASS** if all controls are visible and functional.

---

### Test 3: Create a Wall Section and Wall Object

#### Setup:

In ETABS, create a simple model:
- Define at least one story (e.g., "Story1" at elevation 0m)
- Or use the default "Base" story if available

#### In the Plugin - Walls Tab:

1. **Wall Definition:**
   - Wall Name: `W_Story1_1`
   - Story: Select **Story1** (or **Base**)
   - Click **Refresh** if stories don't appear

2. **Wall Section Properties:**
   - Section Name: `WALL_200mm_C30`
   - Thickness: `200` mm
   - Material: `CONC-C30`

3. **Geometry (meters):**
   - X₀: `0.000`
   - Y₀: `0.000`
   - Z₀: `0.000`
   - Length: `4.000` m
   - Height: `3.000` m
   - Direction: **Along X axis** (checked)

4. **Orientation:**
   - Local Axis β: `0.00` degrees (keep default for now)

5. **Design Labels:**
   - Pier: `P1`
   - Spandrel: (leave blank)

6. Click **Create/Update Wall**

**Expected:**
- Status message in the Logs tab shows:
  ```
  === CREATING WALL ===
  Ensuring material 'CONC-C30'...
    Material ready: CONC-C30
  Ensuring wall section 'WALL_200mm_C30'...
    Section ready: 200.0mm thick
  Creating new wall 'W_Story1_1'...
    Wall created: <actual ETABS name>
  Assigning pier label 'P1'...
    Pier assigned successfully

  ✓ Wall 'W_Story1_1' created successfully
  ```
- Success message box: "Wall 'W_Story1_1' created/updated successfully!"

**✓ PASS** if the wall creation succeeds and logs show all steps completed.

---

### Test 4: Verify Wall in ETABS

1. Switch back to ETABS main window
2. Go to **View → Set 3D View** to see the model in 3D
3. Look for the wall object

**Expected:**
- A rectangular wall is visible at the origin (0, 0, 0)
- Wall dimensions: 4m (length) × 3m (height)
- Wall runs along the X-axis

**Verify Properties:**

4. Select the wall in ETABS
5. Check properties:
   - **Section:** `WALL_200mm_C30`
   - **Material:** Should be `CONC-C30` (check in section definition)
   - **Pier:** `P1` (visible in object properties or assignments)

**✓ PASS** if the wall exists with correct geometry, section, and pier assignment.

---

### Test 5: Update Wall (Idempotency Test)

**Goal:** Verify that re-applying the same wall updates it instead of creating a duplicate.

#### In the Plugin - Walls Tab:

1. Change **Thickness** to `250` mm
2. Change **Local Axis β** to `90.00` degrees
3. Keep all other fields the same
4. Click **Create/Update Wall**

**Expected:**
- Logs show:
  ```
  === CREATING WALL ===
  ...
  Updating existing wall 'W_Story1_1'...
  ...
  Setting local axis angle to 90.0°...
    Orientation set successfully
  ...
  ✓ Wall 'W_Story1_1' updated successfully
  ```

**Verify in ETABS:**
- Still only ONE wall at the location (no duplicate)
- Wall thickness updated to 250mm (check section definition)
- Local axis rotated 90°

**✓ PASS** if the wall is updated without creating a duplicate.

---

### Test 6: Create a Wall with Opening (Optional)

**Note:** This test is optional as opening creation requires more complex geometry setup.

If you want to test openings:
1. Create a wall as in Test 3
2. Manually add opening coordinates within the wall boundary
3. The current UI doesn't have opening controls yet (future enhancement)
4. You can test via code by creating a `WallSpec` with `Openings` collection

**Skip this test for now** or enhance the UI to support openings.

---

### Test 7: Run Analysis with Wall

#### In the Plugin:

1. Go to **Setup** tab
2. Verify concrete strength (default 25 MPa is fine)
3. Check **Walls** checkbox
4. Click **Create Sections**

5. Go to **Analysis** tab
6. Click through the workflow buttons:
   - **Create Load Patterns**
   - **Apply Fixed Boundary Conditions** (applies fixed supports at base)
   - **Assign Sections Intelligently** (may assign wall section to area objects)
   - **Run Complete Workflow** (or individual steps)

**Expected:**
- Analysis completes successfully
- Logs show analysis results and base reactions
- No errors related to wall geometry or properties

**✓ PASS** if analysis completes and wall is included in the model.

---

### Test 8: Check Results Dirty Flag

**Goal:** Verify that the "results dirty" indicator works after wall geometry changes.

1. Create a wall (as in Test 3)
2. Run analysis (as in Test 7)
3. Go back to **Walls** tab
4. Modify the wall (e.g., change height to 4m)
5. Click **Create/Update Wall**
6. Go to **Analysis** tab

**Expected:**
- Some indicator (label, message, or prompt) suggests re-running analysis due to geometry changes
- (Note: The current implementation has a flag in the gateway; future UI enhancement needed to display it)

**✓ PASS** if the system internally tracks that results are outdated (check via logs or future UI indicator).

---

## Summary Checklist

| Test | Description | Status |
|------|-------------|--------|
| 1 | Launch plugin | ☐ |
| 2 | Walls tab visible | ☐ |
| 3 | Create wall successfully | ☐ |
| 4 | Verify wall in ETABS | ☐ |
| 5 | Update wall (idempotency) | ☐ |
| 6 | *(Optional)* Create opening | ☐ |
| 7 | Run analysis with wall | ☐ |
| 8 | Results dirty flag | ☐ |

---

## Troubleshooting

### Plugin doesn't appear in ETABS Add-Ins menu

- **Check:** Is `ETABS_Plugin.dll` registered?
- **Fix:** Re-register via Manage Add-Ins

### "Could not load file or assembly" error

- **Check:** Are `Etabs.Automation.Walls.dll` and `Etabs.Automation.Abstractions.dll` in the same folder as the host DLL?
- **Fix:** Ensure project references have `CopyLocal = true` (or `<Private>True</Private>` in .csproj)

### Stories dropdown is empty

- **Check:** Does the ETABS model have any stories defined?
- **Fix:** Add at least one story in ETABS (Edit → Story Data → Edit Stories)

### Wall creation fails with "invalid coordinates"

- **Check:** Are X, Y, Z coordinates reasonable? (Avoid extreme values)
- **Fix:** Use simple values like (0, 0, 0) for testing

### Material or section not created

- **Check:** Logs tab for error messages
- **Fix:** Ensure material name is valid (no special characters) and strength > 0

### Wall not visible in ETABS

- **Check:** Is the wall at the expected elevation?
- **Fix:** Verify Z₀ matches the story elevation; adjust view to see the correct level

---

## Expected Results

After completing all tests:

✅ **Plugin loads** without errors
✅ **Walls tab** is functional with all controls
✅ **Wall created** in ETABS with correct properties
✅ **Idempotent updates** work (no duplicates)
✅ **Pier label assigned** correctly
✅ **Analysis runs** successfully with wall included

If all tests pass, the Walls feature is working as expected! 🎉

---

## Next Steps

1. Test with more complex geometry (L-shaped walls, multiple walls)
2. Test opening creation (requires UI enhancement)
3. Test pier/spandrel design results extraction (future feature)
4. Test batch wall creation from CSV/JSON (future feature)

---

## Reporting Issues

If any test fails:
1. Check the **Logs** tab for detailed error messages
2. Copy the error log
3. Report the issue with:
   - Test number that failed
   - Error message from logs
   - ETABS version
   - OS version

---

## Conclusion

This smoke test verifies the core functionality of the Walls feature:
- Wall section creation
- Wall object creation with geometry
- Orientation and pier assignment
- Idempotent updates
- Integration with analysis workflow

For more detailed testing, refer to the full test suite (if available) or create additional test cases based on your specific use cases.
