using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ANT
{
    public class antInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ANT";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;

            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("a932931d-2096-499d-9e1f-77b05a2d0549");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
