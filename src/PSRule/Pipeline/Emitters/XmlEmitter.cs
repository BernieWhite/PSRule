// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Xml;
using PSRule.Data;
using PSRule.Emitters;

namespace PSRule.Pipeline.Emitters
{
    internal sealed class XmlEmitter : FileEmitter
    {
        private const string EXTENSION_XML = ".xml";
        private readonly XmlReaderSettings _Settings;

        public XmlEmitter()
        {
            _Settings = new XmlReaderSettings
            {
                
            };
        }

        /// <summary>
        /// Accept the file if it is an XML file.
        /// </summary>
        protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
        {
            return info != null && info.Extension == EXTENSION_XML;
        }

        protected override bool VisitFile(IEmitterContext context, IFileStream stream)
        {
            var reader = stream.AsTextReader();
            if (reader == null) return false;


            var doc = XmlReader.Create(stream.AsTextReader());

            //doc.

            return false;
        }
    }
}
