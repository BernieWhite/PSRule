# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Validation rules for Kubernetes resources
#

# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
