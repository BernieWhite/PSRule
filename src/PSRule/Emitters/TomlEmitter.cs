// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// An <seealso cref="IEmitter"/> for processing TOML.
/// </summary>
internal sealed class TomlEmitter : FileEmitter
{
    private const string FORMAT = "toml";

    private static readonly string[] _DefaultTypes = [".toml"];

    private readonly ILogger<TomlEmitter> _Logger;
    private readonly ImmutableHashSet<string> _Types;

    public TomlEmitter(ILogger<TomlEmitter> logger, IEmitterConfiguration emitterConfiguration)
    {
        if (emitterConfiguration == null) throw new ArgumentNullException(nameof(emitterConfiguration));

        _Logger = logger ?? throw new NullReferenceException(nameof(logger));
    }

    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && _Types.Contains(info.Extension);
    }

    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        throw new NotImplementedException();
    }
}
