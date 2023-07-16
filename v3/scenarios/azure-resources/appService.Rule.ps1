# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Validation rules for Azure App Services
#

# Synopsis: App Service Plan has multiple instances
Rule 'appServicePlan.MinInstanceCount' -If { ResourceType 'Microsoft.Web/serverfarms' } {
    Recommend 'Use at least two (2) instances'

    $TargetObject.Sku.capacity -ge 2
}

# Synopsis: Use at least a Standard App Service Plan
Rule 'appServicePlan.MinPlan' -If { ResourceType 'Microsoft.Web/serverfarms' } {
    Recommend 'Use a Standard or high plans for production services'

    ($TargetObject.Sku.tier -eq 'PremiumV2') -or
    ($TargetObject.Sku.tier -eq 'Premium') -or
    ($TargetObject.Sku.tier -eq 'Standard')
}

# Synopsis: Disable client affinity for stateless services
Rule 'appServiceApp.ARRAffinity' -If { ResourceType 'Microsoft.Web/sites' } {
    Recommend 'Disable ARR affinity when not required'

    $TargetObject.Properties.clientAffinityEnabled -eq $False
}

# Synopsis: Use HTTPS only
Rule 'appServiceApp.UseHTTPS' -If { ResourceType 'Microsoft.Web/sites' } {
    Recommend 'Disable HTTP when not required'

    $TargetObject.Properties.httpsOnly -eq $True
}
