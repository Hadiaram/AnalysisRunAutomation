// Target: net8.0-windows, x64
// References: ETABSv1 (CSiAPIv1) from your ETABS 22 installation

using System;
using ETABSv1;

namespace SetWalls;

public static class WallsPlacementDemo
{
    public static void Main()
    {
        // 1) Attach to (or start) ETABS
        cHelper helper = new ETABSv1.Helper();   // <- interface type
        cOAPI etabs = null;

        try
        {
            etabs = helper.GetObject("CSI.ETABS.API.ETABSObject");  // attach if ETABS is running
        }
        catch
        {
            // not running
        }

        if (etabs == null)
        {
            etabs = helper.CreateObjectProgID("CSI.ETABS.API.ETABSObject");
            etabs.ApplicationStart(); // starts ETABS
        }

        var sap = etabs.SapModel;

        // 2) Units & (optional) start a clean model
        int ret;
        ret = sap.SetPresentUnits(eUnits.kN_m_C);
        Check(ret, "SetPresentUnits");

        // Optional: new blank model
        // ret = sap.InitializeNewModel(eUnits.kN_m_C);
        // Check(ret, "InitializeNewModel");
        // ret = sap.File.NewBlank();
        // Check(ret, "NewBlank");

        // 3) Ensure material & wall property exist
        //    (Assumes a concrete material property "CONC" already exists; change to yours)
        string wallProp = "WALL_200mm_CONC";
        string material = "CONC";     // must exist in the model (or create it first)
        double thickness = 0.200;      // meters, because you set units to kN-m-C

        ret = sap.PropArea.SetWall(
            Name: wallProp,
            WallPropType: eWallPropType.Specified,   // NOT AutoSelectList unless you're actually using an ASL
            ShellType: eShellType.ShellThick,     // ShellThick is typical for walls (ShellThin also acceptable)
            MatProp: material,
            Thickness: thickness,
            color: -1,
            notes: "",
            GUID: ""
        );
        Check(ret, "PropArea.SetWall");


        // 4) Define a rectangular wall outline by adding points (Global CSys, at Z = story elevation)
        //    (If you already have point names, you can skip creating points)
        string p1 = "", p2 = "", p3 = "", p4 = "";
        double z = 3.0; // meters, example story elevation

        // PointObj.AddCartesian(x, y, z, ref name, userName?, CSys?) — adjust params per your interop
        ret = sap.PointObj.AddCartesian(0.0, 0.0, z, ref p1);
        Check(ret, "PointObj.AddCartesian p1");
        ret = sap.PointObj.AddCartesian(4.0, 0.0, z, ref p2);
        Check(ret, "PointObj.AddCartesian p2");
        ret = sap.PointObj.AddCartesian(4.0, 0.2, z, ref p3); // thin rectangle in plan; thickness is property, not geometry
        Check(ret, "PointObj.AddCartesian p3");
        ret = sap.PointObj.AddCartesian(0.0, 0.2, z, ref p4);
        Check(ret, "PointObj.AddCartesian p4");

        // 5) Create the wall area object by those points
        //    AreaObj.AddByPoint(ref name, ref pointNames[], propName?, userName?, CSys?, Story?)
        string[] loop = new[] { p1, p2, p3, p4 };
        string wallName = "";

        ret = sap.AreaObj.AddByPoint(
            NumberPoints: loop.Length,       // 4 here
            Point: ref loop,
            Name: ref wallName,
            PropName: wallProp,          // assign your wall section immediately
            UserName: ""                 // optional
        );
        Check(ret, "AreaObj.AddByPoint");


        // If your AddByPoint overload doesn’t accept propName, you can assign after creation:
        // ret = sap.AreaObj.SetProperty(wallName, wallProp);
        // Check(ret, "AreaObj.SetProperty");

        // 6) Orientation (local axes β)
        double betaAngleDeg = 90.0; // rotate local axes by 90°
        ret = sap.AreaObj.SetLocalAxes(wallName, betaAngleDeg);
        Check(ret, "AreaObj.SetLocalAxes");

        // 7) (Optional) Tag for design & reporting
        // Pier/Spandrel labels are used by design post-processing
        string pierLabel = "P1";
        ret = sap.AreaObj.SetPier(wallName, pierLabel);
        Check(ret, "AreaObj.SetPier");

        // For spandrels (e.g., coupling beams across openings) — optional:
        // string spLabel = "S1";
        // ret = sap.AreaObj.SetSpandrel(wallName, spLabel);
        // Check(ret, "AreaObj.SetSpandrel");

        // 8) (Optional) Add an opening (hole) inside the wall panel
        // Many interops expose AreaObj.AddHole(name, numberPoints, X[], Y[], Z[])
        // Here we place a small opening centered in the panel
        // Adjust signature/units to your interop; keep the hole polygon strictly inside the wall loop
        /*
        {
            int n = 4;
            double[] hx = { 1.8, 2.2, 2.2, 1.8 };
            double[] hy = { 0.05, 0.05, 0.15, 0.15 };
            double[] hz = { z, z, z, z };
            ret = sap.AreaObj.AddHole(wallName, n, ref hx, ref hy, ref hz);
            Check(ret, "AreaObj.AddHole");
        }
        */

        // 9) (Optional) Auto-mesh this panel (often better to batch and mesh later)
        // Toggle and set target size as needed in your version:
        // ret = sap.AreaObj.SetAutoMesh(wallName, true, 0.5 /* target element size (m), example */);
        // Check(ret, "AreaObj.SetAutoMesh");

        // 10) Save (optional)
        // ret = sap.File.Save(@"C:\Temp\WallsDemo.edb");
        // Check(ret, "File.Save");

        Console.WriteLine($"✅ Wall placed: {wallName}, property: {wallProp}, β = {betaAngleDeg} deg, Pier = {pierLabel}");
    }

    private static void Check(int ret, string where)
    {
        if (ret != 0)
            throw new ApplicationException($"{where} failed (ret={ret}).");
    }
}
