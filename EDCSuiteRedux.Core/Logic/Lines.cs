using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDCSuiteRedux
{
    class Lines
    {
        private string _line = string.Empty;

        public string Line
        {
            get { return _line; }
            set { _line = value; }
        }

        public Lines(string line)
        {
            _line = line;
        }
    }
}
