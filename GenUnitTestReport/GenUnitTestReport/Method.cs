using System;
using System.Collections.Generic;

namespace GenUnitTestReport
{
    public struct Method: IComparable<Method>
    {
        public string AccessModifier
        {
            get; set;
        }
        public string Static
        {
            get; set;
        }
        public string Override
        {
            get; set;
        }
        public string Type
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public List<Parameter> Parameters
        {
            get; set;
        }

        public int CompareTo(Method other)
        {
            return Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            string s = Name + "(";
            if (Type != "")
                s = Type + " " + s;
            if (Override != "")
                s = Override + " " + s;
            if (Static != "")
                s = Static + " " + s;
            if (AccessModifier != "")
                s = AccessModifier + " " + s;
            if (Parameters.Count > 0)
            {
                foreach (var item in Parameters)
                {
                    s += item.ParameterType + " " + item.ParameterName + ", ";
                }
                s = s.Substring(0, s.Length - 2);
            }
            s += ")";
            return s;
        }
    }
}
