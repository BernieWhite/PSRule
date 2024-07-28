# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Unit tests for PSRule options
#

[CmdletBinding()]
param ()

BeforeAll {
    # Setup error handling
    $ErrorActionPreference = 'Stop';
    Set-StrictMode -Version latest;

    if ($Env:SYSTEM_DEBUG -eq 'true') {
        $VerbosePreference = 'Continue';
    }

    # Setup tests paths
    $rootPath = $PWD;

    Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule) -Force;

    $here = (Resolve-Path $PSScriptRoot).Path;
    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Tests/Options;
    Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
    $Null = New-Item -Path $outputPath -ItemType Directory -Force;
}

#region New-PSRuleOption

Describe 'New-PSRuleOption' -Tag 'Option','New-PSRuleOption' {
    BeforeAll {
        $emptyOptionsFilePath = (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml');
    }

    Context 'Use -Path' {
        It 'With file' {
            $filePath = Join-Path -Path $outputPath -ChildPath 'new.file/ps-rule.yaml';
            Set-PSRuleOption -Path $filePath -Force;
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Not -Throw;
        }

        It 'With directory' {
            $filePath = Join-Path -Path $outputPath -ChildPath 'new.file';
            Set-PSRuleOption -Path $filePath -Force;
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Not -Throw;
        }

        It 'With missing file' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'new-not-a-file.yaml');
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw;
        }

        It 'With missing directory' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'new-dir/ps-rule.yaml');
            { New-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw;
        }
    }

    Context 'Read Rule.Include' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Rule.Include | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Rule.Include' = 'rule1' };
            $option.Rule.Include | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option @{ 'Rule.Include' = 'rule1', 'rule2' };
            $option.Rule.Include | Should -BeIn 'rule1', 'rule2';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Rule.Include | Should -BeIn 'rule1';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Rule.Include | Should -BeIn 'rule1', 'rule2';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Rule.Include | Should -BeIn 'rule1';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_RULE_INCLUDE = 'rule1';
                $option = New-PSRuleOption;
                $option.Rule.Include | Should -BeIn 'rule1';

                # With array
                $Env:PSRULE_RULE_INCLUDE = 'rule1;rule2';
                $option = New-PSRuleOption;
                $option.Rule.Include | Should -BeIn 'rule1', 'rule2';
            }
            finally {
                Remove-Item 'Env:PSRULE_RULE_INCLUDE' -Force;
            }
        }
    }

    Context 'Read Rule.IncludeLocal' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Rule.IncludeLocal | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Rule.IncludeLocal' = $True };
            $option.Rule.IncludeLocal | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Rule.IncludeLocal | Should -Be $True;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_RULE_INCLUDELOCAL = 'true';
                $option = New-PSRuleOption;
                $option.Rule.IncludeLocal | Should -Be $True;

                # With int
                $Env:PSRULE_RULE_INCLUDELOCAL = '1';
                $option = New-PSRuleOption;
                $option.Rule.IncludeLocal | Should -Be $True;
            }
            finally {
                Remove-Item 'Env:PSRULE_RULE_INCLUDELOCAL' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -RuleIncludeLocal $True -Path $emptyOptionsFilePath;
            $option.Rule.IncludeLocal | Should -Be $True;
        }
    }

    Context 'Read Rule.Exclude' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Rule.Exclude | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Rule.Exclude' = 'rule3' };
            $option.Rule.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option @{ 'Rule.Exclude' = 'rule3', 'rule4' };
            $option.Rule.Exclude | Should -BeIn 'rule3', 'rule4';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Rule.Exclude | Should -BeIn 'rule3';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Rule.Exclude | Should -BeIn 'rule3', 'rule4';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Rule.Exclude | Should -BeIn 'rule3';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_RULE_EXCLUDE = 'rule3';
                $option = New-PSRuleOption;
                $option.Rule.Exclude | Should -BeIn 'rule3';

                # With array
                $Env:PSRULE_RULE_EXCLUDE = 'rule3;rule4';
                $option = New-PSRuleOption;
                $option.Rule.Exclude | Should -BeIn 'rule3', 'rule4';
            }
            finally {
                Remove-Item 'Env:PSRULE_RULE_EXCLUDE' -Force;
            }
        }
    }

    Context 'Read Rule.Tag' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Rule.Tag | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Rule.Tag' = @{ key1 = 'rule3' } };
            $option.Rule.Tag.key1 | Should -Be 'rule3';

            # With array
            $option = New-PSRuleOption -Option @{ 'Rule.Tag' = @{ key1 = 'rule3', 'rule4' } };
            $option.Rule.Tag.key1 | Should -BeIn 'rule3', 'rule4';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Rule.Tag.key1 | Should -Be 'value1';
        }
    }

    Context 'Read Configuration' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Configuration.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -BaselineConfiguration @{ 'option1' = 'option'; 'option2' = 2; option3 = 'option3a', 'option3b' };
            $option.Configuration.option1 | Should -BeIn 'option';
            $option.Configuration.option2 | Should -Be 2;
            $option.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Configuration.option1 | Should -Be 'option';
            $option.Configuration.option2 | Should -Be 2;
            $option.Configuration.option3 | Should -BeIn 'option3a', 'option3b';
            $option.Configuration.option4.Length | Should -Be 2;
            $option.Configuration.option4[0].location | Should -Be 'East US';
            $option.Configuration.option4[0].zones | Should -BeIn '1', '2', '3';
            $option.Configuration.option5 | Should -BeIn 'option5a', 'option5b';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_CONFIGURATION_OPTION1 = 'value1';
                $Env:PSRULE_CONFIGURATION_OPTION2_NAME = 'value2';
                $option = New-PSRuleOption;
                $option.Configuration.option1 | Should -Be 'value1';
                $option.Configuration.option2_name | Should -Be 'value2';
            }
            finally {
                Remove-Item 'Env:PSRULE_CONFIGURATION_OPTION1' -Force;
                Remove-Item 'Env:PSRULE_CONFIGURATION_OPTION2_NAME' -Force;
            }
        }
    }

    Context 'Read Baseline.Group' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Baseline.Group | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Baseline.Group' = @{ 'test' = 'Test123' } };
            $option.Baseline.Group.Count | Should -Be 1;
            $option.Baseline.Group['test'] | Should -Not -BeNullOrEmpty;
            $option.Baseline.Group['test'][0] | Should -Be 'Test123';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Baseline.Group.Count | Should -Be 1;
            $option.Baseline.Group['latest'] | Should -Not -BeNullOrEmpty;
            $option.Baseline.Group['latest'][0] | Should -Be '.\TestBaseline1';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_BASELINE_GROUP = 'latest=YourBaseline';
                $option = New-PSRuleOption;
                $option.Baseline.Group.Count | Should -Be 1;
                $option.Baseline.Group['latest'] | Should -Not -BeNullOrEmpty;
                $option.Baseline.Group['latest'][0] | Should -Be 'YourBaseline';
            }
            finally {
                Remove-Item 'Env:PSRULE_BASELINE_GROUP' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BaselineGroup @{ test = 'Test123' } -Path $emptyOptionsFilePath;
            $option.Baseline.Group.Count | Should -Be 1;
            $option.Baseline.Group['test'] | Should -Not -BeNullOrEmpty;
            $option.Baseline.Group['test'][0] | Should -Be 'Test123';
        }
    }

    Context 'Read Binding.Field' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.Field | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Binding.Field' = @{ id = 'resourceId' } };
            $option.Binding.Field | Should -Not -BeNullOrEmpty;
            $option.Binding.Field.id.Length | Should -Be 1;
            $option.Binding.Field.id[0] | Should -Be 'resourceId';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.Field | Should -Not -BeNullOrEmpty;
            $option.Binding.Field.id.Length | Should -Be 1;
            $option.Binding.Field.id[0] | Should -Be 'resourceId';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingField @{ id = 'resourceId' } -Path $emptyOptionsFilePath;
            $option.Binding.Field | Should -Not -BeNullOrEmpty;
            $option.Binding.Field.id.Length | Should -Be 1;
        }
    }

    Context 'Read Binding.IgnoreCase' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.IgnoreCase | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Binding.IgnoreCase' = $False };
            $option.Binding.IgnoreCase | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.IgnoreCase | Should -Be $False;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_BINDING_IGNORECASE = 'false';
                $option = New-PSRuleOption;
                $option.Binding.IgnoreCase | Should -Be $False;

                # With int
                $Env:PSRULE_BINDING_IGNORECASE = '0';
                $option = New-PSRuleOption;
                $option.Binding.IgnoreCase | Should -Be $False;
            }
            finally {
                Remove-Item 'Env:PSRULE_BINDING_IGNORECASE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingIgnoreCase $False -Path $emptyOptionsFilePath;
            $option.Binding.IgnoreCase | Should -Be $False;
        }
    }

    Context 'Read Binding.NameSeparator' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.NameSeparator | Should -Be '/';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Binding.NameSeparator' = 'zz' };
            $option.Binding.NameSeparator | Should -Be 'zz';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.NameSeparator | Should -Be '::';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_BINDING_NAMESEPARATOR = '::';
                $option = New-PSRuleOption;
                $option.Binding.NameSeparator | Should -Be '::';
            }
            finally {
                Remove-Item 'Env:PSRULE_BINDING_NAMESEPARATOR' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingNameSeparator 'zz' -Path $emptyOptionsFilePath;
            $option.Binding.NameSeparator | Should -Be 'zz';
        }
    }

    Context 'Read Binding.PreferTargetInfo' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.PreferTargetInfo | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Binding.PreferTargetInfo' = $True };
            $option.Binding.PreferTargetInfo | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.PreferTargetInfo | Should -Be $True;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_BINDING_PREFERTARGETINFO = 'true';
                $option = New-PSRuleOption;
                $option.Binding.PreferTargetInfo | Should -Be $True;

                # With int
                $Env:PSRULE_BINDING_PREFERTARGETINFO = '1';
                $option = New-PSRuleOption;
                $option.Binding.PreferTargetInfo | Should -Be $True;
            }
            finally {
                Remove-Item 'Env:PSRULE_BINDING_PREFERTARGETINFO' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingPreferTargetInfo $True -Path $emptyOptionsFilePath;
            $option.Binding.PreferTargetInfo | Should -Be $True;
        }
    }

    Context 'Read Binding.TargetName' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.TargetName | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName' };
            $option.Binding.TargetName | Should -BeIn 'ResourceName';

            # With array
            $option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' };
            $option.Binding.TargetName.Length | Should -Be 2;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Binding.TargetName | Should -BeIn 'ResourceName';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_BINDING_TARGETNAME = 'ResourceName';
                $option = New-PSRuleOption;
                $option.Binding.TargetName | Should -BeIn 'ResourceName';

                # With array
                $Env:PSRULE_BINDING_TARGETNAME = 'ResourceName;AlternateName';
                $option = New-PSRuleOption;
                $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
            }
            finally {
                Remove-Item 'Env:PSRULE_BINDING_TARGETNAME' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -TargetName 'ResourceName', 'AlternateName' -Path $emptyOptionsFilePath;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }
    }

    Context 'Read Binding.TargetType' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.TargetType | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'ResourceType' };
            $option.Binding.TargetType | Should -BeIn 'ResourceType';

            # With array
            $option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'ResourceType', 'Kind' };
            $option.Binding.TargetType.Length | Should -Be 2;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.TargetType | Should -BeIn 'ResourceType';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Binding.TargetType | Should -BeIn 'ResourceType';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_BINDING_TARGETTYPE = 'ResourceType';
                $option = New-PSRuleOption;
                $option.Binding.TargetType | Should -BeIn 'ResourceType';

                # With array
                $Env:PSRULE_BINDING_TARGETTYPE = 'ResourceType;Kind';
                $option = New-PSRuleOption;
                $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
            }
            finally {
                Remove-Item 'Env:PSRULE_BINDING_TARGETTYPE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -TargetType 'ResourceType', 'Kind' -Path $emptyOptionsFilePath;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }
    }

    Context 'Read Binding.UseQualifiedName' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Binding.UseQualifiedName | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Binding.UseQualifiedName' = $True };
            $option.Binding.UseQualifiedName | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Binding.UseQualifiedName | Should -Be $True;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_BINDING_USEQUALIFIEDNAME = 'true';
                $option = New-PSRuleOption;
                $option.Binding.UseQualifiedName | Should -Be $True;

                # With int
                $Env:PSRULE_BINDING_USEQUALIFIEDNAME = '1';
                $option = New-PSRuleOption;
                $option.Binding.UseQualifiedName | Should -Be $True;
            }
            finally {
                Remove-Item 'Env:PSRULE_BINDING_USEQUALIFIEDNAME' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -BindingUseQualifiedName $True -Path $emptyOptionsFilePath;
            $option.Binding.UseQualifiedName | Should -Be $True;
        }
    }

    Context 'Read Convention.Include' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Convention.Include | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Convention.Include' = 'Convention1' };
            $option.Convention.Include | Should -BeIn 'Convention1';

            # With array
            $option = New-PSRuleOption -Option @{ 'Convention.Include' = 'Convention1', 'Convention2' };
            $option.Convention.Include.Length | Should -Be 2;
            $option.Convention.Include | Should -BeIn 'Convention1', 'Convention2';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Convention.Include | Should -BeIn 'Convention1';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Convention.Include | Should -BeIn 'Convention1', 'Convention2';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Convention.Include | Should -BeIn 'Convention3';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_CONVENTION_INCLUDE = 'Convention1';
                $option = New-PSRuleOption;
                $option.Convention.Include | Should -Be 'Convention1';

                # With array
                $Env:PSRULE_CONVENTION_INCLUDE = 'Convention1;Convention2';
                $option = New-PSRuleOption;
                $option.Convention.Include | Should -Be 'Convention1', 'Convention2';
            }
            finally {
                Remove-Item 'Env:PSRULE_CONVENTION_INCLUDE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -Convention 'Convention1', 'Convention2' -Path $emptyOptionsFilePath;
            $option.Convention.Include | Should -BeIn 'Convention1', 'Convention2';
        }
    }

    Context 'Read Execution.AliasReference' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.AliasReference | Should -Be 'Warn';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.AliasReference' = 'error' };
            $option.Execution.AliasReference | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.AliasReference' = 'Error' };
            $option.Execution.AliasReference | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.AliasReference | Should -Be 'Ignore';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_ALIASREFERENCE = 'error';
                $option = New-PSRuleOption;
                $option.Execution.AliasReference | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_ALIASREFERENCE = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.AliasReference | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_ALIASREFERENCE = '3';
                $option = New-PSRuleOption;
                $option.Execution.AliasReference | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_ALIASREFERENCE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionAliasReference 'Error' -Path $emptyOptionsFilePath;
            $option.Execution.AliasReference | Should -Be 'Error';
        }
    }

    Context 'Read Execution.Break' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.Break | Should -Be 'OnError';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.Break' = 'never' };
            $option.Execution.Break | Should -Be 'Never';

            $option = New-PSRuleOption -Option @{ 'Execution.Break' = 'Never' };
            $option.Execution.Break | Should -Be 'Never';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests16.yml');
            $option.Execution.Break | Should -Be 'Never';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_BREAK = 'never';
                $option = New-PSRuleOption;
                $option.Execution.Break | Should -Be 'Never';

                $Env:PSRULE_EXECUTION_BREAK = 'never';
                $option = New-PSRuleOption;
                $option.Execution.Break | Should -Be 'Never';

                # With int
                $Env:PSRULE_EXECUTION_BREAK = '1';
                $option = New-PSRuleOption;
                $option.Execution.Break | Should -Be 'Never';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_BREAK' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionBreak 'Never' -Path $emptyOptionsFilePath;
            $option.Execution.Break | Should -Be 'Never';
        }
    }

    Context 'Read Execution.DuplicateResourceId' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.DuplicateResourceId | Should -Be 'Error'
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.DuplicateResourceId' = 'warn' };
            $option.Execution.DuplicateResourceId | Should -Be 'Warn';

            $option = New-PSRuleOption -Option @{ 'Execution.DuplicateResourceId' = 'Warn' };
            $option.Execution.DuplicateResourceId | Should -Be 'Warn';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.DuplicateResourceId | Should -Be 'Warn';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_DUPLICATERESOURCEID = 'warn';
                $option = New-PSRuleOption;
                $option.Execution.DuplicateResourceId | Should -Be 'Warn';

                $Env:PSRULE_EXECUTION_DUPLICATERESOURCEID = 'Warn';
                $option = New-PSRuleOption;
                $option.Execution.DuplicateResourceId | Should -Be 'Warn';

                # With int
                $Env:PSRULE_EXECUTION_DUPLICATERESOURCEID = '2';
                $option = New-PSRuleOption;
                $option.Execution.DuplicateResourceId | Should -Be 'Warn';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_DUPLICATERESOURCEID' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -DuplicateResourceId 'Warn' -Path $emptyOptionsFilePath;
            $option.Execution.DuplicateResourceId | Should -Be 'Warn';
        }
    }

    Context 'Read Execution.HashAlgorithm' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.HashAlgorithm | Should -Be 'SHA512';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.HashAlgorithm' = 'SHA256' };
            $option.Execution.HashAlgorithm | Should -Be 'SHA256';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.HashAlgorithm | Should -Be 'SHA256';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_EXECUTION_HASHALGORITHM = 'SHA256';
                $option = New-PSRuleOption;
                $option.Execution.HashAlgorithm | Should -Be 'SHA256';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_HASHALGORITHM' -Force;
            }
        }
    }

    Context 'Read Execution.LanguageMode' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.LanguageMode | Should -Be 'FullLanguage';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_EXECUTION_LANGUAGEMODE = 'ConstrainedLanguage';
                $option = New-PSRuleOption;
                $option.Execution.LanguageMode | Should -Be 'ConstrainedLanguage';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_LANGUAGEMODE' -Force;
            }
        }
    }

    Context 'Read Execution.SuppressionGroupExpired' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.SuppressionGroupExpired | Should -Be 'Warn'
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.SuppressionGroupExpired' = 'error' };
            $option.Execution.SuppressionGroupExpired | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.SuppressionGroupExpired' = 'Error' };
            $option.Execution.SuppressionGroupExpired | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.SuppressionGroupExpired | Should -Be 'Debug';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED = 'error';
                $option = New-PSRuleOption;
                $option.Execution.SuppressionGroupExpired | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.SuppressionGroupExpired | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED = '3';
                $option = New-PSRuleOption;
                $option.Execution.SuppressionGroupExpired | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED' -Force;
            }
        }
    }

    Context 'Read Execution.RuleExcluded' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.RuleExcluded | Should -Be 'Ignore'
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.RuleExcluded' = 'error' };
            $option.Execution.RuleExcluded | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.RuleExcluded' = 'Error' };
            $option.Execution.RuleExcluded | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.RuleExcluded | Should -Be 'Warn';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_RULEEXCLUDED = 'error';
                $option = New-PSRuleOption;
                $option.Execution.RuleExcluded | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_RULEEXCLUDED = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.RuleExcluded | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_RULEEXCLUDED = '3';
                $option = New-PSRuleOption;
                $option.Execution.RuleExcluded | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_RULEEXCLUDED' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionRuleExcluded 'Error' -Path $emptyOptionsFilePath;
            $option.Execution.RuleExcluded | Should -Be 'Error';
        }
    }

    Context 'Read Execution.RuleSuppressed' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.RuleSuppressed | Should -Be 'Warn'
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.RuleSuppressed' = 'error' };
            $option.Execution.RuleSuppressed | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.RuleSuppressed' = 'Error' };
            $option.Execution.RuleSuppressed | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.RuleSuppressed | Should -Be 'Error';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_RULESUPPRESSED = 'error';
                $option = New-PSRuleOption;
                $option.Execution.RuleSuppressed | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_RULESUPPRESSED = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.RuleSuppressed | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_RULESUPPRESSED = '3';
                $option = New-PSRuleOption;
                $option.Execution.RuleSuppressed | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_RULESUPPRESSED' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionRuleSuppressed 'Error' -Path $emptyOptionsFilePath;
            $option.Execution.RuleSuppressed | Should -Be 'Error';
        }
    }

    Context 'Read Execution.RuleInconclusive' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.RuleInconclusive | Should -Be 'Warn';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.RuleInconclusive' = 'error' };
            $option.Execution.RuleInconclusive | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.RuleInconclusive' = 'Error' };
            $option.Execution.RuleInconclusive | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.RuleInconclusive | Should -Be 'Ignore';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_RULEINCONCLUSIVE = 'error';
                $option = New-PSRuleOption;
                $option.Execution.RuleInconclusive | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_RULEINCONCLUSIVE = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.RuleInconclusive | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_RULEINCONCLUSIVE = '3';
                $option = New-PSRuleOption;
                $option.Execution.RuleInconclusive | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_RULEINCONCLUSIVE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionRuleInconclusive 'Error' -Path $emptyOptionsFilePath;
            $option.Execution.RuleInconclusive | Should -Be 'Error';
        }
    }

    Context 'Read Execution.InvariantCulture' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.InvariantCulture | Should -Be 'Warn';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.InvariantCulture' = 'error' };
            $option.Execution.InvariantCulture | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.InvariantCulture' = 'Error' };
            $option.Execution.InvariantCulture | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.InvariantCulture | Should -Be 'Ignore';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_INVARIANTCULTURE = 'error';
                $option = New-PSRuleOption;
                $option.Execution.InvariantCulture | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_INVARIANTCULTURE = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.InvariantCulture | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_INVARIANTCULTURE = '3';
                $option = New-PSRuleOption;
                $option.Execution.InvariantCulture | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_INVARIANTCULTURE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionInvariantCulture 'Error' -Path $emptyOptionsFilePath;
            $option.Execution.InvariantCulture | Should -Be 'Error';
        }
    }

    Context 'Read Execution.UnprocessedObject' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.UnprocessedObject | Should -Be 'Warn';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.UnprocessedObject' = 'error' };
            $option.Execution.UnprocessedObject | Should -Be 'Error';

            $option = New-PSRuleOption -Option @{ 'Execution.UnprocessedObject' = 'Error' };
            $option.Execution.UnprocessedObject | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.UnprocessedObject | Should -Be 'Ignore';
        }

        It 'from Environment' {
            try {
                # With enum
                $Env:PSRULE_EXECUTION_UNPROCESSEDOBJECT = 'error';
                $option = New-PSRuleOption;
                $option.Execution.UnprocessedObject | Should -Be 'Error';

                # With enum
                $Env:PSRULE_EXECUTION_UNPROCESSEDOBJECT = 'Error';
                $option = New-PSRuleOption;
                $option.Execution.UnprocessedObject | Should -Be 'Error';

                # With int
                $Env:PSRULE_EXECUTION_UNPROCESSEDOBJECT = '3';
                $option = New-PSRuleOption;
                $option.Execution.UnprocessedObject | Should -Be 'Error';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_UNPROCESSEDOBJECT' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ExecutionUnprocessedObject 'Error' -Path $emptyOptionsFilePath;
            $option.Execution.UnprocessedObject | Should -Be 'Error';
        }
    }

    Context 'Read Execution.InitialSessionState' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.InitialSessionState | Should -Be 'BuiltIn';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.InitialSessionState' = 'Minimal' };
            $option.Execution.InitialSessionState | Should -Be 'Minimal';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.InitialSessionState | Should -Be 'Minimal';
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_EXECUTION_INITIALSESSIONSTATE = 'minimal';
                $option = New-PSRuleOption;
                $option.Execution.InitialSessionState | Should -Be 'Minimal';

                # With int
                $Env:PSRULE_EXECUTION_INITIALSESSIONSTATE = '1';
                $option = New-PSRuleOption;
                $option.Execution.InitialSessionState | Should -Be 'Minimal';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_INITIALSESSIONSTATE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InitialSessionState 'Minimal' -Path $emptyOptionsFilePath;
            $option.Execution.InitialSessionState | Should -Be 'Minimal';
        }
    }

    Context 'Read Execution.RestrictScriptSource' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Execution.RestrictScriptSource | Should -Be 'Unrestricted';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Execution.RestrictScriptSource' = 'ModuleOnly' };
            $option.Execution.RestrictScriptSource | Should -Be 'ModuleOnly';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Execution.RestrictScriptSource | Should -Be 'ModuleOnly';
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE = 'moduleonly';
                $option = New-PSRuleOption;
                $option.Execution.RestrictScriptSource | Should -Be 'ModuleOnly';

                # With int
                $Env:PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE = '1';
                $option = New-PSRuleOption;
                $option.Execution.RestrictScriptSource | Should -Be 'ModuleOnly';
            }
            finally {
                Remove-Item 'Env:PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -RestrictScriptSource 'ModuleOnly' -Path $emptyOptionsFilePath;
            $option.Execution.RestrictScriptSource | Should -Be 'ModuleOnly';
        }
    }

    Context 'Read Include.Path' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Include.Path | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Include.Path' = 'out/' };
            $option.Include.Path.Length | Should -Be 1;
            $option.Include.Path | Should -BeIn 'out/';

            $option = New-PSRuleOption -Option @{ 'Include.Path' = 'out/','.ps-rule/' };
            $option.Include.Path.Length | Should -Be 2;
            $option.Include.Path | Should -BeIn 'out/', '.ps-rule/';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests7.yml');
            $option.Include.Path.Length | Should -Be 2;
            $option.Include.Path | Should -BeIn 'out/', '.ps-rule/';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_INCLUDE_PATH = 'out/;.ps-rule/';
                $option = New-PSRuleOption;
                $option.Include.Path | Should -BeIn 'out/', '.ps-rule/';
            }
            finally {
                Remove-Item 'Env:PSRULE_INCLUDE_PATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -IncludePath 'out/', '.ps-rule/' -Path $emptyOptionsFilePath;
            $option.Include.Path | Should -BeIn 'out/', '.ps-rule/';
        }
    }

    Context 'Read Include.Module' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Include.Module | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Include.Module' = 'TestModule' };
            $option.Include.Module.Length | Should -Be 1;
            $option.Include.Module | Should -BeIn 'TestModule';

            $option = New-PSRuleOption -Option @{ 'Include.Module' = 'TestModule','TestModule2' };
            $option.Include.Module.Length | Should -Be 2;
            $option.Include.Module | Should -BeIn 'TestModule', 'TestModule2';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests7.yml');
            $option.Include.Module.Length | Should -Be 2;
            $option.Include.Module | Should -BeIn 'TestModule', 'TestModule2';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_INCLUDE_MODULE = 'TestModule;TestModule2';
                $option = New-PSRuleOption;
                $option.Include.Module | Should -BeIn 'TestModule', 'TestModule2';
            }
            finally {
                Remove-Item 'Env:PSRULE_INCLUDE_MODULE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -IncludeModule 'TestModule', 'TestModule2' -Path $emptyOptionsFilePath;
            $option.Include.Module | Should -BeIn 'TestModule', 'TestModule2';
        }
    }

    Context 'Read Input.FileObjects' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.FileObjects | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.FileObjects' = $True };
            $option.Input.FileObjects | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests15.yml');
            $option.Input.FileObjects | Should -Be $True;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_INPUT_FILEOBJECTS = 'true';
                $option = New-PSRuleOption;
                $option.Input.FileObjects | Should -Be $True;

                # With int
                $Env:PSRULE_INPUT_FILEOBJECTS = '1';
                $option = New-PSRuleOption;
                $option.Input.FileObjects | Should -Be $True;
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_FILEOBJECTS' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputFileObjects $True -Path $emptyOptionsFilePath;
            $option.Input.FileObjects | Should -Be $True;
        }
    }

    Context 'Read Input.Format' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.Format | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.Format' = 'Yaml' };
            $option.Input.Format | Should -Be 'Yaml';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.Format | Should -Be 'Yaml';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_INPUT_FORMAT = 'Yaml';
                $option = New-PSRuleOption;
                $option.Input.Format | Should -Be 'Yaml';
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_FORMAT' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -Format 'Yaml' -Path $emptyOptionsFilePath;
            $option.Input.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Input.IgnoreGitPath' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.IgnoreGitPath | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.IgnoreGitPath' = $False };
            $option.Input.IgnoreGitPath | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.IgnoreGitPath | Should -Be $False;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_INPUT_IGNOREGITPATH = 'false';
                $option = New-PSRuleOption;
                $option.Input.IgnoreGitPath | Should -Be $False;

                # With int
                $Env:PSRULE_INPUT_IGNOREGITPATH = '0';
                $option = New-PSRuleOption;
                $option.Input.IgnoreGitPath | Should -Be $False;
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_IGNOREGITPATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputIgnoreGitPath $False -Path $emptyOptionsFilePath;
            $option.Input.IgnoreGitPath | Should -Be $False;
        }
    }

    Context 'Read Input.IgnoreObjectSource' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.IgnoreObjectSource | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.IgnoreObjectSource' = $True };
            $option.Input.IgnoreObjectSource | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.IgnoreObjectSource | Should -Be $True;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_INPUT_IGNOREOBJECTSOURCE = 'true';
                $option = New-PSRuleOption;
                $option.Input.IgnoreObjectSource | Should -Be $True;

                # With int
                $Env:PSRULE_INPUT_IGNOREOBJECTSOURCE = '1';
                $option = New-PSRuleOption;
                $option.Input.IgnoreObjectSource | Should -Be $True;
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_IGNOREOBJECTSOURCE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputIgnoreObjectSource $True -Path $emptyOptionsFilePath;
            $option.Input.IgnoreObjectSource | Should -Be $True;
        }
    }

    Context 'Read Input.IgnoreRepositoryCommon' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.IgnoreRepositoryCommon | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.IgnoreRepositoryCommon' = $False };
            $option.Input.IgnoreRepositoryCommon | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.IgnoreRepositoryCommon | Should -Be $False;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_INPUT_IGNOREREPOSITORYCOMMON = 'false';
                $option = New-PSRuleOption;
                $option.Input.IgnoreRepositoryCommon | Should -Be $False;

                # With int
                $Env:PSRULE_INPUT_IGNOREREPOSITORYCOMMON = '0';
                $option = New-PSRuleOption;
                $option.Input.IgnoreRepositoryCommon | Should -Be $False;
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_IGNOREREPOSITORYCOMMON' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputIgnoreRepositoryCommon $False -Path $emptyOptionsFilePath;
            $option.Input.IgnoreRepositoryCommon | Should -Be $False;
        }
    }

    Context 'Read Input.IgnoreUnchangedPath' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.IgnoreUnchangedPath | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.IgnoreUnchangedPath' = $True };
            $option.Input.IgnoreUnchangedPath | Should -Be $True;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.IgnoreUnchangedPath | Should -Be $True;
        }

        It 'from Environment' {
            try {
                # With bool
                $Env:PSRULE_INPUT_IGNOREUNCHANGEDPATH = 'true';
                $option = New-PSRuleOption;
                $option.Input.IgnoreUnchangedPath | Should -Be $True;

                # With int
                $Env:PSRULE_INPUT_IGNOREUNCHANGEDPATH = '1';
                $option = New-PSRuleOption;
                $option.Input.IgnoreUnchangedPath | Should -Be $True;
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_IGNOREUNCHANGEDPATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputIgnoreUnchangedPath $True -Path $emptyOptionsFilePath;
            $option.Input.IgnoreUnchangedPath | Should -Be $True;
        }
    }

    Context 'Read Input.ObjectPath' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.ObjectPath | Should -Be $Null;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.ObjectPath' = 'items' };
            $option.Input.ObjectPath | Should -Be 'items';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.ObjectPath | Should -Be 'items';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_INPUT_OBJECTPATH = 'items';
                $option = New-PSRuleOption;
                $option.Input.ObjectPath | Should -Be 'items';
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_OBJECTPATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -ObjectPath 'items' -Path $emptyOptionsFilePath;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Input.PathIgnore' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.PathIgnore | Should -Be $Null;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Input.PathIgnore' = 'ignore.cs' };
            $option.Input.PathIgnore | Should -Be 'ignore.cs';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.PathIgnore | Should -Be '*.Designer.cs';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_INPUT_PATHIGNORE = 'ignore.cs';
                $option = New-PSRuleOption;
                $option.Input.PathIgnore | Should -Be 'ignore.cs';

                # With array
                $Env:PSRULE_INPUT_PATHIGNORE = 'ignore.cs;*.Designer.cs';
                $option = New-PSRuleOption;
                $option.Input.PathIgnore | Should -Be 'ignore.cs', '*.Designer.cs';
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_PATHIGNORE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputPathIgnore 'ignore.cs' -Path $emptyOptionsFilePath;
            $option.Input.PathIgnore | Should -Be 'ignore.cs';
        }
    }

    Context 'Read Input.TargetType' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Input.TargetType | Should -Be $Null;
        }

        It 'from Hashtable' {
            # With single item
            $option = New-PSRuleOption -Option @{ 'Input.TargetType' = 'virtualMachine' };
            $option.Input.TargetType | Should -BeIn 'virtualMachine';

            # With array
            $option = New-PSRuleOption -Option @{ 'Input.TargetType' = 'virtualMachine', 'virtualNetwork' };
            $option.Input.TargetType.Length | Should -Be 2;
            $option.Input.TargetType | Should -BeIn 'virtualMachine', 'virtualNetwork';
        }

        It 'from YAML' {
            # With single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Input.TargetType | Should -BeIn 'virtualMachine';

            # With array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Input.TargetType | Should -BeIn 'virtualMachine', 'virtualNetwork';

            # With flat single item
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests3.yml');
            $option.Input.TargetType | Should -BeIn 'virtualMachine';
        }

        It 'from Environment' {
            try {
                # With single item
                $Env:PSRULE_INPUT_TARGETTYPE = 'virtualMachine';
                $option = New-PSRuleOption;
                $option.Input.TargetType | Should -Be 'virtualMachine';

                # With array
                $Env:PSRULE_INPUT_TARGETTYPE = 'virtualMachine;virtualNetwork';
                $option = New-PSRuleOption;
                $option.Input.TargetType | Should -Be 'virtualMachine', 'virtualNetwork';
            }
            finally {
                Remove-Item 'Env:PSRULE_INPUT_TARGETTYPE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -InputTargetType 'virtualMachine', 'virtualNetwork' -Path $emptyOptionsFilePath;
            $option.Input.TargetType | Should -BeIn 'virtualMachine', 'virtualNetwork';
        }
    }

    Context 'Read Logging.LimitDebug' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Logging.LimitDebug | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.LimitDebug' = 'TestRule1' };
            $option.Logging.LimitDebug | Should -Be 'TestRule1';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.LimitDebug | Should -Be 'TestRule2';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingLimitDebug 'TestRule2' -Path $emptyOptionsFilePath;
            $option.Logging.LimitDebug | Should -Be 'TestRule2';
        }
    }

    Context 'Read Logging.LimitVerbose' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Logging.LimitVerbose | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.LimitVerbose' = 'TestRule1' };
            $option.Logging.LimitVerbose | Should -Be 'TestRule1';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.LimitVerbose | Should -Be 'TestRule2';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingLimitVerbose 'TestRule2' -Path $emptyOptionsFilePath;
            $option.Logging.LimitVerbose | Should -Be 'TestRule2';
        }
    }

    Context 'Read Logging.RuleFail' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Logging.RuleFail | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
            $option.Logging.RuleFail | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.RuleFail | Should -Be 'Warning';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingRuleFail 'Warning' -Path $emptyOptionsFilePath;
            $option.Logging.RuleFail | Should -Be 'Warning';
        }
    }

    Context 'Read Logging.RulePass' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Logging.RulePass | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Error' };
            $option.Logging.RulePass | Should -Be 'Error';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Logging.RulePass | Should -Be 'Warning';
        }

        It 'from parameter' {
            $option = New-PSRuleOption -LoggingRulePass 'Warning' -Path $emptyOptionsFilePath;
            $option.Logging.RulePass | Should -Be 'Warning';
        }
    }

    Context 'Read Output.As' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.As | Should -Be 'Detail';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.As' = 'Summary' };
            $option.Output.As | Should -Be 'Summary';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.As | Should -Be 'Summary';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_AS = 'Summary';
                $option = New-PSRuleOption;
                $option.Output.As | Should -Be 'Summary';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_AS' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputAs 'Summary' -Path $emptyOptionsFilePath;
            $option.Output.As | Should -Be 'Summary';
        }
    }

    Context 'Read Output.Banner' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Banner | Should -Be 'Default';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Banner' = 'Minimal' };
            $option.Output.Banner | Should -Be 'Minimal';

            $option = New-PSRuleOption -Option @{ 'Output.Banner' = 1 };
            $option.Output.Banner | Should -Be 'Title';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Banner | Should -Be 'Minimal';

            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Output.Banner | Should -Be 'Title';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_BANNER = 'Minimal';
                $option = New-PSRuleOption;
                $option.Output.Banner | Should -Be 'Minimal';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_BANNER' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputBanner 'Minimal' -Path $emptyOptionsFilePath;
            $option.Output.Banner | Should -Be 'Minimal';
        }
    }

    Context 'Read Output.Culture' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Culture | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            # Single
            $option = New-PSRuleOption -Option @{ 'Output.Culture' = 'en-AA' };
            $option.Output.Culture.Length | Should -Be 1;
            $option.Output.Culture | Should -BeIn 'en-AA';

            # Array
            $option = New-PSRuleOption -Option @{ 'Output.Culture' = 'en-AA', 'en-BB' };
            $option.Output.Culture.Length | Should -Be 2;
            $option.Output.Culture | Should -BeIn 'en-AA', 'en-BB';
        }

        It 'from YAML' {
            # Single
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Culture.Length | Should -Be 1;
            $option.Output.Culture | Should -BeIn 'en-CC';

            # Array
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests2.yml');
            $option.Output.Culture.Length | Should -Be 2;
            $option.Output.Culture | Should -BeIn 'en-CC', 'en-DD';
        }

        It 'from Environment' {
            try {
                # Single
                $Env:PSRULE_OUTPUT_CULTURE = 'en-AA';
                $option = New-PSRuleOption;
                $option.Output.Culture | Should -BeIn 'en-AA';

                # Array
                $Env:PSRULE_OUTPUT_CULTURE = 'en-AA;en-BB';
                $option = New-PSRuleOption;
                $option.Output.Culture | Should -BeIn 'en-AA', 'en-BB';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_CULTURE' -Force;
            }
        }

        It 'from parameter' {
            # Single
            $option = New-PSRuleOption -OutputCulture 'en-XX' -Path $emptyOptionsFilePath;
            $option.Output.Culture.Length | Should -Be 1;
            $option.Output.Culture | Should -BeIn 'en-XX';

            # Array
            $option = New-PSRuleOption -OutputCulture 'en-XX', 'en-YY' -Path $emptyOptionsFilePath;
            $option.Output.Culture.Length | Should -Be 2;
            $option.Output.Culture | Should -BeIn 'en-XX', 'en-YY';
        }
    }

    Context 'Read Output.Encoding' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Encoding | Should -Be 'Default';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Encoding' = 'UTF7' };
            $option.Output.Encoding | Should -Be 'UTF7';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Encoding | Should -Be 'UTF7';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_ENCODING = 'UTF7';
                $option = New-PSRuleOption;
                $option.Output.Encoding | Should -Be 'UTF7';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_ENCODING' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputEncoding 'UTF7' -Path $emptyOptionsFilePath;
            $option.Output.Encoding | Should -Be 'UTF7';
        }
    }

    Context 'Read Output.Footer' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Footer | Should -Be 'Default';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Footer' = 'RuleCount' };
            $option.Output.Footer | Should -Be 'RuleCount';

            $option = New-PSRuleOption -Option @{ 'Output.Footer' = 1 };
            $option.Output.Footer | Should -Be 'RuleCount';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Footer | Should -Be 'RuleCount';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_FOOTER = 'RuleCount';
                $option = New-PSRuleOption;
                $option.Output.Footer | Should -Be 'RuleCount';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_FOOTER' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputFooter 'RuleCount' -Path $emptyOptionsFilePath;
            $option.Output.Footer | Should -Be 'RuleCount';
        }
    }

    Context 'Read Output.Format' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Format | Should -Be 'None';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Format' = 'Yaml' };
            $option.Output.Format | Should -Be 'Yaml';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Format | Should -Be 'Json';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_FORMAT = 'Yaml';
                $option = New-PSRuleOption;
                $option.Output.Format | Should -Be 'Yaml';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_FORMAT' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputFormat 'Yaml' -Path $emptyOptionsFilePath;
            $option.Output.Format | Should -Be 'Yaml';
        }
    }

    Context 'Output.JobSummaryPath' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.JobSummaryPath | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.JobSummaryPath' = './summary.md' };
            $option.Output.JobSummaryPath | Should -Be './summary.md';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.JobSummaryPath | Should -Be 'summary.md';
        }

        It 'from Environment' {
            try {
                $env:PSRULE_OUTPUT_JOBSUMMARYPATH = './summary.md';
                $option = New-PSRuleOption;
                $option.Output.JobSummaryPath | Should -Be './summary.md';
            }
            finally {
                Remove-Item 'env:PSRULE_OUTPUT_JOBSUMMARYPATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputJobSummaryPath './summary.md' -Path $emptyOptionsFilePath;
            $option.Output.JobSummaryPath | Should -Be './summary.md';
        }
    }

    Context 'Output.JsonIndent' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.JsonIndent | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.JsonIndent' = 2 };
            $option.Output.JsonIndent | Should -Be 2;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests13.yml');
            $option.Output.JsonIndent | Should -Be 4;
        }

        It 'from Environment' {
            try {
                $env:PSRULE_OUTPUT_JSONINDENT = 2;
                $option = New-PSRuleOption;
                $option.Output.JsonIndent | Should -Be 2;
            }
            finally {
                Remove-Item 'env:PSRULE_OUTPUT_JSONINDENT' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputJsonIndent 4 -Path $emptyOptionsFilePath;
            $option.Output.JsonIndent | Should -Be 4;
        }

        It 'from invalid range using -OutputJsonIndent' {
            { New-PSRuleOption -OutputJsonIndent -1 } | Should -Throw;
            { New-PSRuleOption -OutputJsonIndent 5 } | Should -Throw;

            { Set-PSRuleOption -OutputJsonIndent -1 } | Should -Throw;
            { Set-PSRuleOption -OutputJsonIndent 5 } | Should -Throw;
        }
    }

    Context 'Read Output.Outcome' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Outcome | Should -Be 'Processed';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Outcome' = 'Fail' };
            $option.Output.Outcome | Should -Be 'Fail';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Outcome | Should -Be 'Pass';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_OUTCOME = 'Fail';
                $option = New-PSRuleOption;
                $option.Output.Outcome | Should -Be 'Fail';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_OUTCOME' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputOutcome 'Fail' -Path $emptyOptionsFilePath;
            $option.Output.Outcome | Should -Be 'Fail';
        }
    }

    Context 'Read Output.Path' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Path | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Path' = 'out/OutputPath.txt' };
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_PATH = 'out/OutputPath.txt';
                $option = New-PSRuleOption;
                $option.Output.Path | Should -Be 'out/OutputPath.txt';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_PATH' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputPath 'out/OutputPath.txt' -Path $emptyOptionsFilePath;
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }
    }

    Context 'Output.SarifProblemsOnly' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.SarifProblemsOnly | Should -Be $True;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.SarifProblemsOnly' = $False };
            $option.Output.SarifProblemsOnly | Should -Be $False;
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.SarifProblemsOnly | Should -Be $False;
        }

        It 'from Environment' {
            try {
                $env:PSRULE_OUTPUT_SARIFPROBLEMSONLY = 'false';
                $option = New-PSRuleOption;
                $option.Output.SarifProblemsOnly | Should -Be $False;
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_SARIFPROBLEMSONLY' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputSarifProblemsOnly $False -Path $emptyOptionsFilePath;
            $option.Output.SarifProblemsOnly | Should -Be $False;
        }
    }

    Context 'Read Output.Style' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Output.Style | Should -Be 'Detect';
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Output.Style' = 'AzurePipelines' };
            $option.Output.Style | Should -Be 'AzurePipelines';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Output.Style | Should -Be 'GitHubActions';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OUTPUT_STYLE = 'AzurePipelines';
                $option = New-PSRuleOption;
                $option.Output.Style | Should -Be 'AzurePipelines';
            }
            finally {
                Remove-Item 'Env:PSRULE_OUTPUT_STYLE' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OutputStyle 'AzurePipelines' -Path $emptyOptionsFilePath;
            $option.Output.Style | Should -Be 'AzurePipelines';
        }
    }

    Context 'Read Override.Level' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Override.Level | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Override.Level.rule1' = 'Information' };
            $option.Override.Level['rule1'] | Should -Be 'Information';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Override.Level['rule1'] | Should -Be 'Information';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_OVERRIDE_LEVEL_RULE1 = 'Information';
                $option = New-PSRuleOption;
                $option.Override.Level['rule1'] | Should -Be 'Information';
            }
            finally {
                Remove-Item 'Env:PSRULE_OVERRIDE_LEVEL_RULE1' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -OverrideLevel @{ rule1 = 'Information' } -Path $emptyOptionsFilePath;
            $option.Override.Level['rule1'] | Should -Be 'Information';
        }
    }

    Context 'Read Repository.BaseRef' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Repository.BaseRef | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Repository.BaseRef' = 'dev' };
            $option.Repository.BaseRef | Should -Be 'dev';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Repository.BaseRef | Should -Be 'dev';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_REPOSITORY_BASEREF = 'dev';
                $option = New-PSRuleOption;
                $option.Repository.BaseRef | Should -Be 'dev';
            }
            finally {
                Remove-Item 'Env:PSRULE_REPOSITORY_BASEREF' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -RepositoryBaseRef 'dev' -Path $emptyOptionsFilePath;
            $option.Repository.BaseRef | Should -Be 'dev';
        }
    }

    Context 'Read Repository.Url' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Repository.Url | Should -BeNullOrEmpty;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Repository.Url' = 'https://github.com/microsoft/PSRule.UnitTest' };
            $option.Repository.Url | Should -Be 'https://github.com/microsoft/PSRule.UnitTest';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Repository.Url | Should -Be 'https://github.com/microsoft/PSRule.UnitTest';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_REPOSITORY_URL = 'https://github.com/microsoft/PSRule.UnitTest';
                $option = New-PSRuleOption;
                $option.Repository.Url | Should -Be 'https://github.com/microsoft/PSRule.UnitTest';
            }
            finally {
                Remove-Item 'Env:PSRULE_REPOSITORY_URL' -Force;
            }
        }

        It 'from parameter' {
            $option = New-PSRuleOption -RepositoryUrl 'https://github.com/microsoft/PSRule.UnitTest' -Path $emptyOptionsFilePath;
            $option.Repository.Url | Should -Be 'https://github.com/microsoft/PSRule.UnitTest';
        }
    }

    Context 'Read Requires' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Requires.ContainsKey('PSRule') | Should -Be $False;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -Option @{ 'Requires.PSRule' = '^0.1.0' };
            $option.Requires.PSRule | Should -Be '^0.1.0';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests6.yml');
            $option.Requires.PSRule | Should -Be '>=0.18.0';
        }

        It 'from Environment' {
            try {
                $Env:PSRULE_REQUIRES_PSRULE = '^0.1.0';
                $Env:PSRULE_REQUIRES_PSRULE_RULES_AZURE = '^0.2.0';
                $option = New-PSRuleOption;
                $option.Requires.PSRule | Should -Be '^0.1.0';
                $option.Requires.'PSRule.Rules.Azure' | Should -Be '^0.2.0';
            }
            finally {
                Remove-Item 'Env:PSRULE_REQUIRES_PSRULE' -Force;
                Remove-Item 'Env:PSRULE_REQUIRES_PSRULE_RULES_AZURE' -Force;
            }
        }
    }

    Context 'Read Suppression' {
        It 'from default' {
            $option = New-PSRuleOption -Default;
            $option.Suppression.Count | Should -Be 0;
        }

        It 'from Hashtable' {
            $option = New-PSRuleOption -SuppressTargetName @{ 'SuppressionTest' = 'testObject1', 'testObject3' };
            $option.Suppression['SuppressionTest'].TargetName | Should -BeIn 'testObject1', 'testObject3';
        }

        It 'from YAML' {
            $option = New-PSRuleOption -Option (Join-Path -Path $here -ChildPath 'PSRule.Tests.yml');
            $option.Suppression['SuppressionTest1'].TargetName | Should -BeIn 'TestObject1', 'TestObject3';
            $option.Suppression['SuppressionTest2'].TargetName | Should -BeIn 'TestObject1', 'TestObject3';
        }
    }
}

