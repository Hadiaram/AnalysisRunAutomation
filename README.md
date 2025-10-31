# ETABS Automation Plugin (Analysis + Walls) — .NET 8 (x64)

A single ETABS v22 plugin that:
- Automates **analysis runs**, and
- Adds a **Walls** workflow to define wall section properties, place wall area objects, set local-axis orientation, tag pier/spandrel, and optionally add openings

All features live under one UI (tabs: **Setup | Walls | Analysis | Logs**) and one ETABS add-in entry. The plugin targets **.NET 8 (net8.0-windows)** and **x64**.

---

## Why this exists

Wall geometry and properties directly affect analysis. Keeping **Walls** and **Analysis** in one plugin simplifies iteration, reduces duplication, and enforces consistent units, naming, and guardrails.

---

## Features

### Analysis (existing)
- Run analysis with consistent preferences and unit policy
- Logging and basic guards
- Optional headless console runner for CI/batch (if present in the repo)

### Walls (new)
- Define or ensure **wall section properties** (thickness, material) via `PropArea.SetWall`
- Create or update **wall area objects** by coordinates or points and assign a section
- Set **local axis (β angle)** for orientation
- Assign **Pier** and **Spandrel** labels for design grouping
- (Optional) Add **openings**
- Idempotent re-apply: updates existing walls rather than creating duplicates

---

## Solution structure

```
repo-root/
├─ src/
│  ├─ Etabs.Automation.Host            # ETABS add-in (UI) — the only assembly registered in ETABS
│  ├─ Etabs.Automation.Walls           # Class Library with wall specs + services
│  ├─ Etabs.Automation.Abstractions    # (Optional) Interfaces (e.g., IEtabsGateway) and DTOs
│  └─ Etabs.Automation.Console         # (Optional) Headless runner for CI/batch
└─ docs/
   ├─ ETABS_API_MAP.md                 # Method references and call sequences (optional)
   └─ SMOKE_TEST.md                    # Minimal scenario to verify end-to-end behavior (optional)
```

- Keep **one** ETABS add-in entry pointing to the **Host** assembly.
- The **Host** project references `Etabs.Automation.Walls` (Project Reference, Copy Local = true).
- Do **not** register `Walls.dll` with ETABS; it is loaded by the Host.

---

## ETABS API: what we call

Use an adapter (e.g., `IEtabsGateway`) to wrap raw API calls and centralize units, undo grouping, and selection restore.

**Wall sections (properties)**
- `cSapModel.PropArea.SetWall(...)`
- `cSapModel.PropArea.GetWall(...)`

**Wall objects (geometry)**
- `cSapModel.AreaObj.AddByCoord(...)` or `AddByPoint(...)`
- `cSapModel.AreaObj.SetProperty(name, propName)`
- `cSapModel.AreaObj.GetPoints(name, ...)` (for edits/verification)

**Orientation**
- `cSapModel.AreaObj.SetLocalAxes(name, angleDeg)`
- `cSapModel.AreaObj.GetDesignOrientation(name, ...)`

**Pier / Spandrel**
- `cSapModel.AreaObj.SetPier(name, label)`
- `cSapModel.AreaObj.SetSpandrel(name, label)`

**Openings**
- `cSapModel.AreaObj.SetOpening(name, true/false)` (or inner-loop approach as needed)

**(Later) Shear wall design**
- `cSapModel.DesignShearWall.*` (preferences, overwrites, results)

---

## Requirements

- ETABS v22 (64-bit)
- Windows 10/11 x64
- .NET 8 SDK installed
- Build platform: **x64**
- Target framework: **`net8.0-windows`** for Host and libraries

---

## Build

From the repo root:

```bash
dotnet build -c Release -p:Platform=x64
```

Outputs (example):
- `Etabs.Automation.Host.dll` (registered in ETABS)
- `Etabs.Automation.Walls.dll` (copied next to Host via Project Reference)
- Any additional dependencies emitted into the same output folder

---

## Install in ETABS

1. In ETABS: **Tools → Add-Ins → Manage Add-Ins…**
2. Add the path to the **Host** DLL (Release x64 output folder).
3. Do **not** register `Walls.dll` separately. Keep all assemblies side-by-side with the Host.

---

## Using the plugin

1. Start ETABS and open or create a model.
2. **Add-Ins → [Your Plugin Name] → Open**.
3. Tabs:
   - **Setup**: choose units and plugin preferences.
   - **Walls**:
     - Define/ensure a wall section (e.g., `WALL_200mm_C30`).
     - Enter outline coordinates (or pick points), pick Story, assign section.
     - Set **Local Axis β** for orientation.
     - (Optional) Assign **Pier/Spandrel** labels and set **Openings**.
     - Click **Apply**. All operations run in one ETABS Undo group.
   - **Analysis**: run analysis. If geometry changed, a “results dirty” indicator prompts for a re-run.
   - **Logs**: inspect detailed action and API messages.

---

## Idempotency and safety

- Each multi-step edit runs inside a single **Undo** group.
- Units standardized at entry (e.g., kN-m-C). UI may accept mm; conversion occurs in the adapter.
- Deterministic naming (e.g., `W_<Story>_<Index>`) ensures **upsert** behavior.
- Selection is saved/restored around DB-table calls if required by your ETABS version/quirks.
- Optionally defer meshing until the end of a batch for speed.

---

## Development notes

- Keep the **Walls** library UI-free and ETABS-agnostic where possible; depend only on abstractions (e.g., `IEtabsGateway`).
- The **Host** owns UI and ETABS registration.
- Avoid single-file publish and trimming for the add-in.
- If you add dependencies requiring binding redirects, place them in the Host’s config (if applicable to your hosting model).
- For batch/CI, an optional **Console** app can reference the same libraries.

---

## Minimal smoke test

1. Launch ETABS and open a blank model.
2. Open the plugin window and switch to **Walls**.
3. Create or ensure wall section `WALL_200mm_C30` (thickness 200 mm, material C30).
4. Place a rectangular wall on a given Story using coordinates and assign that section.
5. Set local axis β = 90 degrees.
6. Assign Pier label `P1`.
7. Check in ETABS: property, orientation, and pier are set; geometry exists as expected.
8. Switch to **Analysis** and run analysis.
9. Reopen **Walls**, change thickness to 250 mm, **Apply**, confirm update (no duplicates).

---

## Roadmap

- Mesh controls and per-panel overrides
- `DesignShearWall` preferences/overwrites UI and results extraction
- Bulk import (CSV/JSON) for walls and openings
- Orientation helpers (auto β from dominant edge direction)

---

## FAQ

**Q: Do I need to register `Walls.dll` in ETABS?**  
A: No. Register only the **Host** DLL. `Walls.dll` is loaded via Project Reference and sits next to the Host DLL.

**Q: My existing automation is a console app. Can I keep it?**  
A: Yes. Keep the console for headless runs. The Host add-in provides UI in ETABS and calls the same libraries.

---

## License

Add your license terms here (e.g., MIT, Apache-2.0).
