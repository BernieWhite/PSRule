# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: Automatic services should be running.
Rule 'PS.ServiceStarted' -With 'IsAutomaticService' {
    $status = $TargetObject.Status.ToString()
    $Assert.HasFieldValue($status, '.', 'Running')
}
