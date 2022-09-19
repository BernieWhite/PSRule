# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Create a container, execute the validate-files.ps1 script in the child psrulesample directory
        and then remove the container.
#>

# build image
docker build --tag psrule:latest .

# docker run --rm psrule:latest
docker run -it --rm -v $PSScriptRoot/psrulesample:/src psrule:latest pwsh -file /src/validate-files.ps1

# Remove the image
docker image rm psrule
