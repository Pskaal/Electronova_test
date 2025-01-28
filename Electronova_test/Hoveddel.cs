using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Electronova_test
{
    public class Hoveddel : GH_Component
    {
        public Hoveddel()
          : base("Hoveddel", "HD",
              "Hoveddel til trekkekum",
              "Custom", "Shapes")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Width", "W", "Width of the base", GH_ParamAccess.item, 1200);
            pManager.AddNumberParameter("Height", "H", "Height of the base", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Length", "L", "Length of the base", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Wall thickness", "WT", "Thickness of walls", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Bunnramme offset", "BR OFF", "Z offset cause by Bunnramme", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number in excel file", "N", "Select kum with slider", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Hoveddel", "SB", "The created Hoveddel", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double width = 0;
            double height = 0;
            double length = 0;
            double wallThickness = 0;
            double bunnrammeHeight = 0;

            if (!DA.GetData(0, ref width)) return;
            if (!DA.GetData(1, ref height)) return;
            if (!DA.GetData(2, ref length)) return;
            if (!DA.GetData(3, ref wallThickness)) return;
            if (!DA.GetData(4, ref bunnrammeHeight)) return;

            //Excel import
            int excelNumber = 0;
            if (!DA.GetData(5, ref excelNumber)) return;

            var excelData = ExcelImporter.ImportExcel();
            if (excelData != null & excelNumber != 0)
            {
                double.TryParse(excelData[excelNumber][2], out length);
                double.TryParse(excelData[excelNumber][3], out width);
            }

            //Quick fix since excel measurements seem to be inner, aka without the wallthickness
            length = length + 2 * wallThickness;
            width = width + 2 * wallThickness;

            // Create box. OuterBox minus innerBox to create empty shell "walls"
            var outerBox = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(0, length), 
                new Interval(0, width), 
                new Interval(bunnrammeHeight, bunnrammeHeight + height));

            var innerBox = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis),
                new Interval(wallThickness, length - wallThickness),
                new Interval(wallThickness, width - wallThickness),
                new Interval(bunnrammeHeight, bunnrammeHeight + height));

            var walls = Brep.CreateBooleanDifference(Brep.CreateFromBox(outerBox), Brep.CreateFromBox(innerBox), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            //Probably a stupid way to make holes in walls, but it works.. All holes are 600 x 300 on all sides, so they are just hard coded here.
            var holeBox1 = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(length / 2 - 300, length / 2 + 300), 
                new Interval(0, wallThickness), 
                new Interval(bunnrammeHeight, bunnrammeHeight + 300));
            var holeBox2 = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(length / 2 - 300, length / 2 + 300), 
                new Interval(width - wallThickness, width), 
                new Interval(bunnrammeHeight, bunnrammeHeight + 300));
            var holeBox3 = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(0, wallThickness), 
                new Interval(width / 2 - 300, width / 2 + 300), 
                new Interval(bunnrammeHeight, bunnrammeHeight + 300));
            var holeBox4 = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(length - wallThickness, length), 
                new Interval(width / 2 - 300, width / 2 + 300), 
                new Interval(bunnrammeHeight, bunnrammeHeight + 300));

            var wallshole1 = Brep.CreateBooleanDifference(walls[0], Brep.CreateFromBox(holeBox1), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            var wallshole2 = Brep.CreateBooleanDifference(wallshole1[0], Brep.CreateFromBox(holeBox2), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            var wallshole3 = Brep.CreateBooleanDifference(wallshole2[0], Brep.CreateFromBox(holeBox3), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            var wallshole4 = Brep.CreateBooleanDifference(wallshole3[0], Brep.CreateFromBox(holeBox4), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);


            DA.SetData(0, wallshole4[0]);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can return a custom bitmap here, or null for the default icon.
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F5993A7B-6228-44A3-8C13-028237866477"); }
        }
    }
}