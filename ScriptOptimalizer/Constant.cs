using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptOptimalizer
{
    class Constant
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Constant(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            return "Constant \'"+this.Name+"\' = \'"+this.Value+"\'";
        }
    }

    class ConstantList : List<Constant>
    {
        public static void InitPermanentConstants()
        {
            Program.A_Constants.Add("__DATE__", "\"" + DateTime.Now.ToShortDateString() + "\"");
            Program.A_Constants.Add("__TIME__", "\"" + DateTime.Now.ToShortTimeString() + "\"");
            Program.A_Constants.Add("__FILE__");
            Program.A_Constants.Add("__FILEFULL__");
            Program.A_Constants.Add("__LINE__");
            Program.A_Constants.Add("__LINEFULL__");
            Program.A_Constants.Add("__FUNCTION__");
            Program.A_Constants.Add("__FUNCTIONFULL__");
            Program.A_Constants.Add("__INT_MIN__", "-2147483647");
            Program.A_Constants.Add("__INT_MAX__", "2147483647");
            Program.A_Constants.ToString();
        }

        public void Add(string name) { Add(name, ""); }
        public void Add(string name, string value)
        {
            int index = this.FindIndex(a => a.Name == name);
            if (index != -1)
            {
                Debug.Warning("Redefinition of constant \'" + name + "\' Line: "+Program.DEBUG_LineI);
                this.RemoveAt(index);
            }

            value = this.ProcessConstant(value);

            this.Add(new Constant(name, value));
        }

        public void Update(string name, string value)
        {
            int index = this.FindIndex(a => a.Name == name);
            if (index == -1)
                this.Add(name, value);
            else
                this[index].Value = value;
        }

        public override string ToString()
        {
            if (this.Count == 0)
                return "";

            Debug.Separator();
            Debug.WriteLine("--- Constant List ---");

            foreach (Constant c in this)
                Debug.WriteLine(c.ToString());

            return "";
        }

        private string ProcessConstant(string value)
        {
            if (value == "" || value[0] == '"')
                return value;

            foreach (Constant c in this)
                value = value.Replace(c.Name, c.Value);

            //value = Math.CalculateString(value);

            return value;
        }
    }
}
