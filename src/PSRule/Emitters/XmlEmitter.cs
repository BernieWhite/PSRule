// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Xml;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// An <seealso cref="IEmitter"/> for processing XML.
/// </summary>
internal sealed class XmlEmitter : FileEmitter
{
    private const string FORMAT = "xml";

    private static readonly string[] _DefaultTypes = [".xml"];

    private readonly XmlReaderSettings _Settings;
    private readonly ILogger<XmlEmitter> _Logger;
    private readonly ImmutableHashSet<string> _Types;

    public XmlEmitter(ILogger<XmlEmitter> logger, IEmitterConfiguration emitterConfiguration)
    {
        if (emitterConfiguration == null) throw new ArgumentNullException(nameof(emitterConfiguration));

        _Logger = logger ?? throw new NullReferenceException(nameof(logger));

        _Settings = new XmlReaderSettings
        {

        };
    }

    /// <summary>
    /// Accept the file if it is an XML file.
    /// </summary>
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && _Types.Contains(info.Extension);
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
