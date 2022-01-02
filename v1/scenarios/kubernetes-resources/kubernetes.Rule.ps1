# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Validation rules for Kubernetes resources
#

# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}

# Synopsis: Must have the app.kubernetes.io/version label
Rule 'metadata.Version' -Type 'Deployment', 'Service' {
    Exists 'metadata.labels.''app.kubernetes.io/version'''
}

# Synopsis: Must have the app.kubernetes.io/component label
Rule 'metadata.Component' -Type 'Deployment', 'Service' {
    Exists 'metadata.labels.''app.kubernetes.io/component'''
    Within 'metadata.labels.''app.kubernetes.io/component''' 'web', 'api', 'database', 'gateway' -CaseSensitive
}

# Synopsis: Deployments use a minimum of 2 replicas
Rule 'deployment.HasMinimumReplicas' -Type 'Deployment' {
    Exists 'spec.replicas'
    $TargetObject.spec.replicas -ge 2
}

# Synopsis: Deployments use specific tags
Rule 'deployment.NotLatestImage' -Type 'Deployment' {
    foreach ($container in $TargetObject.spec.template.spec.containers) {
        $container.image -like '*:*' -and
        $container.image -notlike '*:latest'
    }
}

# Synopsis: Resource requirements are set for each container
Rule 'deployment.ResourcesSet' -Type 'Deployment' {
    foreach ($container in $TargetObject.spec.template.spec.containers) {
        $container | Exists 'resources.requests.cpu'
        $container | Exists 'resources.requests.memory'
        $container | Exists 'resources.limits.cpu'
        $container | Exists 'resources.limits.memory'
    }
}
