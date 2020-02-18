using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DLG.ToolBox.Utils
{
    public class Localization
    {
        private static Xml _xml;

        public string langPath
        {
            get
            {
                var startDir = Environment.CommandLine;
                startDir = startDir.Substring(1, startDir.IndexOf('"', 1) - 1);
                startDir = startDir.Substring(0, startDir.LastIndexOf('\\') + 1);
                return startDir + "lang";
            }
        }

        public Localization()
        {
            Reload("english.xml");
        }

        public Localization(string lang)
        {
            Reload(lang);
        }
        public void Reload(string lang)
        {
            _xml = new Xml(Path.Combine(langPath, lang));
            _xml.ThisCanThrowExeptions = true;
            _xml.Reload();
        }

        public string getMessage(string msg)
        {
            if (_xml.GetNode("Messages") == null)
                return msg + " ";
            if (_xml.GetNode("Messages." + msg) != null)
                return _xml["Messages." + msg];
            else
                return msg;
        }

        public void setLocalization(Form targetForm)
        {
            if (_xml.GetNode(targetForm.Name) == null) return;

            var members = targetForm.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Field)
                {
                    var FType = targetForm.GetType().GetField(m.Name);
                    if (FType == null) continue;
                    if (FType.GetValue(targetForm) != null)
                    {
                        if (!FType.FieldType.FullName.StartsWith("System.Windows.Forms."))
                            continue;
                        var nodeName = targetForm.Name + "." + m.Name;
                        if (_xml.GetNode(nodeName) != null)
                        {
                            if (FType.FieldType.FullName.EndsWith("Button"))
                                ((Button)FType.GetValue(targetForm)).Text = _xml[nodeName];
                            else if (FType.FieldType.FullName.EndsWith("Label"))
                                ((Label)FType.GetValue(targetForm)).Text = _xml[nodeName];
                            else if (FType.FieldType.FullName.EndsWith("ToolStripMenuItem"))
                                ((ToolStripMenuItem)FType.GetValue(targetForm)).Text = _xml[nodeName];
                        }
                        else
                        {
                            if (FType.FieldType.FullName.EndsWith("Button"))
                                _xml[nodeName] = ((Button)FType.GetValue(targetForm)).Text;
                            else if (FType.FieldType.FullName.EndsWith("Label"))
                                _xml[nodeName] = ((Label)FType.GetValue(targetForm)).Text;
                            else if (FType.FieldType.FullName.EndsWith("ToolStripMenuItem"))
                                _xml[nodeName] = ((ToolStripMenuItem)FType.GetValue(targetForm)).Text;
                        }
                    }
                }
            }
        }
    }
}