#endregion New-PSRuleOption

#region Set-PSRuleOption

Describe 'Set-PSRuleOption' -Tag 'Option','Set-PSRuleOption' {
    BeforeAll {
        $emptyOptionsFilePath = (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml');
        $optionParams = @{
            Path = $emptyOptionsFilePath
            PassThru = $True
        }
    }

    Context 'Use -AllowClobber' {
        BeforeAll {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'PSRule.Tests4.yml');
            Copy-Item -Path (Join-Path -Path $here -ChildPath 'PSRule.Tests4.yml') -Destination $filePath;
        }

        It 'Errors with comments' {
            { Set-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw -ErrorId 'PSRule.PSRuleOption.YamlContainsComments,Set-PSRuleOption';
        }

        it 'Overwrites file' {
            Set-PSRuleOption -Path $filePath -BindingIgnoreCase $True -AllowClobber;
            $result = New-PSRuleOption -Path $filePath;
            $result.Binding.IgnoreCase | Should -Be $True;
        }
    }

    Context 'Use -Path' {
        It 'With missing file' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'set-not-a-file.yml');
            { Set-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Not -Throw;
            Test-Path -Path $filePath | Should -Be $True;
        }

        It 'With missing directory' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'set-dir/ps-rule.yaml');
            { Set-PSRuleOption -Path $filePath -ErrorAction Stop } | Should -Throw -ErrorId 'PSRule.PSRuleOption.ParentPathNotFound,Set-PSRuleOption';
            Test-Path -Path $filePath | Should -Be $False;
        }

        It 'With missing directory with -Force' {
            $filePath = (Join-Path -Path $outputPath -ChildPath 'set-dir/ps-rule.yaml');
            Set-PSRuleOption -Path $filePath -Force;
            Test-Path -Path $filePath | Should -Be $True;
        }
    }

    Context 'Default YAML file detection' {
        It 'Find default options files' {
            $defaultPath = (Join-Path -Path $outputPath -ChildPath 'default');

            # psrule.yml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'psrule.yml');
            Set-PSRuleOption -Path $filePath -TargetName 'psrule.yml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'psrule.yml';

            # psrule.yaml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'psrule.yaml');
            Set-PSRuleOption -Path $filePath -TargetName 'psrule.yaml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'psrule.yaml';

            # ps-rule.yml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'ps-rule.yml');
            Set-PSRuleOption -Path $filePath -TargetName 'ps-rule.yml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'ps-rule.yml';

            # ps-rule.yaml
            $filePath = (Join-Path -Path $defaultPath -ChildPath 'ps-rule.yaml');
            Set-PSRuleOption -Path $filePath -TargetName 'ps-rule.yaml' -Force;
            $result = New-PSRuleOption -Path $defaultPath;
            $result.Binding.TargetName | Should -Be 'ps-rule.yaml';

            (Get-ChildItem -Path $defaultPath | Measure-Object).Count | Should -Be 4;
        }
    }

    Context 'Read Binding.Field' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingField @{ id = 'resourceId' } @optionParams;
            $option.Binding.Field | Should -Not -BeNullOrEmpty;
            $option.Binding.Field.id[0] | Should -Be 'resourceId';
        }
    }

    Context 'Read Binding.IgnoreCase' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingIgnoreCase $False @optionParams;
            $option.Binding.IgnoreCase | Should -Be $False;
        }
    }

    Context 'Read Binding.NameSeparator' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingNameSeparator '::' @optionParams;
            $option.Binding.NameSeparator | Should -Be $True;
        }
    }

    Context 'Read Binding.PreferTargetInfo' {
        It 'from parameter' {
            $option = Set-PSRuleOption -BindingPreferTargetInfo $True @optionParams;
            $option.Binding.PreferTargetInfo | Should -Be $True;
        }
    }

    Context 'Read Binding.TargetName' {
        It 'from parameter' {
            $option = Set-PSRuleOption -TargetName 'ResourceName', 'AlternateName' @optionParams;
            $option.Binding.TargetName | Should -BeIn 'ResourceName', 'AlternateName';
        }
    }

    Context 'Read Binding.TargetType' {
        It 'from parameter' {
            $option = Set-PSRuleOption -TargetType 'ResourceType', 'Kind' @optionParams;
            $option.Binding.TargetType | Should -BeIn 'ResourceType', 'Kind';
        }
    }

    Context 'Read Execution.Break' {
        It 'from parameter' {
            $option = Set-PSRuleOption -ExecutionBreak 'Never' @optionParams;
            $option.Execution.Break | Should -Be 'Never';
        }
    }

    Context 'Read Execution.SuppressionGroupExpired' {
        It 'from parameter' {
            $option = Set-PSRuleOption -SuppressionGroupExpired 'Error' @optionParams;
            $option.Execution.SuppressionGroupExpired | Should -Be 'Error';
        }
    }

    Context 'Read Input.FileObjects' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputFileObjects $True @optionParams;
            $option.Input.FileObjects | Should -Be $True;
        }
    }

    Context 'Read Input.Format' {
        It 'from parameter' {
            $option = Set-PSRuleOption -Format 'Yaml' @optionParams;
            $option.Input.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Input.IgnoreGitPath' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputIgnoreGitPath $False @optionParams;
            $option.Input.IgnoreGitPath | Should -Be $False;
        }
    }

    Context 'Read Input.IgnoreRepositoryCommon' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputIgnoreRepositoryCommon $False @optionParams;
            $option.Input.IgnoreRepositoryCommon | Should -Be $False;
        }
    }

    Context 'Read Input.IgnoreUnchangedPath' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputIgnoreUnchangedPath $True @optionParams;
            $option.Input.IgnoreUnchangedPath | Should -Be $True;
        }
    }

    Context 'Read Input.ObjectPath' {
        It 'from parameter' {
            $option = Set-PSRuleOption -ObjectPath 'items' @optionParams;
            $option.Input.ObjectPath | Should -Be 'items';
        }
    }

    Context 'Read Input.PathIgnore' {
        It 'from parameter' {
            $option = Set-PSRuleOption -InputPathIgnore 'ignore.cs' @optionParams;
            $option.Input.PathIgnore | Should -Be 'ignore.cs';
        }
    }

    Context 'Read Logging.LimitDebug' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingLimitDebug 'TestRule2' @optionParams;
            $option.Logging.LimitDebug | Should -Be 'TestRule2';
        }
    }

    Context 'Read Logging.LimitVerbose' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingLimitVerbose 'TestRule2' @optionParams;
            $option.Logging.LimitVerbose | Should -Be 'TestRule2';
        }
    }

    Context 'Read Logging.RuleFail' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingRuleFail 'Warning' @optionParams;
            $option.Logging.RuleFail | Should -Be 'Warning';
        }
    }

    Context 'Read Logging.RulePass' {
        It 'from parameter' {
            $option = Set-PSRuleOption -LoggingRulePass 'Warning' @optionParams;
            $option.Logging.RulePass | Should -Be 'Warning';
        }
    }

    Context 'Read Output.As' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputAs 'Summary' @optionParams;
            $option.Output.As | Should -Be 'Summary';
        }
    }

    Context 'Read Output.Banner' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputBanner 'Minimal' @optionParams;
            $option.Output.Banner | Should -Be 'Minimal';
        }
    }

    Context 'Read Output.Culture' {
        It 'from parameter' {
            # Single
            $option = Set-PSRuleOption -OutputCulture 'en-EE' @optionParams;
            $option.Output.Culture.Length | Should -Be 1;
            $option.Output.Culture | Should -BeIn 'en-EE';

            # Array
            $option = Set-PSRuleOption -OutputCulture 'en-EE', 'en-FF' @optionParams;
            $option.Output.Culture.Length | Should -Be 2;
            $option.Output.Culture | Should -BeIn 'en-EE', 'en-FF';
        }
    }

    Context 'Read Output.Encoding' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputEncoding 'UTF7' @optionParams;
            $option.Output.Encoding | Should -Be 'UTF7';
        }
    }

    Context 'Read Output.Footer' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputFooter 'RuleCount' @optionParams;
            $option.Output.Footer | Should -Be 'RuleCount';
        }
    }

    Context 'Read Output.Format' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputFormat 'Yaml' @optionParams;
            $option.Output.Format | Should -Be 'Yaml';
        }
    }

    Context 'Read Output.JobSummaryPath' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputJobSummaryPath './summary.md' @optionParams;
            $option.Output.JobSummaryPath | Should -Be './summary.md';
        }
    }

    Context 'Read Output.JsonIndent' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputJsonIndent 4 @optionParams;
            $option.Output.JsonIndent | Should -Be 4;
        }
    }

    Context 'Read Output.Outcome' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputOutcome Fail @optionParams;
            $option.Output.Outcome | Should -Be 'Fail';
        }
    }

    Context 'Read Output.Path' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputPath 'out/OutputPath.txt' @optionParams;
            $option.Output.Path | Should -Be 'out/OutputPath.txt';
        }
    }

    Context 'Read Output.SarifProblemsOnly' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputSarifProblemsOnly $False @optionParams;
            $option.Output.SarifProblemsOnly | Should -Be $False;
        }
    }

    Context 'Read Output.Style' {
        It 'from parameter' {
            $option = Set-PSRuleOption -OutputStyle 'AzurePipelines' @optionParams;
            $option.Output.Style | Should -Be 'AzurePipelines';
        }
    }

    Context 'Read Repository.BaseRef' {
        It 'from parameter' {
            $option = Set-PSRuleOption -RepositoryBaseRef 'dev' @optionParams;
            $option.Repository.BaseRef | Should -Be 'dev';
        }
    }

    Context 'Read Repository.Url' {
        It 'from parameter' {
            $option = Set-PSRuleOption -RepositoryUrl 'https://github.com/microsoft/PSRule.UnitTest' @optionParams;
            $option.Repository.Url | Should -Be 'https://github.com/microsoft/PSRule.UnitTest';
        }
    }

    Context 'Read Rule.IncludeLocal' {
        It 'from parameter' {
            $option = Set-PSRuleOption -RuleIncludeLocal $True @optionParams;
            $option.Rule.IncludeLocal | Should -Be $True;
        }
    }
}

#endregion Set-PSRuleOption
