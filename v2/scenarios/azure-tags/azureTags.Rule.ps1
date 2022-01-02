# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Validation rules for Azure resource tagging standard
#

if ($Null -eq $Configuration.allowedBusinessUnits) {
    Write-Warning -Message 'The allowedBusinessUnits option is not configured';
}

# Synopsis: Resource must have environment tag
Rule 'environmentTag' {
    Exists 'Tags.environment' -CaseSensitive
    Within 'Tags.environment' 'production', 'test', 'development' -CaseSensitive
}

# Synopsis: Resource must have costCentre tag
Rule 'costCentreTag' {
    Exists 'Tags.costCentre' -CaseSensitive
    Match 'Tags.costCentre' '^([1-9][0-9]{4})$'
}

# Synopsis: Resource must have businessUnit tag
Rule 'businessUnitTag' {
    Exists 'Tags.businessUnit' -CaseSensitive
    Within 'Tags.businessUnit' $Configuration.allowedBusinessUnits
}
