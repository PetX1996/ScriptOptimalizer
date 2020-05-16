using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptOptimalizer
{
    class XMLArg
    {
        public string Name { get; set; }
        public List<XMLAttribute> Attributes { get; set; }

        public XMLArg(string name) : this(name, new List<XMLAttribute>()) { }
        public XMLArg(string name, List<XMLAttribute> atts)
        {
            this.Name = name;
            this.Attributes = atts;
        }
    }

    class XMLAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public XMLAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
