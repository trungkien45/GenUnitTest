using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GenUnitTestReport
{
    public class ProcessCSFile
    {
        public string GetUsing(string source)
        {
            string UsingNamespace = string.Empty;
            if (!isRemovedComment)
                source = RemoveCommemt(source);
            if (!isRemovedString)
                source = RemoveString(source);
            string namepattern = @"(\s|^)(?<namespace>(using\s+\S+;))";
            MatchCollection mc = Regex.Matches(source, namepattern);
            foreach (Match item in mc)
            {
                UsingNamespace += item.Groups["namespace"].Value+"\n";
            }
            return UsingNamespace;
        }
        public string GetNameSpace(string source)
        {
            if (!isRemovedComment)
                source = RemoveCommemt(source);
            if (!isRemovedString)
                source = RemoveString(source);
            string NameSpace = string.Empty;
            string namepattern = @"(\s|^)namespace\s+(?<namespace>\S+)\s+{";
            MatchCollection mc = Regex.Matches(source, namepattern);
            foreach (Match item in mc)
            {
                NameSpace = item.Groups["namespace"].Value;
            }
            return NameSpace;
        }
        public List<string> GetClassesName(string source)
        {
            List<string> ClassesName = new List<string>();
            string methodpattern = @"(\s|^)(public|private|protected)?\s+(partial)?\s*class\s+(?<classname>\S+)";
            MatchCollection mc = Regex.Matches(source, methodpattern);
            foreach (Match item in mc)
            {
                ClassesName.Add(item.Groups["classname"].Value);
            }
            return ClassesName;
        }
        bool isRemovedString = false;
        bool isRemovedComment = false;

        public string RemoveCommemt(string source)
        {
            isRemovedComment = true;
            string newline = @"\r\n?|\n";
            string blockComments = @"/\*(.*?)\*/";
            string lineComments = @"//(.*?)\r?\n";
            string strings = @"""((\\[^\n]|[^""\n])*)""";
            string verbatimStrings = @"@(""[^""]*"")+";
            string blankLine = @"^\s+$[\r\n]*";
            source = Regex.Replace(source, newline, "\r\n");
            string sourceCRLF = Regex.Replace(source, blankLine, string.Empty, RegexOptions.Multiline);
            string noComments = Regex.Replace(sourceCRLF,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : string.Empty;
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);
            string noBlankLine = Regex.Replace(noComments, blankLine, string.Empty, RegexOptions.Multiline);
            return noBlankLine;
        }
        public string RemoveString(string source)
        {
            isRemovedString = true;
            string stringPatern = "\".*\"";

            string stringNostring = Regex.Replace(source, stringPatern, "\"\"");
            return stringNostring;
        }

        public List<Method> GetPublicMethods(string source)
        {
            if (!isRemovedComment)
                source = RemoveCommemt(source);
            if (!isRemovedString)
                source = RemoveString(source);
            List<Method> methods = new List<Method>();

            string methodpattern = @"(\s|^)((?<accessmodifier>public)\s*(?<static>static)*\s*(?<async>async)*\s*(?<override>override)*\s*)(?<type>\S+)*\s+(?<name>\S+)\s*\(\s*(?<paramaters>[^\)]*\s*)\)\s*{";
            MatchCollection mc = Regex.Matches(source, methodpattern);
            foreach (Match item in mc)
            {
                Method method = new Method();
                method.AccessModifier = item.Groups["accessmodifier"].Value;
                method.Static = item.Groups["static"].Value;
                method.Async = item.Groups["async"].Value;
                method.Override = item.Groups["override"].Value;
                method.Type = item.Groups["type"].Value;
                method.Name = item.Groups["name"].Value;
                method.Parameters = GetParameters(item.Groups["paramaters"].Value);
                methods.Add(method);
            }
            return methods;
        }

        private List<Parameter> GetParameters(string value)
        {
            List<Parameter> parameters = new List<Parameter>();
            if (value.Length == 0)
                return parameters;
            string pattern = @"(?<type>[\S^=,]+)\s+(?<name>[^=,\s]+)\s*=*\s*[^,]*,*";
            MatchCollection mc = Regex.Matches(value, pattern);
            foreach (Match item in mc)
            {
                Parameter parameter = new Parameter();
                parameter.ParameterName = item.Groups["name"].Value;
                parameter.ParameterType = item.Groups["type"].Value;
                parameters.Add(parameter);
            }
            return parameters;
        }
    }
}
