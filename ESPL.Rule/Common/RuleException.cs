using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ESPL.Rule.Common
{
    /// <summary>
    /// Base class for all rule-related exception types. This exception can be thrown on generic rule-related issues
    /// </summary>
    public class RuleException : Exception
    {
        private string number;

        private string message;

        /// <summary>
        /// Message containing details of the exception
        /// </summary>
        public override string Message
        {
            get
            {
                if (this.message == null)
                {
                    return base.Message;
                }
                return this.message;
            }
        }

        /// <summary>
        /// Internal Code Effects control number of the exception being thrown. Use this number when contacting customer support.
        /// </summary>
        public string Number
        {
            get
            {
                return this.number;
            }
        }

        /// <summary>
        /// Returns true if text is a properly formatted message id, i.e. "s100".
        /// </summary>
        /// <param name="text">A text to be checked if it is a message id.</param>
        /// <returns></returns>
        private bool IsMessageId(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            Regex regex = new Regex("^[a-z]+\\d+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(text);
        }

        /// <summary>
        /// The RuleException type is not intended for public use
        /// </summary>
        protected internal RuleException(string errorId, params string[] parameters)
            : base(errorId)
        {
            if (this.IsMessageId(errorId))
            {
                this.number = errorId.ToUpper();
                this.message = this.LoadMessage(errorId, parameters);
            }
        }

        /// <summary>
        /// Reads and formats an error message from the Errors.config file based on message id (tag).
        /// </summary>
        /// <param name="messageId">A unique message identifier (tag). It must be in the form of {s|v|e|i|m}{number}.</param>
        /// <param name="parameters">Optional parameters that may be used to format a message, similar to string.Format().</param>
        /// <returns>A formatted message string.</returns>
        private string LoadMessage(string messageId, string[] parameters)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Resource.Errors);
            XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("/codeeffects/values/" + messageId);
            if (xmlNode == null)
            {
                return "Generic error: " + messageId.ToUpper();
            }
            string text = string.Format("{0} (Error {1})", xmlNode.InnerText, messageId.ToUpper());
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    text = text.Replace("{" + i + "}", (parameters[i] == null) ? "[NULL]" : parameters[i]);
                }
            }
            return text;
        }
    }
}
