# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'Org.Az.Storage.UseHttps' -Type 'Microsoft.Storage/storageAccounts' -Tag @{ release = 'GA' } {
    $Assert.HasFieldValue($TargetObject, 'Properties.supportsHttpsTrafficOnly', $True);
}

# Synopsis: Require mandatory tags
Rule 'Org.Az.Resource.Tagging' {
    Exists 'tags.Environment'
    Exists 'tags.BusinessUnit'
    Exists 'tags.Department'
    Exists 'tags.CostCode'
}
