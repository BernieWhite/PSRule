# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Install PSRule module
if ($Null -eq (Get-InstalledModule -Name PSRule -MinimumVersion '2.1.0' -ErrorAction SilentlyContinue)) {
    Install-Module -Name PSRule -Scope CurrentUser -MinimumVersion '2.1.0' -Force;
}

# Validate files
$assertParams = @{
    Path = './.ps-rule/'
    Style = 'AzurePipelines'
    OutputFormat = 'NUnit3'
    OutputPath = 'reports/rule-report.xml'
}
$items = Get-ChildItem -Recurse -Path .\src\,.\tests\ -Include *.ps1,*.psd1,*.psm1,*.yaml;
$items | Assert-PSRule $assertParams -ErrorAction Stop;
