# AnalysisRunAutomation

C# tooling for automating **ETABS** analysis runs via the CSI oAPI (plugin/external app).  
> Solution includes a Visual Studio solution (`ETABS_Plugin.sln`) and a C# project under `AnalysisRunAutomation/`. 🛠️

---

## What this does (at a glance)

- Provides a starting point to **connect to ETABS**, run analyses, and script repetitive tasks.
- Can be wired up as an **ETABS Plugin** or run as a **stand-alone .NET app** that attaches/starts ETABS.
- Intended for batch/guarded runs (e.g., iterate scenarios, run analysis, export results).  
  _Customize the code to match your firm’s workflow (export CSV/Excel, save model, etc.)._

> **NOTE:** Fill in the “Usage” and “Configuration” sections with your exact inputs/outputs once you finalize your flow.

---

## Repository layout

