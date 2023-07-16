# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: An example rule
Rule 'isFruit' {
    # An recommendation to display in output
    Recommend 'Fruit is only Apple, Orange and Pear'

    # An failure reason to display for non-fruit
    Reason "$($PSRule.TargetName) is not fruit."

    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
