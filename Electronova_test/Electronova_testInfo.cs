using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace Electronova_test
{
    public class Electronova_testInfo : GH_AssemblyInfo
    {
        public override string Name => "Electronova_test";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("bc111cb9-4478-4020-834d-52cc1d5e1204");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}