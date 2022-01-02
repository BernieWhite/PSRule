# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Validation rules for Azure Storage Accounts
#

# Synopsis: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' -If { ResourceType 'Microsoft.Storage/storageAccounts' } {
    Recommend 'Storage accounts should only allow secure traffic'

    $TargetObject.Properties.supportsHttpsTrafficOnly
}

# Synopsis: Use at-rest storage encryption
Rule 'storageAccounts.UseEncryption' -If { ResourceType 'Microsoft.Storage/storageAccounts' } {
    Recommend 'Storage accounts should have encryption enabled'

    ($Null -ne $TargetObject.Properties.encryption) -and
    ($Null -ne $TargetObject.Properties.encryption.services.blob) -and
    ($Null -ne $TargetObject.Properties.encryption.services.file) -and
    ($TargetObject.Properties.encryption.services.blob.enabled -and $TargetObject.Properties.encryption.services.file.enabled)
}
