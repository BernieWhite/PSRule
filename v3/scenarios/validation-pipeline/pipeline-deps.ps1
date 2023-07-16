# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Install dependencies for connecting to PowerShell Gallery
if ($Null -eq (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser;
}

if ($Null -eq (Get-InstalledModule -Name PowerShellGet -MinimumVersion '2.2.1' -ErrorAction SilentlyContinue)) {
    Install-Module PowerShellGet -MinimumVersion '2.2.1' -Scope CurrentUser -Force -AllowClobber;
}
