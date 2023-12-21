// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Emitters;

namespace PSRule.Pipeline.Emitters;

internal sealed class TomlEmitter : FileEmitter
{
    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        throw new NotImplementedException();
    }

    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        throw new NotImplementedException();
    }
}
