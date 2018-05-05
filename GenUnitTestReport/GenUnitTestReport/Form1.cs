using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenUnitTestReport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        const string sourcepre = "using Microsoft.VisualStudio.TestTools.UnitTesting;\n" +
                                 "{0}\n" +
                                 "namespace UnitTest\n" +
                                 "{{\n" +
                                 "    [TestClass()]\n" +
                                 "    public class UnitTest{1}\n" +
                                 "    {{\n" +
                                 "        {1} {2};\n" +
                                 "{3}\n" +
                                 "    }}\n" +
                                 "}}";
        string classfilename = "";
        string usingname = "";
        private void btOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C# Source|*.cs";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
                classfilename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                try
                {
                    GetMethods(textBox1.Text);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
        List<Method> publicmethods;
        List<string> classname;
        string namespacename = "";
        private void GetMethods(string filepath)
        {
            clbFunction.Items.Clear();
            listBox1.Items.Clear();
            string fileContent = File.ReadAllText(filepath);
            ProcessCSFile processCSFile = new ProcessCSFile();
            publicmethods = processCSFile.GetPublicMethods(fileContent);
            publicmethods.Sort();
            classname = processCSFile.GetClassesName(fileContent);
            usingname = processCSFile.GetUsing(fileContent);
            namespacename = processCSFile.GetNameSpace(fileContent);
            if (namespacename.Length != 0)
                usingname += "\n" + "using " + namespacename + ";\n";
            for (int i = 0; i < classname.Count; i++)
            {
                listBox1.Items.Add(classname[i]);
            }

            for (int i = 0; i < publicmethods.Count; i++)
            {
                clbFunction.Items.Add(publicmethods[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                string s = GenMethodTest(publicmethods);
                string classnam = classname[listBox1.SelectedIndex];
                string soure = string.Format(sourcepre, usingname, classnam, "var" + classnam, s);
                textBox2.Text = soure.Replace("\n", "\r\n");
            }
        }

        private string GenMethodTest(List<Method> publicmethods)
        {
            string classnam = classname[listBox1.SelectedIndex];
            string pattern =
                "        [TestMethod()]\n" +
                "        public void TestMethodFor{0}()\n" +
                "        {{\n" +
                "            //PRE\n" +
                "            //PARA\n" +
                "{1}\n" +
                "            //CALL\n" +
                "            {2}\n" +
                "            //CHECK\n" +
                "            Assert.()\n" +
                "        }}\n";
            string result = "        private SetUp()\n" +
                "        {\n" +
                "        }\n";
            result += "        private TearDown()\n" +
                "        {\n" +
                "        }\n";
            int k = 0;
            for (int i = 0; i < publicmethods.Count; i++)
            {
                string s = "";
                Method item = publicmethods[i];
                if (i < publicmethods.Count - 1)
                {
                    Method nextitem = publicmethods[i + 1];
                    if (nextitem.Name == item.Name)
                        s = (++k).ToString();
                    else
                    {
                        if (k != 0){
                            s = (++k).ToString();
                            k = 0;
                        }
                    }
                }
                string ppt = "            {0}";
                string p = "";
                string r = "";
                string ps = "";
                foreach (var item1 in item.Parameters)
                {
                    string para = item1.ParameterType + " " + item1.ParameterName + " = ;";
                    p += string.Format(ppt, para + "\n");

                    ps += item1.ParameterName + ",";
                }
                if (ps.Length > 0)
                    ps = ps.Substring(0, ps.Length - 1);
                if (item.Static == "")
                    r = NewMethod(classnam, item, ps, "var");
                else
                    r = NewMethod(classnam, item, ps);
                string mrs = string.Format(pattern, item.Name + s, p, r);
                result += mrs;
            }
            return result;

        }

        private static string NewMethod(string classnam, Method item, string ps, string var = "")
        {
            string r;
            if (item.Type != "void")
                r = item.Type + " " + "returnValue = " + var + classnam + "." + item.Name + string.Format("({0});", ps);
            else
                r = "var" + classnam + "." + item.Name + string.Format("({0})", ps);
            return r;
        }
    }
}
