using System;
using System.Collections.Generic;
using System.Text;

namespace EDCSuiteRedux
{
    class SymbolAxesTranslator
    {

        private Int32 GetSymbolAxisXID(SymbolCollection curSymbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in curSymbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.X_axis_ID;
                }
            }
            return 0;
        }
        private Int32 GetSymbolAxisYID(SymbolCollection curSymbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in curSymbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.Y_axis_ID;
                }
            }
            return 0;
        }

        public bool GetAxisSymbols(SymbolCollection curSymbols, string symbolname, out string x_axis, out string y_axis, out string x_axis_description, out string y_axis_description, out string z_axis_description)
        {
            bool retval = false;
            x_axis = "";
            y_axis = "";
            x_axis_description = "x-axis";
            y_axis_description = "y-axis";
            z_axis_description = "z-axis";
            int xid = GetSymbolAxisXID(curSymbols, symbolname);
            y_axis_description = TranslateAxisID(xid);
            int yid = GetSymbolAxisYID(curSymbols, symbolname);
            x_axis_description = TranslateAxisID(yid);

            return retval;
        }

        public string TranslateAxisID(int id)
        {
            string retval = id.ToString("X4");
            return retval;
        }

        public string GetXaxisSymbol(string symbolname)
        {
            string retval = string.Empty;
            return retval;
        }

        public string GetYaxisSymbol(string symbolname)
        {
            string retval = string.Empty;
            
            return retval;
        }

    }
}
