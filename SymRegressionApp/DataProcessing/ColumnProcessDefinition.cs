//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace SymRegressionApp.DataProcessing
{
    public class ColumnProcessDefinition
    {
        public string Header { get; set; }
        public bool Ignore { get; set; }
        public bool ConvertToIntegers { get; set; }
        public bool ExpandIntegers { get; set; }
    }
}
