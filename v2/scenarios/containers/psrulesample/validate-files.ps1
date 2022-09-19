# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Create a PSRule AzRuleTemplate data file and run the PSRule.Rules.Azure module rules against the output.
#>

Get-AzRuleTemplateLink "$PSScriptRoot/template" | Export-AzRuleTemplateData -OutputPath "$PSScriptRoot/out"

Assert-PSRule -InputPath "$PSScriptRoot/out/" -Module 'PSRule.Rules.Azure' -As Summary
