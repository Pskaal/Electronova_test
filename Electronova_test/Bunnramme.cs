using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Electronova_test
{
    public class Bunnramme : GH_Component
    {
        public Bunnramme()
          : base("Bunnramme", "BR",
              "Bunnramme til trekkekum",
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
            pManager.AddIntegerParameter("Number in excel file", "N", "Select kum with slider", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Bunnramme", "BR", "Bunnramme til trekkekum", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double width = 0;
            double height = 0;
            double length = 0;
            double wallThickness = 0;

            if (!DA.GetData(0, ref width)) return;
            if (!DA.GetData(1, ref height)) return;
            if (!DA.GetData(2, ref length)) return;
            if (!DA.GetData(3, ref wallThickness)) return;

            //Excel import
            int excelNumber = 0;
            if (!DA.GetData(4, ref excelNumber)) return;

            var excelData = ExcelImporter.ImportExcel();
            if (excelData != null & excelNumber != 0)
            {
                double.TryParse(excelData[excelNumber][2], out length);
                double.TryParse(excelData[excelNumber][3], out width);
            }

            //Quick fix since excel measurements seem to be inner, aka without the wallthickness
            length = length + 2 * wallThickness;
            width = width + 2 * wallThickness;

            // Create box. Basically doing outerBox minus innerBox to create empty shell "walls"
            var outerBox = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(0, length),
                new Interval(0, width), 
                new Interval(0, height));
            
            var innerBox = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis),
                new Interval(wallThickness, length - wallThickness),
                new Interval(wallThickness, width - wallThickness),
                new Interval(0, height));

            var walls = new Box().ToBrep();
            if (height != 0.00)
            {
                walls = Brep.CreateBooleanDifference(Brep.CreateFromBox(outerBox), Brep.CreateFromBox(innerBox), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
            }

            DA.SetData(0, walls);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("F4993A7B-6228-44A3-8C13-028237866477"); }
        }
    }
}