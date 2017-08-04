using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPL.Rule.Core
{
    internal class RecursionVisitor
    {
        private Dictionary<string, XElement> ruleCache;

        private GetRuleDelegate getRule;

        private XNamespace ns;

        private XElement root;

        private Stack<string> recursionStack;

        public Stack<string> RecursionStack
        {
            get
            {
                return this.recursionStack;
            }
        }

        public RecursionVisitor(string ruleXml, GetRuleDelegate getRule = null)
        {
            this.getRule = getRule;
            this.ruleCache = new Dictionary<string, XElement>();
            this.root = this.LoadRuleset(ruleXml);
            this.recursionStack = new Stack<string>();
        }

        private XElement LoadRuleset(string ruleXml)
        {
            XElement xElement = XElement.Parse(ruleXml);
            this.ns = xElement.GetDefaultNamespace();
            XElement xElement2 = null;
            if (xElement.Name == this.ns + "codeeffects")
            {
                using (IEnumerator<XElement> enumerator = xElement.Elements(this.ns + "rule").GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        XElement current = enumerator.Current;
                        if (xElement2 == null)
                        {
                            xElement2 = current;
                        }
                        if (!this.ruleCache.ContainsKey((string)current.Attribute("id")))
                        {
                            this.ruleCache.Add((string)current.Attribute("id"), current);
                        }
                    }
                    return xElement2;
                }
            }
            if (!(xElement.Name == this.ns + "rule"))
            {
                throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleXMLIsInvalid, new string[0]);
            }
            xElement2 = xElement;
            if (!this.ruleCache.ContainsKey((string)xElement2.Attribute("id")))
            {
                this.ruleCache.Add((string)xElement2.Attribute("id"), xElement2);
            }
            return xElement2;
        }

        public bool HasRecursion()
        {
            this.recursionStack.Clear();
            return this.HasRecursion(this.root);
        }

        private bool HasRecursion(XElement root)
        {
            if (root == null)
            {
                return false;
            }
            if (this.recursionStack.Contains((string)root.Attribute("id")))
            {
                return true;
            }
            this.recursionStack.Push((string)root.Attribute("id"));
            XNamespace defaultNamespace = root.GetDefaultNamespace();
            foreach (XElement current in root.Descendants(defaultNamespace + "rule"))
            {
                XElement rule = this.GetRule((string)current.Attribute("id"));
                if (rule == null)
                {
                    throw new InvalidRuleException(InvalidRuleException.ErrorIds.ReferencedRuleNotFound, new string[]
					{
						(string)current.Attribute("id")
					});
                }
                if (this.HasRecursion(rule))
                {
                    return true;
                }
            }
            this.recursionStack.Pop();
            return false;
        }

        protected XElement GetRule(string ruleId)
        {
            XElement result = null;
            if (this.ruleCache.ContainsKey(ruleId))
            {
                return this.ruleCache[ruleId];
            }
            if (this.getRule != null)
            {
                string text = this.getRule(ruleId);
                if (!string.IsNullOrEmpty(text))
                {
                    result = this.LoadRuleset(text);
                }
            }
            return result;
        }
    }
}
