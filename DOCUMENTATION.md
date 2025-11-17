# ETABS Analysis Run Automation Plugin - Complete Documentation

## Table of Contents
- [Overview](#overview)
- [System Architecture](#system-architecture)
- [ETABS API Fundamentals](#etabs-api-fundamentals)
- [Project Structure](#project-structure)
- [Manager Classes Reference](#manager-classes-reference)
- [Workflow Automation](#workflow-automation)
- [Developer Guide](#developer-guide)
- [Usage Guide](#usage-guide)
- [Troubleshooting](#troubleshooting)
- [Appendix](#appendix)

---

## Overview

The ETABS Analysis Run Automation Plugin is a comprehensive C# Windows Forms application that integrates with CSI ETABS (v1 API) to automate structural engineering workflows. The plugin provides a complete toolkit for:

- **Model Setup**: Creating materials, sections, loads, and boundary conditions
- **Intelligent Assignment**: Automatically assigning properties based on element geometry and orientation
- **Mesh Generation**: Applying finite element mesh to shell elements
- **Analysis Execution**: Running structural analysis with comprehensive error handling
- **Wall Placement**: Interactive GUI for placing shear walls with pier labels
- **Complete Workflow**: One-click automation of the entire 11-step modeling process

### Key Features

✅ **11-Step Automated Workflow**: From materials to analysis in one click
✅ **Intelligent Section Assignment**: Automatically identifies columns, beams, slabs, and walls
✅ **Load Combination Generation**: Creates ULS, SLS, and envelope combinations per code requirements
✅ **Diaphragm Intelligence**: Analyzes floor configuration to select rigid vs. semi-rigid diaphragms
✅ **Mass Source Configuration**: Automated seismic mass setup (Dead + 30% Live)
✅ **Wall Placement Tool**: Interactive UI for placing shear walls with custom orientations
✅ **Comprehensive Error Handling**: Detailed error messages and troubleshooting guidance

---

## System Architecture

### Design Philosophy

The plugin follows a **Manager-Based Architecture** where each manager class encapsulates a specific domain of functionality. This design provides:

- **Separation of Concerns**: Each manager handles one aspect of the workflow
- **Loose Coupling**: Managers are independent with no cross-dependencies
- **Testability**: Managers can be tested independently
- **Maintainability**: Easy to modify or extend individual managers
- **Centralized Orchestration**: The main form (Form1) coordinates all managers

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      ETABS Application                       │
│                     (ETABSv1.dll API)                        │
└────────────────────────┬────────────────────────────────────┘
                         │ cSapModel, cPluginCallback
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                     cPlugin.cs (Entry Point)                 │
│                   Implements ETABS Plugin Interface          │
└────────────────────────┬────────────────────────────────────┘
                         │ Creates & Shows
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Form1.cs (Main UI Controller)               │
│              Orchestrates all managers & workflow            │
└────┬───────┬───────┬───────┬───────┬───────┬───────┬────────┘
     │       │       │       │       │       │       │
     │       │       │       │       │       │       │
┌────▼──┐ ┌──▼──┐ ┌─▼───┐ ┌─▼────┐ ┌▼────┐ ┌▼────┐ ┌▼─────┐
│Section│ │Load │ │Bound│ │Diaph │ │Mesh │ │Sect │ │Load  │
│Manager│ │Mgr  │ │ary  │ │ragm  │ │Mgr  │ │Asgn │ │Asgn  │
│       │ │     │ │Mgr  │ │Mgr   │ │     │ │Mgr  │ │Mgr   │
└───────┘ └─────┘ └─────┘ └──────┘ └─────┘ └─────┘ └──────┘

┌────────┐ ┌────────┐ ┌────────┐ ┌─────────────────────┐
│Mass    │ │Analysis│ │Wall    │ │ WallPlacementForm   │
│Source  │ │Manager │ │Place   │ │ (Interactive UI)    │
│Manager │ │        │ │ment    │ │                     │
│        │ │        │ │Manager │ │                     │
└────────┘ └────────┘ └────────┘ └─────────────────────┘
```

### Data Flow

1. **User Action** → Button Click in Form1
2. **Manager Invocation** → Form1 calls appropriate manager method(s)
3. **ETABS API Calls** → Manager executes ETABS API commands
4. **Result Reporting** → Manager returns bool + detailed string report
5. **UI Update** → Form1 displays results in txtStatus TextBox

---

## ETABS API Fundamentals

### Understanding the ETABS API

The plugin uses the **ETABSv1 COM API**, which provides programmatic access to ETABS functionality.

#### 1. cSapModel Object

The central object representing the entire ETABS model:

```csharp
private cSapModel _SapModel;

// Accessing sub-objects
_SapModel.PropMaterial    // Material properties
_SapModel.PropFrame       // Frame section properties
_SapModel.PropArea        // Area section properties
_SapModel.FrameObj        // Frame objects (beams, columns)
_SapModel.AreaObj         // Area objects (walls, slabs)
_SapModel.PointObj        // Point objects (joints)
_SapModel.LoadPatterns    // Load pattern definitions
_SapModel.RespCombo       // Load combinations
_SapModel.Analyze         // Analysis control
_SapModel.Results         // Analysis results
```

#### 2. Reference Parameters Pattern

ETABS API extensively uses `ref` parameters:

```csharp
// Initialize arrays before calling
int count = 0;
string[] names = null;
int ret = _SapModel.FrameObj.GetNameList(ref count, ref names);
```

#### 3. Return Codes

- `0` = Success
- `Non-zero` = Error

```csharp
int ret = _SapModel.PropMaterial.AddMaterial("CONC", eMatType.Concrete);
if (ret != 0)
{
    // Handle error
    return false;
}
```

#### 4. Units Management

```csharp
eUnits originalUnits = eUnits.kN_m_C;
_SapModel.GetPresentUnits(ref originalUnits);
_SapModel.SetPresentUnits(eUnits.kN_m_C);

// Do work...

_SapModel.SetPresentUnits(originalUnits); // Restore
```

#### 5. Model Locking

```csharp
bool isLocked = false;
_SapModel.GetModelIsLocked(ref isLocked);
if (isLocked)
{
    _SapModel.SetModelIsLocked(false);
}
```

---

## Project Structure

```
AnalysisRunAutomation/
│
├── AnalysisRunAutomation/          # Main plugin project
│   ├── cPlugin.cs                  # Plugin entry point
│   ├── Form1.cs                    # Main UI form (controller)
│   ├── Form1.Designer.cs           # UI layout
│   │
│   ├── WallPlacementForm.cs        # Wall placement UI
│   ├── WallPlacementForm.Designer.cs
│   │
│   ├── SectionManager.cs           # Material & section creation
│   ├── LoadManager.cs              # Load patterns & combinations
│   ├── BoundaryManager.cs          # Boundary conditions
│   ├── DiaphragmManager.cs         # Floor diaphragm creation
│   ├── MeshManager.cs              # Finite element meshing
│   ├── SectionAssignmentManager.cs # Intelligent section assignment
│   ├── LoadAssignmentManager.cs    # Load application
│   ├── MassSourceManager.cs        # Mass source configuration
│   ├── AnalysisManager.cs          # Analysis execution & results
│   ├── WallPlacementManager.cs     # Wall creation logic
│   │
│   └── ETABS_Plugin.csproj         # Project file
│
└── README.md / DOCUMENTATION.md
```

---

## Manager Classes Reference

### 1. SectionManager

**Purpose**: Creates materials and section properties.

#### Key Methods

##### `CreateAllStandardSections(double concreteStrength)`
Creates complete set of materials and sections:
- Concrete material (custom strength)
- Steel material (S275)
- 6 column sizes: 300×300, 350×350, 400×400, 450×450, 500×500, 600×600
- 6 beam sizes: 300×600, 350×700, 400×800, 450×900, 500×1000, 600×1200
- 6 slab thicknesses: 100, 150, 200, 225, 250, 300 mm
- 8 wall thicknesses: 150, 200, 250, 300, 350, 400, 500, 600 mm

**ETABS API Calls**:
- `PropMaterial.AddMaterial()` - Creates material
- `PropMaterial.SetMPIsotropic()` - Sets elastic properties
- `PropMaterial.SetOConcrete_1()` - Sets concrete properties
- `PropFrame.SetRectangle()` - Creates rectangular sections
- `PropArea.SetSlab()` / `SetWall()` - Creates slab/wall sections

**Section Naming**:
```
PLUGIN_COL_400x400    (Columns)
PLUGIN_BEAM_300x600   (Beams)
PLUGIN_SLAB_200       (Slabs)
PLUGIN_WALL_250       (Walls)
```

---

### 2. LoadManager

**Purpose**: Creates load patterns and combinations.

#### Load Patterns

| Name | Type | Usage |
|------|------|-------|
| PLUGIN_DEAD | DEAD | Self-weight + superimposed dead |
| PLUGIN_LIVE | LIVE | Occupancy loads |
| PLUGIN_WIND | WIND | Wind loads |
| PLUGIN_SEISMIC | QUAKE | Seismic loads |

#### Load Combinations

**Ultimate Limit State (ULS)**:
```
ULS_1: 1.4 DL
ULS_2: 1.2 DL + 1.6 LL
ULS_3: 1.2 DL + LL + 1.2 WIND
ULS_4: 1.2 DL + LL + 1.0 SEISMIC
ULS_5: 1.0 DL + 1.0 SEISMIC
```

**Serviceability Limit State (SLS)**:
```
SLS_1: 1.0 DL
SLS_2: 1.0 DL + 1.0 LL
SLS_3: 1.0 DL + 0.5 LL + 1.0 WIND
SLS_4: 1.0 DL + 0.3 LL + 1.0 SEISMIC
SLS_5: 1.0 DL + 1.0 LL + 0.5 WIND
```

**ETABS API Calls**:
- `LoadPatterns.Add()` - Creates load pattern
- `RespCombo.Add()` - Creates combination
- `RespCombo.SetCaseList()` - Adds cases with factors

---

### 3. BoundaryManager

**Purpose**: Manages boundary conditions and restraints.

#### Key Methods

##### `ApplyFixedSupportsAtBase(double tolMm, out string report)`

**Algorithm**:
1. Get all joint coordinates
2. Find minimum Z elevation
3. Find joints within tolerance of minimum
4. Apply fixed restraints (all 6 DOFs)

**ETABS API Calls**:
- `PointObj.GetNameList()` - Gets all points
- `PointObj.GetCoordCartesian()` - Gets coordinates
- `PointObj.SetRestraint()` - Sets restraints

**Restraint Pattern**:
```csharp
// Fixed support: all 6 DOFs restrained
bool[] restraints = { true, true, true, true, true, true };
//                     UX    UY    UZ    RX    RY    RZ
```

---

### 4. DiaphragmManager

**Purpose**: Creates floor diaphragms with intelligent rigid/semi-rigid selection.

#### Diaphragm Intelligence

| Condition | Type | Reason |
|-----------|------|--------|
| Opening ratio > 30% | Semi-Rigid | Large openings reduce stiffness |
| Thick walls (>400mm) | Semi-Rigid | Thick cores may not act rigid |
| Few walls (<2) | Semi-Rigid | Insufficient lateral support |
| Normal floor | Rigid | Typical floor diaphragm |

**ETABS API Calls**:
- `Story.GetStories()` - Gets story info
- `Diaphragm.SetDiaphragm()` - Creates diaphragm
- `PointObj.SetDiaphragm()` - Assigns points

---

### 5. MeshManager

**Purpose**: Applies finite element mesh.

#### Recommended Mesh Sizes

| Element | Size | Reason |
|---------|------|--------|
| Walls | 0.5-1.0 m | Captures stress gradients |
| Slabs | 1.0-2.0 m | Balance accuracy/computation |
| Complex | 0.25-0.5 m | Better representation |

**ETABS API Calls**:
- `DatabaseTables.GetTableForEditingArray()` - Reads mesh table
- `DatabaseTables.SetTableForEditingArray()` - Updates mesh
- `Analyze.CreateAnalysisModel()` - Generates mesh

---

### 6. SectionAssignmentManager

**Purpose**: Intelligently assigns sections based on geometry.

#### Intelligence Algorithm

**For Frames**:
```csharp
double deltaZ = Math.Abs(j.Z - i.Z);
double deltaXY = Math.Sqrt(dx*dx + dy*dy);

if (deltaZ > deltaXY)
    return "COLUMN";  // Vertical
else
    return "BEAM";    // Horizontal
```

**For Areas**:
```csharp
Vector normal = CrossProduct(v1, v2);
if (Math.Abs(normal.Z) > 0.7)
    return "SLAB";   // Horizontal
else
    return "WALL";   // Vertical
```

**ETABS API Calls**:
- `FrameObj.SetSection()` - Assigns frame section
- `AreaObj.SetProperty()` - Assigns area property

---

### 7. LoadAssignmentManager

**Purpose**: Assigns loads to elements.

#### Standard Values

| Use | Live Load |
|-----|-----------|
| Residential | 2.0 kPa |
| Office | 3.0 kPa |
| Assembly | 5.0 kPa (plugin default) |

**ETABS API Calls**:
- `AreaObj.SetLoadUniform()` - Uniform load on area
- `FrameObj.SetLoadDistributed()` - Distributed load on frame

**Direction Codes**:
```csharp
X = 4, Y = 5, Z = 6
-X = 7, -Y = 8, -Z = 10 (gravity)
```

---

### 8. MassSourceManager

**Purpose**: Configures mass for dynamic analysis.

#### Seismic Mass

**Standard Practice**:
```
Seismic Mass = Dead Load + 0.3 × Live Load
```

**Why 30%?** Represents likely occupancy during earthquake.

**ETABS API Call**:
```csharp
_SapModel.PropMaterial.SetMassSource_1(
    "MyMassSource",
    massFromElements: true,
    massFromLoads: true,
    loadPattern: ["PLUGIN_DEAD", "PLUGIN_LIVE"],
    scaleFactor: [1.0, 0.3]
);
```

---

### 9. AnalysisManager

**Purpose**: Executes structural analysis.

#### Analysis Workflow

```
1. Create Analysis Model
   ├── Generate mesh elements
   ├── Create internal nodes
   └── Validate model

2. Run Analysis
   ├── Assemble stiffness matrix
   ├── Apply loads
   └── Solve equations

3. Retrieve Results
   ├── Displacements
   ├── Forces
   └── Reactions
```

**ETABS API Calls**:
- `Analyze.CreateAnalysisModel()` - Creates model
- `Analyze.RunAnalysis()` - Runs analysis
- `Results.JointReact()` - Gets reactions

**Common Errors**:

| Error | Cause | Solution |
|-------|-------|----------|
| "Unstable" | Missing supports | Check BCs |
| "Singular matrix" | Mechanisms | Add restraints |
| "Not converged" | Nonlinear issues | Check properties |

---

### 10. WallPlacementManager

**Purpose**: Places shear walls with properties.

#### Wall Placement

**ETABS API Sequence**:
```csharp
// 1. Create wall property
_SapModel.PropArea.SetWall(propName, type, material, thickness);

// 2. Create corner points
_SapModel.PointObj.AddCartesian(x, y, z, ref pname);

// 3. Create area object
_SapModel.AreaObj.AddByPoint(4, ref points, ref areaName);

// 4. Set property
_SapModel.AreaObj.SetProperty(areaName, propName);

// 5. Rotate local axes
_SapModel.AreaObj.SetLocalAxes(areaName, betaAngle);

// 6. Assign pier label
_SapModel.AreaObj.SetPier(areaName, pierLabel);
```

**Beta Angle**: Rotates local axis 2 for:
- Angled walls
- Consistent orientation
- Correct force directions

---

## Workflow Automation

### The 11-Step Workflow

```csharp
btnRunWorkflow_Click:
1. Create Materials (concrete, steel)
2. Create Frame Sections (columns, beams)
3. Create Area Sections (slabs, walls)
4. Create Load Patterns
5. Create Load Combinations
6. Apply Boundary Conditions (fixed base)
7. Create Diaphragms
8. Apply Mesh (1m)
9. Assign Sections (intelligent)
10. Assign Loads (5 kPa on slabs)
11. Setup Mass Source (seismic)
```

### Error Handling

**Critical Steps** (stop if fail):
- Material creation
- Load patterns
- Boundary conditions

**Non-Critical Steps** (warnings):
- Diaphragms
- Mesh
- Load assignment

**Status Symbols**:
- ✓ Success
- ⚠ Warning
- ✗ Error

---

## Developer Guide

### Setting Up

1. **Prerequisites**:
   - ETABS installed
   - Visual Studio 2019+
   - .NET Framework 4.7.2+

2. **Add COM Reference**:
   ```
   References → COM → CSI ETABS 1.0 Type Library
   ```

3. **Build**:
   ```
   Configuration: Release
   Platform: x64
   ```

4. **Install Plugin**:
   ```
   Copy DLL to: C:\Program Files\Computers and Structures\ETABS\Plugins\
   ```

### Creating New Manager

```csharp
public class MyNewManager
{
    private readonly cSapModel _SapModel;

    public MyNewManager(cSapModel sapModel)
    {
        _SapModel = sapModel;
    }

    public bool DoSomething(out string report)
    {
        var sb = new StringBuilder();

        // Save units
        eUnits originalUnits = eUnits.kN_m_C;
        _SapModel.GetPresentUnits(ref originalUnits);

        try
        {
            _SapModel.SetPresentUnits(eUnits.kN_m_C);

            // Your logic here

            report = sb.ToString();
            return true;
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error: {ex.Message}");
            report = sb.ToString();
            return false;
        }
        finally
        {
            _SapModel.SetPresentUnits(originalUnits);
        }
    }
}
```

### Best Practices

1. **Always use try-finally for units**
2. **Check model lock before operations**
3. **Validate API return codes**
4. **Initialize arrays before ref parameters**
5. **Use StringBuilder for reports**
6. **Provide detailed error messages**

---

## Usage Guide

### Basic Workflow

1. **Open ETABS model**
2. **Launch plugin**: Tools → Plugins → ETABS Workflow Plugin
3. **Configure**: Set concrete strength
4. **Run**: Click "Run Complete Workflow" or individual buttons
5. **Review**: Check Status window
6. **Analyze**: Click "▶ RUN ANALYSIS"

### Wall Placement

1. Click "Place Walls"
2. Enter 4 corner coordinates
3. Set elevation
4. Select material
5. Set thickness (150-600mm)
6. Enter beta angle
7. Enter pier label
8. Click "Place Wall"

**Coordinate Order**: Counter-clockwise

---

## Troubleshooting

### Common Issues

#### Plugin not appearing
- Verify DLL in plugins folder
- Check ETABS version compatibility
- Run ETABS as administrator

#### Model locked error
Already handled in managers, check custom code

#### Analysis fails "Unstable"
- Run "Apply Fixed BC"
- Check restraints in ETABS
- Ensure elements connected

#### Sections not assigned
- Check element geometry
- Review orientations
- Manually assign problematic elements

#### Mesh not applied
- Check area objects exist
- Try manual mesh in ETABS
- Update field indices for your ETABS version

---

## Appendix

### Unit Conversions

| From | To | Factor |
|------|-----|--------|
| m | mm | 1000 |
| kN | N | 1000 |
| MPa | N/mm² | 1 |
| kPa | kN/m² | 1 |

### Glossary

- **API**: Application Programming Interface
- **COM**: Component Object Model
- **ULS**: Ultimate Limit State
- **SLS**: Serviceability Limit State
- **DOF**: Degree of Freedom
- **Diaphragm**: Horizontal load-distributing element
- **Pier**: Vertical wall segment
- **Beta Angle**: Local axis rotation

---

**Document Version**: 1.0
**Last Updated**: 2025-01-07
**ETABS API Version**: v1
