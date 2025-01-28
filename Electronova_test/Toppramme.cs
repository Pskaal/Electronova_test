using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;

namespace Electronova_test
{
    public class Toppramme : GH_Component
    {
        public Toppramme()
          : base("Toppramme", "TR",
              "Toppramme til trekkekum",
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
            pManager.AddNumberParameter("Bunnramme and Hoveddel offset", "BRHR-OFF", "Offset to compensate for Z offset of other parts", GH_ParamAccess.item);
            pManager.AddNumberParameter("Slant height", "SH", "Height of extended wall which causes slant in top", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number in excel file", "N", "Select kum with slider", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Toppramme", "BR", "Toppramme til trekkekum", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double width = 0;
            double height = 0;
            double length = 0;
            double wallThickness = 0;
            double offsetHeight = 0;
            double slantHeight = 0;
            

            if (!DA.GetData(0, ref width)) return;
            if (!DA.GetData(1, ref height)) return;
            if (!DA.GetData(2, ref length)) return;
            if (!DA.GetData(3, ref wallThickness)) return;
            if (!DA.GetData(4, ref offsetHeight)) return;
            if (!DA.GetData(5, ref slantHeight)) return;

            int excelNumber = 0;
            if (!DA.GetData(6, ref excelNumber)) return;

            var excelData = ExcelImporter.ImportExcel();
            if ( excelData != null & excelNumber != 0)
            {
                double.TryParse(excelData[excelNumber][2], out length);
                double.TryParse(excelData[excelNumber][3], out width);
            }

            //Quick fix since excel measurements seem to be inner, aka without the wallthickness
            length = length + 2*wallThickness;
            width = width + 2*wallThickness;

            // Create box. Basically doing outerBox minus innerBox to create empty shell "walls"
            var outerBox = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis), 
                new Interval(0, length),
                new Interval(0, width), 
                new Interval(offsetHeight, offsetHeight + height));
            
            var innerBox = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis),
                new Interval(wallThickness, length - wallThickness),
                new Interval(wallThickness, width - wallThickness),
                new Interval(offsetHeight, offsetHeight + height));

            var extendedWall = new Box(new Plane(Point3d.Origin, Vector3d.ZAxis),
                new Interval(0, length),
                new Interval(0, wallThickness),
                new Interval(offsetHeight + height, offsetHeight + height + slantHeight));

            var walls = new Box().ToBrep();
            var extendedWallBrep = Brep.CreateFromBox(extendedWall);
            if (height != 0.00)
            {
                walls = Brep.CreateBooleanDifference(Brep.CreateFromBox(outerBox), Brep.CreateFromBox(innerBox), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
                extendedWallBrep = Brep.CreateFromBox(extendedWall);
            }

            var sideTriangle1 = new Triangle3d(
                new Point3d(0, wallThickness, offsetHeight + height),
                new Point3d(0, width, offsetHeight + height),
                new Point3d(0, wallThickness, offsetHeight + height + slantHeight)
                );

            var sideTriangle2 = new Triangle3d(
                new Point3d(length - wallThickness, wallThickness, offsetHeight + height),
                new Point3d(length - wallThickness, width, offsetHeight + height),
                new Point3d(length - wallThickness, wallThickness, offsetHeight + height + slantHeight)
                );

            var triangleBrep1 = Brep.CreateFromMesh(sideTriangle1.ToMesh(), false);
            var triangleBrep2 = Brep.CreateFromMesh(sideTriangle2.ToMesh(), false);

            var breps = new List<Brep>
            {
                walls,
                extendedWallBrep,
                triangleBrep1,
                triangleBrep2
            };
                

            DA.SetDataList(0, breps);
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
            get { return new Guid("F4693A7B-6228-44A3-8C13-028237866477"); }
        }
    }
}