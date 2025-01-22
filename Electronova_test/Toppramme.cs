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

            
            var walls = Brep.CreateBooleanDifference(Brep.CreateFromBox(outerBox), Brep.CreateFromBox(innerBox), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            var extendedWallBrep = Brep.CreateFromBox(extendedWall);

            var sideTriangles = new Triangle3d(
                new Point3d(0, 0, offsetHeight + height),
                new Point3d(0, width, offsetHeight + height),
                new Point3d(0, 0, offsetHeight + height + slantHeight)
                );

            var triangleMesh = sideTriangles.ToMesh();
            var triangleBrep = Brep.CreateFromMesh(triangleMesh, false);
        
            var breps = new List<Brep>
            {
                walls[0],
                extendedWallBrep,
                triangleBrep
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