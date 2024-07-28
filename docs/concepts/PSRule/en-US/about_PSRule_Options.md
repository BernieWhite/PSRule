# PSRule_Options

## about_PSRule_Options

## SHORT DESCRIPTION

Describes additional options that can be used during rule execution.

## LONG DESCRIPTION

PSRule lets you use options when calling cmdlets such as `Invoke-PSRule` and `Test-PSRuleTarget` to change how rules are processed.
This topic describes what options are available, when to and how to use them.

The following workspace options are available for use:

- [Baseline.Group](#baselinegroup)
- [Convention.Include](#conventioninclude)
- [Execution.AliasReference](#executionaliasreference)
- [Execution.Break](#executionbreak)
- [Execution.DuplicateResourceId](#executionduplicateresourceid)
- [Execution.HashAlgorithm](#executionhashalgorithm)
- [Execution.LanguageMode](#executionlanguagemode)
- [Execution.InvariantCulture](#executioninvariantculture)
- [Execution.InitialSessionState](#executioninitialsessionstate)
- [Execution.RestrictScriptSource](#executionrestrictscriptsource)
- [Execution.RuleInconclusive](#executionruleinconclusive)
- [Execution.SuppressionGroupExpired](#executionsuppressiongroupexpired)
- [Execution.UnprocessedObject](#executionunprocessedobject)
- [Include.Module](#includemodule)
- [Include.Path](#includepath)
- [Input.FileObjects](#inputfileobjects)
- [Input.Format](#inputformat)
- [Input.IgnoreGitPath](#inputignoregitpath)
- [Input.IgnoreObjectSource](#inputignoreobjectsource)
- [Input.IgnoreRepositoryCommon](#inputignorerepositorycommon)
- [Input.IgnoreUnchangedPath](#inputignoreunchangedpath)
- [Input.ObjectPath](#inputobjectpath)
- [Input.PathIgnore](#inputpathignore)
- [Input.TargetType](#inputtargettype)
- [Logging.LimitDebug](#logginglimitdebug)
- [Logging.LimitVerbose](#logginglimitverbose)
- [Logging.RuleFail](#loggingrulefail)
- [Logging.RulePass](#loggingrulepass)
- [Output.As](#outputas)
- [Output.Banner](#outputbanner)
- [Output.Culture](#outputculture)
- [Output.Encoding](#outputencoding)
- [Output.Footer](#outputfooter)
- [Output.Format](#outputformat)
- [Output.JobSummaryPath](#outputjobsummarypath)
- [Output.JsonIndent](#outputjsonindent)
- [Output.Outcome](#outputoutcome)
- [Output.Path](#outputpath)
- [Output.SarifProblemsOnly](#outputsarifproblemsonly)
- [Output.Style](#outputstyle)
- [Repository.BaseRef](#repositorybaseref)
- [Repository.Url](#repositoryurl)
- [Requires](#requires)
- [Suppression](#suppression)

Additionally the following baseline options can be included:

- [Binding.Field](#bindingfield)
- [Binding.IgnoreCase](#bindingignorecase)
- [Binding.NameSeparator](#bindingnameseparator)
- [Binding.PreferTargetInfo](#bindingprefertargetinfo)
- [Binding.TargetName](#bindingtargetname)
- [Binding.TargetType](#bindingtargettype)
- [Binding.UseQualifiedName](#bindingusequalifiedname)
- [Configuration](#configuration)
- [Override.Level](#overridelevel)
- [Rule.Baseline](#rulebaseline)
- [Rule.Include](#ruleinclude)
- [Rule.IncludeLocal](#ruleincludelocal)
- [Rule.Exclude](#ruleexclude)
- [Rule.Tag](#ruletag)

See [about_PSRule_Baseline](about_PSRule_Baseline.md) for more information on baseline options.

Options can be used with the following PSRule cmdlets:

- Export-PSRuleBaseline
- Get-PSRule
- Get-PSRuleBaseline
- Get-PSRuleHelp
- Invoke-PSRule
- Test-PSRuleTarget

Each of these cmdlets support:

- Using the `-Option` parameter with an object created with the `New-PSRuleOption` cmdlet.
  See cmdlet help for syntax and examples.
- Using the `-Option` parameter with a hashtable object.
- Using the `-Option` parameter with a YAML file path.

When using a hashtable object `@{}`, one or more options can be specified as keys using a dotted notation.

For example:

```powershell
$option = @{ 'Output.Format' = 'Yaml' };
Invoke-PSRule -Path . -Option $option;
```

```powershell
Invoke-PSRule -Path . -Option @{ 'Output.Format' = 'Yaml' };
```

The above example shows how the `Output.Format` option as a hashtable key can be used.
Continue reading for a full list of options and how each can be used.

Alternatively, options can be stored in a YAML formatted file and loaded from disk.
Storing options as YAML allows different configurations to be loaded in a repeatable way instead of having to create an options object each time.

Options are stored as YAML properties using a lower camel case naming convention, for example:

```yaml
output:
  format: Yaml
```

The `Set-PSRuleOption` cmdlet can be used to set options stored in YAML or the YAML file can be manually edited.

```powershell
Set-PSRuleOption -OutputFormat Yaml;
```

By default, PSRule will automatically look for a default YAML options file in the current working directory.
Alternatively, you can specify a specific file path.

For example:

```powershell
Invoke-PSRule -Option '.\myconfig.yml';
```

```powershell
New-PSRuleOption -Path '.\myconfig.yaml';
```

PSRule uses any of the following file names (in order) as the default YAML options file.
If more than one of these files exist, the following order will be used to find the first match.

- `ps-rule.yaml`
- `ps-rule.yml`
- `psrule.yaml`
- `psrule.yml`

We recommend only using lowercase characters as shown above.
This is because not all operating systems treat case in the same way.

Most options can be set using environment variables.
When configuring environment variables we recommend that all capital letters are used.
This is because environment variables are case-sensitive on some operating systems.

PSRule environment variables use a consistent naming pattern of `PSRULE_<PARENT>_<NAME>`.
Where `<PARENT>` is the parent class and `<NAME>` is the specific option.
For example:

- `Execution.InconclusiveWarning` is configured by `PSRULE_EXECUTION_INCONCLUSIVEWARNING`.
- `Input.TargetType` is configured by `PSRULE_INPUT_TARGETTYPE`.
- `Output.Format` is configured by `PSRULE_OUTPUT_FORMAT`.

When setting environment variables:

- Enum values are set by string.
For example `PSRULE_OUTPUT_FORMAT` could be set to `Yaml`.
Enum values are case-insensitive.
- Boolean values are set by `true`, `false`, `1`, or `0`.
For example `PSRULE_EXECUTION_INCONCLUSIVEWARNING` could be set to `false`.
Boolean values are case-insensitive.
- String array values can specify multiple items by using a semi-colon separator.
For example `PSRULE_INPUT_TARGETTYPE` could be set to `virtualMachine;virtualNetwork`.

### Baseline.Group

You can use a baseline group to provide a friendly name to an existing baseline.
When you run PSRule you can opt to use the baseline group name as an alternative name for the baseline.
To indicate a baseline group, prefix the group name with `@` where you would use the name of a baseline.

Baseline groups can be specified using:

```powershell
# PowerShell: Using the BaselineGroup parameter
$option = New-PSRuleOption -BaselineGroup @{ latest = 'YourBaseline' };
```

```powershell
# PowerShell: Using the Baseline.Group hashtable key
$option = New-PSRuleOption -Option @{ 'Baseline.Group' = @{ latest = 'YourBaseline' } };
```

```powershell
# PowerShell: Using the BaselineGroup parameter to set YAML
Set-PSRuleOption -BaselineGroup @{ latest = 'YourBaseline' };
```

```yaml
# YAML: Using the baseline/group property
baseline:
  group:
    latest: YourBaseline
```

```bash
# Bash: Using environment variable
export PSRULE_BASELINE_GROUP='latest=YourBaseline'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BASELINE_GROUP: 'latest=YourBaseline'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BASELINE_GROUP
  value: 'latest=YourBaseline'
```

### Binding.Field

When an object is passed from the pipeline, PSRule automatically extracts fields from object properties.
PSRule provides standard fields such as `TargetName` and `TargetType`.
In addition to standard fields, custom fields can be bound.
Custom fields are available to rules and included in output.

PSRule uses the following logic to determine which property should be used for binding:

- By default PSRule will not extract any custom fields.
- If custom fields are configured, PSRule will attempt to bind the field.
  - If **none** of the configured property names exist, the field will be skipped.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity.
    To use a case sensitive match, configure the [Binding.IgnoreCase](#bindingignorecase) option.

Custom field bindings can be specified using:

```powershell
# PowerShell: Using the BindingField parameter
$option = New-PSRuleOption -BindingField @{ id = 'ResourceId', 'AlternativeId' };
```

```powershell
# PowerShell: Using the Binding.Field hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.Field' = @{ id = 'ResourceId', 'AlternativeId' } };
```

```powershell
# PowerShell: Using the BindingField parameter to set YAML
Set-PSRuleOption -BindingField @{ id = 'ResourceId', 'AlternativeId' };
```

```yaml
# YAML: Using the binding/field property
binding:
  field:
    id:
    - ResourceId
    - AlternativeId
```

### Binding.IgnoreCase

When evaluating an object, PSRule extracts a few key properties from the object to help filter rules and display output results.
The process of extract these key properties is called _binding_.
The properties that PSRule uses for binding can be customized by providing a order list of alternative properties to use.
See [`Binding.TargetName`](#bindingtargetname) and [`Binding.TargetType`](#bindingtargettype) for these options.

- By default, custom property binding finds the first matching property by name regardless of case. i.e. `Binding.IgnoreCase` is `true`.
- To make custom bindings case sensitive, set the `Binding.IgnoreCase` option to `false`.
  - Changing this option will affect custom property bindings for both _TargetName_ and _TargetType_.
  - Setting this option has no affect on binding defaults or custom scripts.

This option can be specified using:

```powershell
# PowerShell: Using the BindingIgnoreCase parameter
$option = New-PSRuleOption -BindingIgnoreCase $False;
```

```powershell
# PowerShell: Using the Binding.IgnoreCase hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.IgnoreCase' = $False };
```

```powershell
# PowerShell: Using the BindingIgnoreCase parameter to set YAML
Set-PSRuleOption -BindingIgnoreCase $False;
```

```yaml
# YAML: Using the binding/ignoreCase property
binding:
  ignoreCase: false
```

```bash
# Bash: Using environment variable
export PSRULE_BINDING_IGNORECASE=false
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BINDING_IGNORECASE: false
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BINDING_IGNORECASE
  value: false
```

### Binding.NameSeparator

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_.
_TargetName_ is used in output results to identify one object from another.

In cases where different types of objects share the same _TargetName_, this may become confusing.
Using a qualified name, prefixes the _TargetName_ with _TargetType_.
i.e. _TargetType/TargetName_

To use a qualified name, see the `Binding.UseQualifiedName` option.

By default, PSRule uses `/` to separate _TargetType_ from _TargetName_.
This option configures the separator that PSRule uses between the two components.

This option can be specified using:

```powershell
# PowerShell: Using the BindingNameSeparator parameter
$option = New-PSRuleOption -BindingNameSeparator '::';
```

```powershell
# PowerShell: Using the Binding.NameSeparator hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.NameSeparator' = '::' };
```

```powershell
# PowerShell: Using the BindingNameSeparator parameter to set YAML
Set-PSRuleOption -BindingNameSeparator '::';
```

```yaml
# YAML: Using the binding/nameSeparator property
binding:
  nameSeparator: '::'
```

```bash
# Bash: Using environment variable
export PSRULE_BINDING_NAMESEPARATOR='::'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BINDING_NAMESEPARATOR: '::'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BINDING_NAMESEPARATOR
  value: '::'
```

### Binding.PreferTargetInfo

Some built-in objects within PSRule perform automatic binding of TargetName and TargetType.
These built-in objects provide their own target info.

When binding has been configured these values override automatic binding by default.
This can occur when the built-in object uses one of the fields specified by the custom configuration.
The common occurrences of this are on fields such as `Name` and `FullName` which are widely used.
To prefer automatic binding when specified set this option to `$True`.

This option can be specified using:

```powershell
# PowerShell: Using the BindingPreferTargetInfo parameter
$option = New-PSRuleOption -BindingPreferTargetInfo $True;
```

```powershell
# PowerShell: Using the Binding.PreferTargetInfo hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.PreferTargetInfo' = $True };
```

```powershell
# PowerShell: Using the BindingPreferTargetInfo parameter to set YAML
Set-PSRuleOption -BindingPreferTargetInfo $True;
```

```yaml
# YAML: Using the binding/preferTargetInfo property
binding:
  preferTargetInfo: true
```

```bash
# Bash: Using environment variable
export PSRULE_BINDING_PREFERTARGETINFO=false
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BINDING_PREFERTARGETINFO: false
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BINDING_PREFERTARGETINFO
  value: false
```

### Binding.TargetName

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_.
_TargetName_ is used in output results to identify one object from another.
Many objects could be passed down the pipeline at the same time, so using a _TargetName_ that is meaningful is important.
_TargetName_ is also used for advanced features such as rule suppression.

The value that PSRule uses for _TargetName_ is configurable.
PSRule uses the following logic to determine what _TargetName_ should be used:

- By default PSRule will:
  - Use `TargetName` or `Name` properties on the object. These property names are case insensitive.
  - If both `TargetName` and `Name` properties exist, `TargetName` will take precedence over `Name`.
  - If neither `TargetName` or `Name` properties exist, a hash of the object will be used as _TargetName_.
  - The hash algorithm used can be set by the `Execution.HashAlgorithm` option.
- If custom _TargetName_ binding properties are configured, the property names specified will override the defaults.
  - If **none** of the configured property names exist, PSRule will revert back to `TargetName` then `Name`.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity.
    To use a case sensitive match, configure the [Binding.IgnoreCase](#bindingignorecase) option.
- If a custom _TargetName_ binding function is specified, the function will be evaluated first before any other option.
  - If the function returns `$Null` then custom properties, `TargetName` and `Name` properties will be used.
  - The custom binding function is executed outside the PSRule engine, so PSRule keywords and variables will not be available.
  - Custom binding functions are blocked in constrained language mode is used.
    See [language mode](#executionlanguagemode) for more information.

Custom property names to use for binding can be specified using:

```powershell
# PowerShell: Using the TargetName parameter
$option = New-PSRuleOption -TargetName 'ResourceName', 'AlternateName';
```

```powershell
# PowerShell: Using the Binding.TargetName hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.TargetName' = 'ResourceName', 'AlternateName' };
```

```powershell
# PowerShell: Using the TargetName parameter to set YAML
Set-PSRuleOption -TargetName 'ResourceName', 'AlternateName';
```

```yaml
# YAML: Using the binding/targetName property
binding:
  targetName:
  - ResourceName
  - AlternateName
```

```bash
# Bash: Using environment variable
export PSRULE_BINDING_TARGETNAME='ResourceName;AlternateName'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BINDING_TARGETNAME: 'ResourceName;AlternateName'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BINDING_TARGETNAME
  value: 'ResourceName;AlternateName'
```

To specify a custom binding function use:

```powershell
# Create a custom function that returns a TargetName string
$bindFn = {
    param ($TargetObject)

    $otherName = $TargetObject.PSObject.Properties['OtherName'];
    if ($Null -eq $otherName) { return $Null }
    return $otherName.Value;
}

# Specify the binding function script block code to execute
$option = New-PSRuleOption -BindTargetName $bindFn;
```

### Binding.TargetType

When an object is passed from the pipeline, PSRule assigns the object a _TargetType_.
_TargetType_ is used to filter rules based on object type and appears in output results.

The value that PSRule uses for _TargetType_ is configurable.
PSRule uses the following logic to determine what _TargetType_ should be used:

- By default PSRule will:
  - Use the default type presented by PowerShell from `TypeNames`. i.e. `.PSObject.TypeNames[0]`
- If custom _TargetType_ binding properties are configured, the property names specified will override the defaults.
  - If **none** of the configured property names exist, PSRule will revert back to the type presented by PowerShell.
  - If more then one property name is configured, the order they are specified in the configuration determines precedence.
    - i.e. The first configured property name will take precedence over the second property name.
  - By default the property name will be matched ignoring case sensitivity.
    To use a case sensitive match, configure the [`Binding.IgnoreCase`](#bindingignorecase) option.
- If a custom _TargetType_ binding function is specified, the function will be evaluated first before any other option.
  - If the function returns `$Null` then custom properties, or the type presented by PowerShell will be used in order instead.
  - The custom binding function is executed outside the PSRule engine, so PSRule keywords and variables will not be available.
  - Custom binding functions are blocked in constrained language mode is used.
    See [language mode](#executionlanguagemode) for more information.

Custom property names to use for binding can be specified using:

```powershell
# PowerShell: Using the TargetType parameter
$option = New-PSRuleOption -TargetType 'ResourceType', 'kind';
```

```powershell
# PowerShell: Using the Binding.TargetType hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.TargetType' = 'ResourceType', 'kind' };
```

```powershell
# PowerShell: Using the TargetType parameter to set YAML
Set-PSRuleOption -TargetType 'ResourceType', 'kind';
```

```yaml
# YAML: Using the binding/targetType property
binding:
  targetType:
  - ResourceType
  - kind
```

```bash
# Bash: Using environment variable
export PSRULE_BINDING_TARGETTYPE='ResourceType;kind'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BINDING_TARGETTYPE: 'ResourceType;kind'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BINDING_TARGETTYPE
  value: 'ResourceType;kind'
```

To specify a custom binding function use:

```powershell
# Create a custom function that returns a TargetType string
$bindFn = {
    param ($TargetObject)

    $otherType = $TargetObject.PSObject.Properties['OtherType'];

    if ($otherType -eq $Null) {
        return $Null
    }

    return $otherType.Value;
}

# Specify the binding function script block code to execute
$option = New-PSRuleOption -BindTargetType $bindFn;
```

### Binding.UseQualifiedName

When an object is passed from the pipeline, PSRule assigns the object a _TargetName_.
_TargetName_ is used in output results to identify one object from another.

In cases where different types of objects share the same _TargetName_, this may become confusing.
Using a qualified name, prefixes the _TargetName_ with _TargetType_.
i.e. _TargetType/TargetName_

This option determines if PSRule uses qualified or unqualified names (default).

By default, PSRule uses `/` to separate _TargetType_ from _TargetName_.
Set `Binding.NameSeparator` to change.

This option can be specified using:

```powershell
# PowerShell: Using the BindingUseQualifiedName parameter
$option = New-PSRuleOption -BindingUseQualifiedName $True;
```

```powershell
# PowerShell: Using the Binding.UseQualifiedName hashtable key
$option = New-PSRuleOption -Option @{ 'Binding.UseQualifiedName' = $True };
```

```powershell
# PowerShell: Using the BindingUseQualifiedName parameter to set YAML
Set-PSRuleOption -BindingUseQualifiedName $True;
```

```yaml
# YAML: Using the binding/useQualifiedName property
binding:
  useQualifiedName: true
```

```bash
# Bash: Using environment variable
export PSRULE_BINDING_USEQUALIFIEDNAME=false
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_BINDING_USEQUALIFIEDNAME: false
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_BINDING_USEQUALIFIEDNAME
  value: false
```

### Configuration

Configures a set of baseline configuration values that can be used in rule definitions.
Configuration values can be overridden at different scopes.

This option can be specified using:

```powershell
# PowerShell: Using the Configuration option with a hashtable
$option = New-PSRuleOption -Configuration @{ LOCAL_APPSERVICEMININSTANCECOUNT = 2 };
```

```yaml
# YAML: Using the configuration property
configuration:
  LOCAL_APPSERVICEMININSTANCECOUNT: 2
```

Configuration values can be specified using environment variables.
To specify a configuration value, prefix the configuration value with `PSRULE_CONFIGURATION_`.

```bash
# Bash: Using environment variable
export PSRULE_CONFIGURATION_LOCAL_APPSERVICEMININSTANCECOUNT=2
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_CONFIGURATION_LOCAL_APPSERVICEMININSTANCECOUNT: '2'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_CONFIGURATION_LOCAL_APPSERVICEMININSTANCECOUNT
  value: '2'
```

### Convention.Include

Specifies conventions to execute when the pipeline run.
Conventions are included by name and must be defined within files included in `-Path` or `-Module`.

This option can be specified using:

```powershell
# PowerShell: Using the Convention parameter
$option = New-PSRuleOption -Convention 'Convention1', 'Convention2';
```

```powershell
# PowerShell: Using the Convention.Include hashtable key
$option = New-PSRuleOption -Option @{ 'Convention.Include' = $True };
```

```powershell
# PowerShell: Using the Convention parameter to set YAML
Set-PSRuleOption -Convention 'Convention1', 'Convention2';
```

```yaml
# YAML: Using the convention/include property
convention:
  include:
  - 'Convention1'
  - 'Convention2'
```

```bash
# Bash: Using environment variable
export PSRULE_CONVENTION_INCLUDE='Convention1;Convention2'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_CONVENTION_INCLUDE: 'Convention1;Convention2'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_CONVENTION_INCLUDE
  value: 'Convention1;Convention2'
```

### Execution.AliasReference

:octicons-milestone-24: v2.9.0

Determines how to handle when an alias to a resource is used.
By default, a warning is generated, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Warn`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the ExecutionAliasReference parameter
$option = New-PSRuleOption -ExecutionAliasReference 'Error';
```

```powershell
# PowerShell: Using the Execution.AliasReference hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.AliasReference' = 'Error' };
```

```powershell
# PowerShell: Using the ExecutionAliasReference parameter to set YAML
Set-PSRuleOption -ExecutionAliasReference 'Error';
```

```yaml
# YAML: Using the execution/aliasReference property
execution:
  aliasReference: Error
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_ALIASREFERENCE=Error
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_ALIASREFERENCE: Error
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_ALIASREFERENCE
  value: Error
```

### Execution.Break

:octicons-milestone-24: v3.0.0

Determines the minimum rule severity level that breaks the pipeline.
By default, the pipeline will break if a rule of error severity level fails.

For this to take effect the rule must execute successfully and return a failure.
This does not affect the pipeline if other errors or exceptions occurs.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Error`.
- `Never` = (1) - Never break the pipeline if a rule fails regardless of level.
  The pipeline will still break if other errors occur.
- `OnError` = (2) - Break the pipeline if a rule of error severity level fails.
  This is the default.
- `OnWarning` = (3) - Break the pipeline if a rule of warning or error severity level fails.
- `OnInformation` = (4) - Break the pipeline if a rule of information, warning, or error severity level fails.

This option can be specified using:

```powershell
# PowerShell: Using the Break parameter
$option = New-PSRuleOption -ExecutionBreak 'Never';
```

```powershell
# PowerShell: Using the Execution.Break hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.Break' = 'Never' };
```

```powershell
# PowerShell: Using the ExecutionBreak parameter to set YAML
Set-PSRuleOption -ExecutionBreak 'Never';
```

```yaml
# YAML: Using the execution/break property
execution:
  break: Never
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_BREAK=Never
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_BREAK: Never
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_BREAK
  value: Never
```

### Execution.DuplicateResourceId

:octicons-milestone-24: v2.4.0

Determines how to handle duplicate resources identifiers during execution.
A duplicate resource identifier may exist if two resources are defined with the same name, ref, or alias.
By default, an error is thrown, however this behavior can be modified by this option.

If this option is configured to `Warn` or `Ignore` only the first resource will be used,
however PSRule will continue to execute.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Error`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
- `Error` (3) - Abort and throw an error.
  This is the default.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the DuplicateResourceId parameter
$option = New-PSRuleOption -DuplicateResourceId 'Warn';
```

```powershell
# PowerShell: Using the Execution.DuplicateResourceId hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.DuplicateResourceId' = 'Warn' };
```

```powershell
# PowerShell: Using the DuplicateResourceId parameter to set YAML
Set-PSRuleOption -DuplicateResourceId 'Warn';
```

```yaml
# YAML: Using the execution/duplicateResourceId property
execution:
  duplicateResourceId: Warn
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_DUPLICATERESOURCEID=Warn
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_DUPLICATERESOURCEID: Warn
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_DUPLICATERESOURCEID
  value: Warn
```

### Execution.HashAlgorithm

:octicons-milestone-24: v3.0.0

Specifies the hashing algorithm used by the PSRule runtime.
This hash algorithm is used when generating a resource identifier for an object that does not have a bound name.

By default, the _SHA512_ algorithm is used.

The following algorithms are available for use in PSRule:

- `SHA512`
- `SHA384`
- `SHA256`

This option can be specified using:

```powershell
# PowerShell: Using the Execution.HashAlgorithm hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.HashAlgorithm' = 'SHA256' };
```

```yaml
# YAML: Using the execution/hashAlgorithm property
execution:
  hashAlgorithm: SHA256
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_HASHALGORITHM=SHA256
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_HASHALGORITHM: SHA256
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_HASHALGORITHM
  value: SHA256
```

### Execution.LanguageMode

Unless PowerShell has been constrained, full language features of PowerShell are available to use within rule definitions.
In locked down environments, a reduced set of language features may be desired.

When PSRule is executed in an environment configured for Device Guard, only constrained language features are available.

The following language modes are available for use in PSRule:

- `FullLanguage` - Executes with all language features.
  This is the default.
- `ConstrainedLanguage` - Executes in constrained language mode that restricts the types and methods that can be used.

This option can be specified using:

```powershell
# PowerShell: Using the Execution.LanguageMode hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' };
```

```yaml
# YAML: Using the execution/languageMode property
execution:
  languageMode: ConstrainedLanguage
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_LANGUAGEMODE=ConstrainedLanguage
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_LANGUAGEMODE: ConstrainedLanguage
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_LANGUAGEMODE
  value: ConstrainedLanguage
```

### Execution.InvariantCulture

:octicons-milestone-24: v2.9.0

Determines how to report when an invariant culture is used.
By default, a warning is generated, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Warn`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the ExecutionInvariantCulture parameter
$option = New-PSRuleOption -ExecutionInvariantCulture 'Error';
```

```powershell
# PowerShell: Using the Execution.InvariantCulture hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.InvariantCulture' = 'Error' };
```

```powershell
# PowerShell: Using the ExecutionInvariantCulture parameter to set YAML
Set-PSRuleOption -ExecutionInvariantCulture 'Error';
```

```yaml
# YAML: Using the execution/invariantCulture property
execution:
  invariantCulture: Error
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_INVARIANTCULTURE=Error
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_INVARIANTCULTURE: Error
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_INVARIANTCULTURE
  value: Error
```

### Execution.InitialSessionState

:octicons-milestone-24: v2.5.0

Determines how the initial session state for executing PowerShell code is created.

The following preferences are available:

- `BuiltIn` (0) - Create the initial session state with all built-in cmdlets loaded.
  This is the default.
- `Minimal` (1) - Create the initial session state with only a minimum set of cmdlets loaded.

This option can be specified using:

```powershell
# PowerShell: Using the InitialSessionState parameter
$option = New-PSRuleOption -InitialSessionState 'Minimal';
```

```powershell
# PowerShell: Using the Execution.InitialSessionState hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.InitialSessionState' = 'Minimal' };
```

```powershell
# PowerShell: Using the InitialSessionState parameter to set YAML
Set-PSRuleOption -InitialSessionState 'Minimal';
```

```yaml
# YAML: Using the execution/initialSessionState property
execution:
  initialSessionState: Minimal
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_INITIALSESSIONSTATE=Minimal
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_INITIALSESSIONSTATE: Minimal
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_INITIALSESSIONSTATE
  value: Minimal
```

### Execution.RestrictScriptSource

:octicons-milestone-24: v3.0.0

Configures where to allow PowerShell language features (such as rules and conventions) to run from.
In locked down environments, running PowerShell scripts from the workspace may not be allowed.
Only run scripts from a trusted source.

This option does not affect YAML or JSON based rules and resources.

The following script source restrictions are available:

- `Unrestricted` - PowerShell language features are allowed from workspace and modules.
  This is the default.
- `ModuleOnly` - PowerShell language features are allowed from loaded modules,
  but script files within the workspace are ignored.
- `DisablePowerShell` - No PowerShell language features are used during PSRule run.
  When this mode is used, rules and conventions written in PowerShell will not execute.
  Modules that use PowerShell rules and conventions may not work as expected.

This option can be specified using:

```powershell
# PowerShell: Using the RestrictScriptSource parameter
$option = New-PSRuleOption -RestrictScriptSource 'ModuleOnly';
```

```powershell
# PowerShell: Using the Execution.RestrictScriptSource hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.RestrictScriptSource' = 'ModuleOnly' };
```

```powershell
# PowerShell: Using the RestrictScriptSource parameter to set YAML
Set-PSRuleOption -RestrictScriptSource 'ModuleOnly';
```

```yaml
# YAML: Using the execution/restrictScriptSource property
execution:
  restrictScriptSource: ModuleOnly
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE=ModuleOnly
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE: ModuleOnly
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_RESTRICTSCRIPTSOURCE
  value: ModuleOnly
```

### Execution.RuleInconclusive

:octicons-milestone-24: v2.9.0

Determines how to handle rules that generate inconclusive results.
By default, a warning is generated, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Warn`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the ExecutionRuleInconclusive parameter
$option = New-PSRuleOption -ExecutionRuleInconclusive 'Error';
```

```powershell
# PowerShell: Using the Execution.RuleInconclusive hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.RuleInconclusive' = 'Error' };
```

```powershell
# PowerShell: Using the ExecutionRuleInconclusive parameter to set YAML
Set-PSRuleOption -ExecutionRuleInconclusive 'Error';
```

```yaml
# YAML: Using the execution/ruleInconclusive property
execution:
  ruleInconclusive: Error
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_RULEINCONCLUSIVE=Error
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_RULEINCONCLUSIVE: Error
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_RULEINCONCLUSIVE
  value: Error
```

### Execution.SuppressionGroupExpired

:octicons-milestone-24: v2.6.0

Determines how to handle expired suppression groups.
Regardless of the value, an expired suppression group will be ignored.
By default, a warning is generated, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Warn`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the SuppressionGroupExpired parameter
$option = New-PSRuleOption -SuppressionGroupExpired 'Error';
```

```powershell
# PowerShell: Using the Execution.SuppressionGroupExpired hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.SuppressionGroupExpired' = 'Error' };
```

```powershell
# PowerShell: Using the SuppressionGroupExpired parameter to set YAML
Set-PSRuleOption -SuppressionGroupExpired 'Error';
```

```yaml
# YAML: Using the execution/suppressionGroupExpired property
execution:
  suppressionGroupExpired: Error
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED=Error
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED: Error
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_SUPPRESSIONGROUPEXPIRED
  value: Error
```

### Execution.RuleExcluded

:octicons-milestone-24: v2.8.0

Determines how to handle excluded rules.
Regardless of the value, excluded rules are ignored.
By default, a rule is excluded silently, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Ignore`.
- `Ignore` (1) - Continue to execute silently.
  This is the default.
- `Warn` (2) - Continue to execute but log a warning.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the ExecutionRuleExcluded parameter
$option = New-PSRuleOption -ExecutionRuleExcluded 'Warn';
```

```powershell
# PowerShell: Using the Execution.RuleExcluded hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.RuleExcluded' = 'Warn' };
```

```powershell
# PowerShell: Using the ExecutionRuleExcluded parameter to set YAML
Set-PSRuleOption -ExecutionRuleExcluded 'Warn';
```

```yaml
# YAML: Using the execution/ruleExcluded property
execution:
  ruleExcluded: Warn
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_RULEEXCLUDED=Warn
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_RULEEXCLUDED: Warn
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_RULEEXCLUDED
  value: Warn
```

### Execution.RuleSuppressed

:octicons-milestone-24: v2.8.0

Determines how to handle suppressed rules.
Regardless of the value, a suppressed rule is ignored.
By default, a warning is generated, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Warn`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

```powershell
# PowerShell: Using the ExecutionRuleSuppressed parameter
$option = New-PSRuleOption -ExecutionRuleSuppressed 'Error';
```

```powershell
# PowerShell: Using the Execution.RuleSuppressed hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.RuleSuppressed' = 'Error' };
```

```powershell
# PowerShell: Using the ExecutionRuleSuppressed parameter to set YAML
Set-PSRuleOption -ExecutionRuleSuppressed 'Error';
```

```yaml
# YAML: Using the execution/ruleSuppressed property
execution:
  ruleSuppressed: Error
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_RULESUPPRESSED=Error
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_RULESUPPRESSED: Error
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_RULESUPPRESSED
  value: Error
```

### Execution.UnprocessedObject

:octicons-milestone-24: v2.9.0

Determines how to report objects that are not processed by any rule.
By default, a warning is generated, however this behavior can be modified by this option.

The following preferences are available:

- `None` (0) - No preference.
  Inherits the default of `Warn`.
- `Ignore` (1) - Continue to execute silently.
- `Warn` (2) - Continue to execute but log a warning.
  This is the default.
- `Error` (3) - Abort and throw an error.
- `Debug` (4) - Continue to execute but log a debug message.

This option can be specified using:

```powershell
# PowerShell: Using the ExecutionUnprocessedObject parameter
$option = New-PSRuleOption -ExecutionUnprocessedObject 'Ignore';
```

```powershell
# PowerShell: Using the Execution.UnprocessedObject hashtable key
$option = New-PSRuleOption -Option @{ 'Execution.UnprocessedObject' = 'Ignore' };
```

```powershell
# PowerShell: Using the ExecutionUnprocessedObject parameter to set YAML
Set-PSRuleOption -ExecutionUnprocessedObject 'Ignore';
```

```yaml
# YAML: Using the execution/unprocessedObject property
execution:
  unprocessedObject: Ignore
```

```bash
# Bash: Using environment variable
export PSRULE_EXECUTION_UNPROCESSEDOBJECT=Ignore
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_EXECUTION_UNPROCESSEDOBJECT: Ignore
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_EXECUTION_UNPROCESSEDOBJECT
  value: Ignore
```

### Include.Module

Automatically include rules and resources from the specified module.
To automatically import and include a module specify the module by name.
The module must already be installed on the system.

When `$PSModuleAutoLoadingPreference` is set to a value other then `All` the module must be imported.

This option is equivalent to using the `-Module` parameter on PSRule cmdlets, with the following addition:

- Modules specified with `Include.Module` are combined with `-Module`.
  Both sets of modules will be imported and used using execution.

This option can be specified using:

```powershell
# PowerShell: Using the IncludeModule parameter
$option = New-PSRuleOption -IncludeModule 'TestModule1', 'TestModule2';
```

```powershell
# PowerShell: Using the Include.Module hashtable key
$option = New-PSRuleOption -Option @{ 'Include.Module' = 'TestModule1', 'TestModule2' };
```

```powershell
# PowerShell: Using the IncludeModule parameter to set YAML
Set-PSRuleOption -IncludeModule 'TestModule1', 'TestModule2';
```

```yaml
# YAML: Using the include/module property
include:
  module:
  - TestModule1
```

```bash
# Bash: Using environment variable
export PSRULE_INCLUDE_MODULE=TestModule1;TestModule2
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INCLUDE_MODULE: TestModule1;TestModule2
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INCLUDE_MODULE
  value: TestModule1;TestModule2
```

### Include.Path

Automatically include rules and resources from the specified path.
By default, `.ps-rule/` is included.

This option is equivalent to using the `-Path` parameter on PSRule cmdlets, with the following additions:

- Paths specified with `Include.Path` are combined with `-Path`.
  Both sets of paths will be imported and used using execution.
- The `Include.Path` option defaults to `.ps-rule/`.
  To override this default, specify one or more alternative paths or an empty array.

This option can be specified using:

```powershell
# PowerShell: Using the IncludePath parameter
$option = New-PSRuleOption -IncludePath '.ps-rule/', 'custom-rules/';
```

```powershell
# PowerShell: Using the Include.Path hashtable key
$option = New-PSRuleOption -Option @{ 'Include.Path' = '.ps-rule/', 'custom-rules/' };
```

```powershell
# PowerShell: Using the IncludePath parameter to set YAML
Set-PSRuleOption -IncludePath '.ps-rule/', 'custom-rules/';
```

```yaml
# YAML: Using the include/path property
include:
  path:
  - custom-rules/
```

```bash
# Bash: Using environment variable
export PSRULE_INCLUDE_PATH=.ps-rule/;custom-rules/
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INCLUDE_PATH: .ps-rule/;custom-rules/
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INCLUDE_PATH
  value: .ps-rule/;custom-rules/
```

### Input.FileObjects

:octicons-milestone-24: v3.0.0

Determines if file objects are processed by rules.
This option is for backwards compatibility with PSRule v2.x in cases where file objects are used as input.

By default, file are not processed by rules.
Set to `$True` to enable processing of file objects by rules.

This option can be specified using:

```powershell
# PowerShell: Using the InputFileObjects parameter
$option = New-PSRuleOption -InputFileObjects $True;
```

```powershell
# PowerShell: Using the Input.FileObjects hashtable key
$option = New-PSRuleOption -Option @{ 'Input.FileObjects' = $True };
```

```powershell
# PowerShell: Using the InputFileObjects parameter to set YAML
Set-PSRuleOption -InputFileObjects $True;
```

```yaml
# YAML: Using the input/fileObjects property
input:
  fileObjects: true
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_FILEOBJECTS=true
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_FILEOBJECTS: true
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_FILEOBJECTS
  value: true
```

### Input.Format

Configures the input format for when a string is passed in as a target object.
This option determines if the target object is deserialized into an alternative form.

Use this option with `Assert-PSRule`, `Invoke-PSRule` or `Test-PSRuleTarget`.
Set this option to either `Yaml`, `Json`, `Markdown`, `PowerShellData` to deserialize as a specific format.
The `-Format` parameter will override any value set in configuration.

When the `-InputObject` parameter or pipeline input is used, strings are treated as plain text by default.
`FileInfo` objects for supported file formats will be deserialized based on file extension.

When the `-InputPath` parameter is used, supported file formats will be deserialized based on file extension.
The `-InputPath` parameter can be used with a file path or URL.

The following formats are available:

- `None` - Treat strings as plain text and do not deserialize files.
  This is the default.
- `Yaml` - Deserialize as one or more YAML objects.
- `Json` - Deserialize as one or more JSON objects.
- `Markdown` - Deserialize as a markdown object.
- `PowerShellData` - Deserialize as a PowerShell data object.

The `Markdown` format does not parse the whole markdown document.
Specifically this format deserializes YAML front matter from the top of the document if any exists.

Files within `.git` sub-directories are ignored.
Path specs specified in `.gitignore` directly in the current working path are ignored.
A `RepositoryInfo` object is generated if the current working path if a `.git` sub-directory is present.
Additionally, PSRule performs automatic type binding for file objects, using the extension as the type.
When files have no extension the whole file name is used.

Detect uses the following file extensions:

- Yaml - `.yaml` or `.yml`
- Json - `.json` or `.jsonc`
- Markdown - `.md` or `.markdown`
- PowerShellData - `.psd1`

This option can be specified using:

```powershell
# PowerShell: Using the Format parameter
$option = New-PSRuleOption -Format Yaml;
```

```powershell
# PowerShell: Using the Input.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Input.Format' = 'Yaml' };
```

```powershell
# PowerShell: Using the Format parameter to set YAML
Set-PSRuleOption -Format Yaml;
```

```yaml
# YAML: Using the input/format property
input:
  format: Yaml
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_FORMAT=Yaml
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_FORMAT: Yaml
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_FORMAT
  value: Yaml
```

### Input.IgnoreGitPath

When reading files from an input path, files within the `.git` sub-directory are ignored by default.
Files stored within the `.git` sub-directory are system repository files used by git.
To read files stored within the `.git` path, set this option to `$False`.

This option can be specified using:

```powershell
# PowerShell: Using the InputIgnoreGitPath parameter
$option = New-PSRuleOption -InputIgnoreGitPath $False;
```

```powershell
# PowerShell: Using the Input.IgnoreGitPath hashtable key
$option = New-PSRuleOption -Option @{ 'Input.IgnoreGitPath' = $False };
```

```powershell
# PowerShell: Using the InputIgnoreGitPath parameter to set YAML
Set-PSRuleOption -InputIgnoreGitPath $False;
```

```yaml
# YAML: Using the input/ignoreGitPath property
input:
  ignoreGitPath: false
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_IGNOREGITPATH=false
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_IGNOREGITPATH: false
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_IGNOREGITPATH
  value: false
```

### Input.IgnoreObjectSource

By default, objects read from file using `inputPath` will be skipped if the file path has been ignored.
When set to true, additionally objects with a source path that has been ignored will be skipped.
This will include `FileInfo` objects, and objects with a source set using the `_PSRule.source` property.

File paths to ignore are set by `Input.PathIgnore`, `Input.IgnoreGitPath`, and `Input.IgnoreRepositoryCommon`.

This option can be specified using:

```powershell
# PowerShell: Using the InputIgnoreObjectSource parameter
$option = New-PSRuleOption -InputIgnoreObjectSource $True;
```

```powershell
# PowerShell: Using the Input.IgnoreObjectSource hashtable key
$option = New-PSRuleOption -Option @{ 'Input.IgnoreObjectSource' = $True };
```

```powershell
# PowerShell: Using the InputIgnoreObjectSource parameter to set YAML
Set-PSRuleOption -InputIgnoreObjectSource $True;
```

```yaml
# YAML: Using the input/ignoreObjectSource property
input:
  ignoreObjectSource: true
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_IGNOREOBJECTSOURCE=true
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_IGNOREOBJECTSOURCE: true
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_IGNOREOBJECTSOURCE
  value: true
```

### Input.IgnoreRepositoryCommon

When reading files from an input path, files are discovered recursively.
A number of files are commonly found within a private and open-source repositories.
In many cases these files are of no interest for analysis and should be ignored by rules.
PSRule will ignore the following files by default:

- `README.md`
- `.DS_Store`
- `.gitignore`
- `.gitattributes`
- `.gitmodules`
- `LICENSE`
- `LICENSE.txt`
- `CODE_OF_CONDUCT.md`
- `CONTRIBUTING.md`
- `SECURITY.md`
- `SUPPORT.md`
- `.vscode/*.json`
- `.vscode/*.code-snippets`
- `.github/**/*.md`
- `.github/CODEOWNERS`
- `.pipelines/**/*.yml`
- `.pipelines/**/*.yaml`
- `.azure-pipelines/**/*.yml`
- `.azure-pipelines/**/*.yaml`
- `.azuredevops/*.md`

To include these files, set this option to `$False`.
This option can be specified using:

```powershell
# PowerShell: Using the InputIgnoreRepositoryCommon parameter
$option = New-PSRuleOption -InputIgnoreRepositoryCommon $False;
```

```powershell
# PowerShell: Using the Input.IgnoreRepositoryCommon hashtable key
$option = New-PSRuleOption -Option @{ 'Input.IgnoreRepositoryCommon' = $False };
```

```powershell
# PowerShell: Using the InputIgnoreRepositoryCommon parameter to set YAML
Set-PSRuleOption -InputIgnoreRepositoryCommon $False;
```

```yaml
# YAML: Using the input/ignoreRepositoryCommon property
input:
  ignoreRepositoryCommon: false
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_IGNOREREPOSITORYCOMMON=false
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_IGNOREREPOSITORYCOMMON: false
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_IGNOREREPOSITORYCOMMON
  value: false
```

### Input.IgnoreUnchangedPath

:octicons-milestone-24: v2.5.0

By default, PSRule will process all files within an input path.
For large repositories, this can result in a large number of files being processed.
Additionally, for a pull request you may only be interested in files that have changed.

When set to `true`, files that have not changed will be ignored.
This option can be specified using:

```powershell
# PowerShell: Using the InputIgnoreUnchangedPath parameter
$option = New-PSRuleOption -InputIgnoreUnchangedPath $True;
```

```powershell
# PowerShell: Using the Input.IgnoreUnchangedPath hashtable key
$option = New-PSRuleOption -Option @{ 'Input.IgnoreUnchangedPath' = $True };
```

```powershell
# PowerShell: Using the InputIgnoreUnchangedPath parameter to set YAML
Set-PSRuleOption -InputIgnoreUnchangedPath $True;
```

```yaml
# YAML: Using the input/ignoreUnchangedPath property
input:
  ignoreUnchangedPath: true
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_IGNOREUNCHANGEDPATH=true
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_IGNOREUNCHANGEDPATH: true
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_IGNOREUNCHANGEDPATH
  value: true
```

### Input.ObjectPath

The object path to a property to use instead of the pipeline object.

By default, PSRule processes objects passed from the pipeline against selected rules.
When this option is set, instead of evaluating the pipeline object,
PSRule looks for a property of the pipeline object specified by `ObjectPath` and uses that instead.
If the property specified by `ObjectPath` is a collection/ array, then each item is evaluated separately.

If the property specified by `ObjectPath` does not exist, PSRule skips the object.

When using `Invoke-PSRule`, `Test-PSRuleTarget`, and `Assert-PSRule` the `-ObjectPath` parameter will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the ObjectPath parameter
$option = New-PSRuleOption -ObjectPath 'items';
```

```powershell
# PowerShell: Using the Input.ObjectPath hashtable key
$option = New-PSRuleOption -Option @{ 'Input.ObjectPath' = 'items' };
```

```powershell
# PowerShell: Using the ObjectPath parameter to set YAML
Set-PSRuleOption -ObjectPath 'items';
```

```yaml
# YAML: Using the input/objectPath property
input:
  objectPath: items
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_OBJECTPATH=items
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_OBJECTPATH: items
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_OBJECTPATH
  value: items
```

### Input.PathIgnore

Ignores input files that match the path spec when using `-InputPath`.
If specified, files that match the path spec will not be processed.
By default, all files are processed.

For example, ignoring file extensions:

```yaml
input:
  pathIgnore:
  # Exclude files with these extensions
  - '*.md'
  - '*.png'
  # Exclude specific configuration files
  - 'bicepconfig.json'
```

For example, ignoring all files with exceptions:

```yaml
input:
  pathIgnore:
  # Exclude all files
  - '*'
  # Only process deploy.bicep files
  - '!**/deploy.bicep'
```

This option can be specified using:

```powershell
# PowerShell: Using the InputPathIgnore parameter
$option = New-PSRuleOption -InputPathIgnore '*.Designer.cs';
```

```powershell
# PowerShell: Using the Input.PathIgnore hashtable key
$option = New-PSRuleOption -Option @{ 'Input.PathIgnore' = '*.Designer.cs' };
```

```powershell
# PowerShell: Using the InputPathIgnore parameter to set YAML
Set-PSRuleOption -InputPathIgnore '*.Designer.cs';
```

```yaml
# YAML: Using the input/pathIgnore property
input:
  pathIgnore:
  - '*.Designer.cs'
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_PATHIGNORE=*.Designer.cs
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_PATHIGNORE: '*.Designer.cs'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_PATHIGNORE
  value: '*.Designer.cs'
```

### Input.TargetType

Filters input objects by TargetType.

If specified, only objects with the specified TargetType are processed.
Objects that do not match TargetType are ignored.
If multiple values are specified, only one TargetType must match. This option is not case-sensitive.

By default, all objects are processed.

To change the field TargetType is bound to set the `Binding.TargetType` option.

When using `Invoke-PSRule`, `Test-PSRuleTarget`, and `Assert-PSRule` the `-TargetType` parameter will override any value set in configuration.

This option can be specified using:

```powershell
# PowerShell: Using the InputTargetType parameter
$option = New-PSRuleOption -InputTargetType 'virtualMachine', 'virtualNetwork';
```

```powershell
# PowerShell: Using the Input.TargetType hashtable key
$option = New-PSRuleOption -Option @{ 'Input.TargetType' = 'virtualMachine', 'virtualNetwork' };
```

```powershell
# PowerShell: Using the InputTargetType parameter to set YAML
Set-PSRuleOption -InputTargetType 'virtualMachine', 'virtualNetwork';
```

```yaml
# YAML: Using the input/targetType property
input:
  targetType:
  - virtualMachine
```

```bash
# Bash: Using environment variable
export PSRULE_INPUT_TARGETTYPE=virtualMachine;virtualNetwork
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_INPUT_TARGETTYPE: virtualMachine;virtualNetwork
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_INPUT_TARGETTYPE
  value: virtualMachine;virtualNetwork
```

### Logging.LimitDebug

Limits debug messages to a list of named debug scopes.

When using the `-Debug` switch or preference variable, by default PSRule cmdlets log all debug output.
When using debug output for debugging a specific rule, it may be helpful to limit debug message to a specific rule.

To identify a rule to include in debug output use the rule name.

The following built-in scopes exist in addition to rule names:

- `[Discovery.Source]` - Discovery messages for `.Rule.ps1` files and rule modules.
- `[Discovery.Rule]` - Discovery messages for individual rules within `.Rule.ps1` files.

This option can be specified using:

```powershell
# PowerShell: Using the LoggingLimitDebug parameter
$option = New-PSRuleOption -LoggingLimitDebug Rule1, Rule2;
```

```powershell
# PowerShell: Using the Logging.LimitDebug hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.LimitDebug' = Rule1, Rule2 };
```

```powershell
# PowerShell: Using the LoggingLimitDebug parameter to set YAML
Set-PSRuleOption -LoggingLimitDebug Rule1, Rule2;
```

```yaml
# YAML: Using the logging/limitDebug property
logging:
  limitDebug:
  - Rule1
  - Rule2
```

### Logging.LimitVerbose

Limits verbose messages to a list of named verbose scopes.

When using the `-Verbose` switch or preference variable, by default PSRule cmdlets log all verbose output.
When using verbose output for troubleshooting a specific rule,
it may be helpful to limit verbose messages to a specific rule.

To identify a rule to include in verbose output use the rule name.

The following built-in scopes exist in addition to rule names:

- `[Discovery.Source]` - Discovery messages for `.Rule.ps1` files and rule modules.
- `[Discovery.Rule]` - Discovery messages for individual rules within `.Rule.ps1` files.

This option can be specified using:

```powershell
# PowerShell: Using the LoggingLimitVerbose parameter
$option = New-PSRuleOption -LoggingLimitVerbose Rule1, Rule2;
```

```powershell
# PowerShell: Using the Logging.LimitVerbose hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.LimitVerbose' = Rule1, Rule2 };
```

```powershell
# PowerShell: Using the LoggingLimitVerbose parameter to set YAML
Set-PSRuleOption -LoggingLimitVerbose Rule1, Rule2;
```

```yaml
# YAML: Using the logging/limitVerbose property
logging:
  limitVerbose:
  - Rule1
  - Rule2
```

### Logging.RuleFail

When an object fails a rule condition the results are written to output as a structured object marked with the outcome of _Fail_.
If the rule executed successfully regardless of outcome no other informational messages are shown by default.

In some circumstances such as a continuous integration (CI) pipeline,
it may be preferable to see informational messages or abort the CI process if one or more _Fail_ outcomes are returned.

By settings this option, error, warning or information messages will be generated for each rule _fail_ outcome in addition to structured output.
By default, outcomes are not logged to an informational stream (i.e. None).

The following streams available:

- None
- Error
- Warning
- Information

This option can be specified using:

```powershell
# PowerShell: Using the LoggingRuleFail parameter
$option = New-PSRuleOption -LoggingRuleFail Error;
```

```powershell
# PowerShell: Using the Logging.RuleFail hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.RuleFail' = 'Error' };
```

```powershell
# PowerShell: Using the LoggingRuleFail parameter to set YAML
Set-PSRuleOption -LoggingRuleFail Error;
```

```yaml
# YAML: Using the logging/ruleFail property
logging:
  ruleFail: Error
```

### Logging.RulePass

When an object passes a rule condition the results are written to output as a structured object marked with the outcome of _Pass_.
If the rule executed successfully regardless of outcome no other informational messages are shown by default.

In some circumstances such as a continuous integration (CI) pipeline, it may be preferable to see informational messages.

By settings this option, error, warning or information messages will be generated for each rule _pass_ outcome in addition to structured output.
By default, outcomes are not logged to an informational stream (i.e. None).

The following streams available:

- None
- Error
- Warning
- Information

This option can be specified using:

```powershell
# PowerShell: Using the LoggingRulePass parameter
$option = New-PSRuleOption -LoggingRulePass Information;
```

```powershell
# PowerShell: Using the Logging.RulePass hashtable key
$option = New-PSRuleOption -Option @{ 'Logging.RulePass' = 'Information' };
```

```powershell
# PowerShell: Using the LoggingRulePass parameter to set YAML
Set-PSRuleOption -LoggingRulePass Information;
```

```yaml
# YAML: Using the logging/rulePass property
logging:
  rulePass: Information
```

### Output.As

Configures the type of results to produce.

This option only applies to `Invoke-PSRule` and `Assert-PSRule`.
`Invoke-PSRule` and `Assert-PSRule` also include a `-As` parameter to set this option at runtime.
If specified, the `-As` parameter take precedence, over this option.

The following options are available:

- Detail - Return a record per rule per object.
- Summary - Return summary results.

This option can be specified using:

```powershell
# PowerShell: Using the OutputAs parameter
$option = New-PSRuleOption -OutputAs Summary;
```

```powershell
# PowerShell: Using the Output.As hashtable key
$option = New-PSRuleOption -Option @{ 'Output.As' = 'Summary' };
```

```powershell
# PowerShell: Using the OutputAs parameter to set YAML
Set-PSRuleOption -OutputAs Summary;
```

```yaml
# YAML: Using the output/as property
output:
  as: Summary
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_AS=Summary
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_AS: Summary
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_AS
  value: Summary
```

### Output.Banner

The information displayed for PSRule banner.
This option is only applicable when using `Assert-PSRule` cmdlet.

The following information can be shown or hidden by configuring this option.

- `Title` (1) - Shows the PSRule title ASCII text.
- `Source` (2) - Shows rules module versions used in this run.
- `SupportLinks` (4) - Shows supporting links for PSRule and rules modules.
- `RepositoryInfo` (8) - Show information about the repository where PSRule is being run from.

Additionally the following rollup options exist:

- `Default` - Shows `Title`, `Source`, `SupportLinks`, `RepositoryInfo`.
This is the default option.
- `Minimal` - Shows `Source`.

This option can be configured using one of the named values described above.
Alternatively, this value can be configured by specifying a bit mask as an integer.
For example `6` would show `Source`, and `SupportLinks`.

This option can be specified using:

```powershell
# PowerShell: Using the OutputBanner parameter
$option = New-PSRuleOption -OutputBanner Minimal;
```

```powershell
# PowerShell: Using the Output.Banner hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Banner' = 'Minimal' };
```

```powershell
# PowerShell: Using the OutputBanner parameter to set YAML
Set-PSRuleOption -OutputBanner Minimal;
```

```yaml
# YAML: Using the output/banner property
output:
  banner: Minimal
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_BANNER=Minimal
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_BANNER: Minimal
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_BANNER
  value: Minimal
```

### Output.Culture

Specified the name of one or more cultures to use for generating output.
When multiple cultures are specified, the first matching culture will be used.
If a culture is not specified, PSRule will use the current PowerShell culture.

PSRule cmdlets also include a `-Culture` parameter to set this option at runtime.
If specified, the `-Culture` parameter take precedence, over this option.

To get a list of cultures use the `Get-Culture -ListAvailable` cmdlet.

This option can be specified using:

```powershell
# PowerShell: Using the OutputCulture parameter
$option = New-PSRuleOption -OutputCulture 'en-AU';
```

```powershell
# PowerShell: Using the Output.Culture hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Culture' = 'en-AU' };
```

```powershell
# PowerShell: Using the OutputCulture parameter to set YAML
Set-PSRuleOption -OutputCulture 'en-AU', 'en-US';
```

```yaml
# YAML: Using the output/culture property
output:
  culture: [ 'en-AU', 'en-US' ]
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_CULTURE=en-AU;en-US
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_CULTURE: en-AU;en-US
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_CULTURE
  value: en-AU;en-US
```

### Output.Encoding

Configures the encoding used when output is written to file.
This option has no affect when `Output.Path` is not set.

The following encoding options are available:

- Default
- UTF-8
- UTF-7
- Unicode
- UTF-32
- ASCII

This option can be specified using:

```powershell
# PowerShell: Using the OutputEncoding parameter
$option = New-PSRuleOption -OutputEncoding UTF8;
```

```powershell
# PowerShell: Using the Output.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Encoding' = 'UTF8' };
```

```powershell
# PowerShell: Using the OutputEncoding parameter to set YAML
Set-PSRuleOption -OutputEncoding UTF8;
```

```yaml
# YAML: Using the output/encoding property
output:
  encoding: UTF8
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_ENCODING=UTF8
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_ENCODING: UTF8
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_ENCODING
  value: UTF8
```

### Output.Footer

The information displayed for PSRule footer.
This option is only applicable when using `Assert-PSRule` cmdlet.

The following information can be shown or hidden by configuring this option.

- `RuleCount` (1) - Shows a summary of rules processed.
- `RunInfo` (2) - Shows information about the run.
- `OutputFile` (4) - Shows information about the output file if an output path is set.

Additionally the following rollup options exist:

- `Default` - Shows `RuleCount`, `RunInfo`, and `OutputFile`.
This is the default option.

This option can be configured using one of the named values described above.
Alternatively, this value can be configured by specifying a bit mask as an integer.
For example `3` would show `RunInfo`, and `RuleCount`.

This option can be specified using:

```powershell
# PowerShell: Using the OutputFooter parameter
$option = New-PSRuleOption -OutputFooter RuleCount;
```

```powershell
# PowerShell: Using the Output.Footer hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Footer' = 'RuleCount' };
```

```powershell
# PowerShell: Using the OutputFooter parameter to set YAML
Set-PSRuleOption -OutputFooter RuleCount;
```

```yaml
# YAML: Using the output/footer property
output:
  footer: RuleCount
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_FOOTER=RuleCount
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_FOOTER: RuleCount
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_FOOTER
  value: RuleCount
```

### Output.Format

Configures the format that results will be presented in.
This option applies to `Invoke-PSRule`, `Assert-PSRule`, `Get-PSRule` and `Get-PSRuleBaseline`.
This options is ignored by other cmdlets.

The following format options are available:

- None - Output is presented as an object using PowerShell defaults.
  This is the default.
- Yaml - Output is serialized as YAML.
- Json - Output is serialized as JSON.
- Markdown - Output is serialized as Markdown.
- NUnit3 - Output is serialized as NUnit3 (XML).
- Csv - Output is serialized as a comma-separated values (CSV).
  - The following columns are included for `Detail` output:
RuleName, TargetName, TargetType, Outcome, OutcomeReason, Synopsis, Recommendation
  - The following columns are included for `Summary` output:
RuleName, Pass, Fail, Outcome, Synopsis, Recommendation
- Wide -  Output is presented using the wide table format, which includes reason and wraps columns.
- Sarif - Output is serialized as SARIF.

The Wide format is ignored by `Assert-PSRule`.
`Get-PSRule` only accepts `None`, `Wide`, `Yaml` and `Json`.
Usage of other formats are treated as `None`.

The `Get-PSRuleBaseline` cmdlet only accepts `None` or `Yaml`.
The `Export-PSRuleBaseline` cmdlet only accepts `Yaml`.

This option can be specified using:

```powershell
# PowerShell: Using the OutputFormat parameter
$option = New-PSRuleOption -OutputFormat Yaml;
```

```powershell
# PowerShell: Using the Output.Format hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Format' = 'Yaml' };
```

```powershell
# PowerShell: Using the OutputFormat parameter to set YAML
Set-PSRuleOption -OutputFormat Yaml;
```

```yaml
# YAML: Using the output/format property
output:
  format: Yaml
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_FORMAT=Yaml
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_FORMAT: Yaml
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_FORMAT
  value: Yaml
```

### Output.Outcome

Filters output to include results with the specified outcome.
The following outcome options are available:

- `None` (0) - Results for rules that did not get processed are returned.
  This include rules that have been suppressed or were not run against a target object.
- `Fail` (1) - Results for rules that failed are returned.
- `Pass` (2)  - Results for rules that passed are returned.
- `Error` (4) - Results for rules that raised an error are returned.

Additionally the following rollup options exist:

- `Processed` - Results for rules with the `Fail`, `Pass`, or `Error` outcome.
This is the default option.
- `Problem` - Results for rules with the `Fail`, or `Error` outcome.
- `All` - All results for rules are returned.

This option can be specified using:

```powershell
# PowerShell: Using the OutputOutcome parameter
$option = New-PSRuleOption -OutputOutcome Fail;
```

```powershell
# PowerShell: Using the Output.Outcome hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Outcome' = 'Fail' };
```

```powershell
# PowerShell: Using the OutputOutcome parameter to set YAML
Set-PSRuleOption -OutputOutcome Fail;
```

```yaml
# YAML: Using the output/outcome property
output:
  outcome: 'Fail'
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_OUTCOME=Fail
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_OUTCOME: Fail
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_OUTCOME
  value: Fail
```

### Output.Path

Specifies the output file path to write results.
Directories along the file path will automatically be created if they do not exist.

This option only applies to `Invoke-PSRule`.
`Invoke-PSRule` also includes a parameter `-OutputPath` to set this option at runtime.
If specified, the `-OutputPath` parameter take precedence, over this option.

Syntax:

```yaml
output:
  path: string
```

Default:

```yaml
output:
  path: null
```

This option can be specified using:

```powershell
# PowerShell: Using the OutputPath parameter
$option = New-PSRuleOption -OutputPath 'out/results.yaml';
```

```powershell
# PowerShell: Using the Output.Path hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Path' = 'out/results.yaml' };
```

```powershell
# PowerShell: Using the OutputPath parameter to set YAML
Set-PSRuleOption -OutputPath 'out/results.yaml';
```

```yaml
# YAML: Using the output/path property
output:
  path: 'out/results.yaml'
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_PATH=out/results.yaml
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_PATH: out/results.yaml
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_PATH
  value: out/results.yaml
```

### Output.SarifProblemsOnly

Determines if SARIF output only includes rules with fail or error outcomes.
By default, only rules with fail or error outcomes are included for compatibility with external tools.
To include rules with pass outcomes, set this option to `false`.
This option only applies when the output format is `Sarif`.

Syntax:

```yaml
output:
  sarifProblemsOnly: boolean
```

Default:

```yaml
output:
  sarifProblemsOnly: true
```

This option can be specified using:

```powershell
# PowerShell: Using the OutputSarifProblemsOnly parameter
$option = New-PSRuleOption -OutputSarifProblemsOnly $False;
```

```powershell
# PowerShell: Using the Output.SarifProblemsOnly hashtable key
$option = New-PSRuleOption -Option @{ 'Output.SarifProblemsOnly' = $False };
```

```powershell
# PowerShell: Using the OutputSarifProblemsOnly parameter to set YAML
Set-PSRuleOption -OutputSarifProblemsOnly $False;
```

```yaml
# YAML: Using the output/sarifProblemsOnly property
output:
  sarifProblemsOnly: false
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_SARIFPROBLEMSONLY=false
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_SARIFPROBLEMSONLY: false
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_SARIFPROBLEMSONLY
  value: false
```

### Output.Style

Configures the style that results will be presented in.

This option only applies to output generated from `Assert-PSRule`.
`Assert-PSRule` also include a parameter `-Style` to set this option at runtime.
If specified, the `-Style` parameter takes precedence, over this option.

The following styles are available:

- `Client` - Output is written to the host directly in green/ red to indicate outcome.
- `Plain` - Output is written as an unformatted string.
This option can be redirected to a file.
- `AzurePipelines` - Output is written for integration Azure Pipelines.
- `GitHubActions` - Output is written for integration GitHub Actions.
- `VisualStudioCode` - Output is written for integration with Visual Studio Code.
- `Detect` - Output style will be detected by checking the environment variables.
This is the default.

Detect uses the following logic:

1. If the `TF_BUILD` environment variable is set to `true`, `AzurePipelines` will be used.
2. If the `GITHUB_ACTIONS` environment variable is set to `true`, `GitHubActions` will be used.
3. If the `TERM_PROGRAM` environment variable is set to `vscode`, `VisualStudioCode` will be used.
4. Use `Client`.

Syntax:

```yaml
output:
  style: string
```

Default:

```yaml
output:
  style: Detect
```

This option can be specified using:

```powershell
# PowerShell: Using the OutputStyle parameter
$option = New-PSRuleOption -OutputStyle AzurePipelines;
```

```powershell
# PowerShell: Using the Output.Style hashtable key
$option = New-PSRuleOption -Option @{ 'Output.Style' = 'AzurePipelines' };
```

```powershell
# PowerShell: Using the OutputStyle parameter to set YAML
Set-PSRuleOption -OutputFormat AzurePipelines;
```

```yaml
# YAML: Using the output/style property
output:
  style: AzurePipelines
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_STYLE=AzurePipelines
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_STYLE: AzurePipelines
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_STYLE
  value: AzurePipelines
```

### Output.JobSummaryPath

:octicons-milestone-24: v2.6.0

Configures the file path a job summary will be written to when using `Assert-PSRule`.
A job summary is a markdown file that summarizes the results of a job.
When not specified, a job summary will not be generated.

Syntax:

```yaml
output:
  jobSummaryPath: string
```

Default:

```yaml
output:
  jobSummaryPath: null
```

This option can be specified using:

```powershell
# PowerShell: Using the OutputJobSummaryPath parameter
$option = New-PSRuleOption -OutputJobSummaryPath 'reports/summary.md';
```

```powershell
# PowerShell: Using the Output.JobSummaryPath hashtable key
$option = New-PSRuleOption -Option @{ 'Output.JobSummaryPath' = 'reports/summary.md' };
```

```powershell
# PowerShell: Using the OutputJobSummaryPath parameter to set YAML
Set-PSRuleOption -OutputJobSummaryPath 'reports/summary.md';
```

```yaml
# YAML: Using the output/jobSummaryPath property
output:
  jobSummaryPath: 'reports/summary.md'
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_JOBSUMMARYPATH='reports/summary.md'
```

```powershell
# PowerShell: Using environment variable
$env:PSRULE_OUTPUT_JOBSUMMARYPATH = 'reports/summary.md';
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_JOBSUMMARYPATH: reports/summary.md
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_JOBSUMMARYPATH
  value: reports/summary.md
```

### Output.JsonIndent

Configures the number of spaces to indent JSON properties and elements.
The default number of spaces is 0.

This option applies to output generated from `-OutputFormat Json` for `Get-PSRule` and `Invoke-PSRule`.
This option also applies to output generated from `-OutputPath` for `Assert-PSRule`.

The range of indentation accepts a minimum of 0 (machine first) spaces and a maximum of 4 spaces.

This option can be specified using:

```powershell
# PowerShell: Using the OutputJsonIndent parameter
$option = New-PSRuleOption -OutputJsonIndent 2;
```

```powershell
# PowerShell: Using the Output.JsonIndent hashtable key
$option = New-PSRuleOption -Option @{ 'Output.JsonIndent' = 2 };
```

```powershell
# PowerShell: Using the OutputJsonIndent parameter to set YAML
Set-PSRuleOption -OutputJsonIndent 2;
```

```yaml
# YAML: Using the output/jsonIndent property
output:
  jsonIndent: 2
```

```bash
# Bash: Using environment variable
export PSRULE_OUTPUT_JSONINDENT=2
```

```powershell
# PowerShell: Using environment variable
$env:PSRULE_OUTPUT_JSONINDENT = 2;
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_OUTPUT_JSONINDENT: 2
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_OUTPUT_JSONINDENT
  value: 2
```

### Override.Level

This option is used to override the severity level of one or more rules.
When specified, the severity level of the rule will be set to the value specified.
Use this option to change the severity level of a rule to be different then orignally defined by the author.

The following severity levels are available:

- `Error` - A serious problem that must be addressed before going forward.
- `Warning` - A problem that should be addressed.
- `Information` - A minor problem or an opportunity to improve the code.

This option can be specified using:

```powershell
# PowerShell: Using the OverrideLevel parameter
$option = New-PSRuleOption -OverrideLevel @{ 'rule1' = 'Information' };
```

```powershell
# PowerShell: Using the OVerride.Level hashtable key
$option = New-PSRuleOption -Option @{ 'Override.Level.rule1' = 'Information' };
```

```powershell
# PowerShell: Using the OverrideLevel parameter to set YAML
Set-PSRuleOption -OverrideLevel @{ 'rule1' = 'Information' };
```

```yaml
# YAML: Using the override/level property
override:
  level:
    rule1: Information
```

```bash
# Bash: Using environment variable
export PSRULE_OVERRIDE_LEVEL_RULE1='Information'
```

```powershell
# PowerShell: Using environment variable
$env:PSRULE_OVERRIDE_LEVEL_RULE1 = 'Information';
```

### Repository.BaseRef

This option is used for specify the base branch for pull requests.
When evaluating changes files only PSRule uses this option for comparison with the current branch.
By default, the base ref is detected from environment variables set by the build system.

This option can be specified using:

```powershell
# PowerShell: Using the RepositoryBaseRef parameter
$option = New-PSRuleOption -RepositoryBaseRef 'main';
```

```powershell
# PowerShell: Using the Repository.BaseRef hashtable key
$option = New-PSRuleOption -Option @{ 'Repository.BaseRef' = 'main' };
```

```powershell
# PowerShell: Using the RepositoryBaseRef parameter to set YAML
Set-PSRuleOption -RepositoryBaseRef 'main';
```

```yaml
# YAML: Using the repository/baseRef property
repository:
  baseRef: main
```

```bash
# Bash: Using environment variable
export PSRULE_REPOSITORY_BASEREF='main'
```

```powershell
# PowerShell: Using environment variable
$env:PSRULE_REPOSITORY_BASEREF = 'main';
```

### Repository.Url

This option can be configured to set the repository URL reported in output.
By default, the repository URL is detected from environment variables set by the build system.

- In GitHub Actions, the repository URL is detected from the `GITHUB_REPOSITORY` environment variable.
- In Azure Pipelines, the repository URL is detected from the `BUILD_REPOSITORY_URI` environment variable.

This option can be specified using:

```powershell
# PowerShell: Using the RepositoryUrl parameter
$option = New-PSRuleOption -RepositoryUrl 'https://github.com/microsoft/PSRule';
```

```powershell
# PowerShell: Using the Repository.Url hashtable key
$option = New-PSRuleOption -Option @{ 'Repository.Url' = 'https://github.com/microsoft/PSRule' };
```

```powershell
# PowerShell: Using the RepositoryUrl parameter to set YAML
Set-PSRuleOption -RepositoryUrl 'https://github.com/microsoft/PSRule';
```

```yaml
# YAML: Using the repository/url property
repository:
  url: 'https://github.com/microsoft/PSRule'
```

```bash
# Bash: Using environment variable
export PSRULE_REPOSITORY_URL='https://github.com/microsoft/PSRule'
```

```powershell
# PowerShell: Using environment variable
$env:PSRULE_REPOSITORY_URL = 'https://github.com/microsoft/PSRule';
```

### Requires

Specifies module version constraints for running PSRule.
When set PSRule will error if a module version is used that does not satisfy the requirements.
The format for version constraints are the same as the `Version` assertion method.
See [about_PSRule_Assert] for more information.

Module version constraints a not enforced prior to PSRule v0.19.0.

The version constraint for a rule module is enforced when the module is included with `-Module`.
A version constraint does not require a rule module to be included.
Use the `Include.Module` option to automatically include a rule module.

This option can be specified using:

```powershell
# PowerShell: Using the Requires.module hashtable key
$option = New-PSRuleOption -Option @{ 'Requires.PSRule' = '>=1.0.0' };
```

```yaml
# YAML: Using the requires property
requires:
  PSRule: '>=1.0.0'                 # Require v1.0.0 or greater.
  PSRule.Rules.Azure: '>=1.0.0'     # Require v1.0.0 or greater.
  PSRule.Rules.CAF: '@pre >=0.1.0'  # Require stable or pre-releases v0.1.0 or greater.
```

This option can be configured using environment variables.
To specify a module version constraint, prefix the module name with `PSRULE_REQUIRES_`.
When the module name includes a dot (`.`) use an underscore (`_`) instead.

```bash
# Bash: Using environment variable
export PSRULE_REQUIRES_PSRULE='>=1.0.0'
export PSRULE_REQUIRES_PSRULE_RULES_AZURE='>=1.0.0'
export PSRULE_REQUIRES_PSRULE_RULES_CAF='@pre >=0.1.0'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_REQUIRES_PSRULE: '>=1.0.0'
  PSRULE_REQUIRES_PSRULE_RULES_AZURE: '>=1.0.0'
  PSRULE_REQUIRES_PSRULE_RULES_CAF: '@pre >=0.1.0'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_REQUIRES_PSRULE
  value: '>=1.0.0'
- name: PSRULE_REQUIRES_PSRULE_RULES_AZURE
  value: '>=1.0.0'
- name: PSRULE_REQUIRES_PSRULE_RULES_CAF
  value: '@pre >=0.1.0'
```

### Rule.Baseline

The name of a default baseline to use for the module.
Currently this option can only be set within a module configuration resource.

For example:

```yaml
---
# Synopsis: Example module configuration for Enterprise.Rules module.
apiVersion: github.com/microsoft/PSRule/v1
kind: ModuleConfig
metadata:
  name: Enterprise.Rules
spec:
  rule:
    baseline: Enterprise.Baseline1
```

### Rule.Include

The name of specific rules to evaluate.
If this option is not specified all rules in search paths will be evaluated.

This option can be overridden at runtime by using the `-Name` cmdlet parameter.

This option can be specified using:

```powershell
# PowerShell: Using the Rule.Include hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.Include' = 'Rule1','Rule2' };
```

```yaml
# YAML: Using the rule/include property
rule:
  include:
  - Rule1
  - Rule2
```

```bash
# Bash: Using environment variable
export PSRULE_RULE_INCLUDE='Rule1;Rule2'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_RULE_INCLUDE: 'Rule1;Rule2'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_RULE_INCLUDE
  value: 'Rule1;Rule2'
```

### Rule.IncludeLocal

Automatically include all local rules in the search path unless they have been explicitly excluded.
This option will include local rules even when they do not match `Rule.Include` or `Rule.Tag` filters.
By default, local rules will be filtered with `Rule.Include` and `Rule.Tag` filters.

This option is useful when you want to include local rules not included in a baseline.

This option can be specified using:

```powershell
# PowerShell: Using the RuleIncludeLocal parameter
$option = New-PSRuleOption -RuleIncludeLocal $True;
```

```powershell
# PowerShell: Using the Rule.IncludeLocal hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.IncludeLocal' = $True };
```

```powershell
# PowerShell: Using the RuleIncludeLocal parameter to set YAML
Set-PSRuleOption -RuleIncludeLocal $True;
```

```yaml
# YAML: Using the rule/includeLocal property
rule:
  includeLocal: true
```

```bash
# Bash: Using environment variable
export PSRULE_RULE_INCLUDELOCAL=true
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_RULE_INCLUDELOCAL: true
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_RULE_INCLUDELOCAL
  value: true
```

### Rule.Exclude

The name of specific rules to exclude from being evaluated.
This will exclude rules specified by `Rule.Include` or discovered from a search path.

This option can be specified using:

```powershell
# PowerShell: Using the Rule.Exclude hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.Exclude' = 'Rule3','Rule4' };
```

```yaml
# YAML: Using the rule/exclude property
rule:
  exclude:
  - Rule3
  - Rule4
```

```bash
# Bash: Using environment variable
export PSRULE_RULE_EXCLUDE='Rule3;Rule4'
```

```yaml
# GitHub Actions: Using environment variable
env:
  PSRULE_RULE_EXCLUDE: 'Rule3;Rule4'
```

```yaml
# Azure Pipelines: Using environment variable
variables:
- name: PSRULE_RULE_EXCLUDE
  value: 'Rule3;Rule4'
```

### Rule.Tag

A set of required key value pairs (tags) that rules must have applied to them to be included.

Multiple values can be specified for the same tag.
When multiple values are used, only one must match.

This option can be overridden at runtime by using the `-Tag` cmdlet parameter.

This option can be specified using:

```powershell
# PowerShell: Using the Rule.Tag hashtable key
$option = New-PSRuleOption -Option @{ 'Rule.Tag' = @{ severity = 'Critical','Warning' } };
```

```yaml
# YAML: Using the rule/tag property
rule:
  tag:
    severity: Critical
```

```yaml
# YAML: Using the rule/tag property, with multiple values
rule:
  tag:
    severity:
    - Critical
    - Warning
```

In the example above, rules must have a tag of `severity` set to either `Critical` or `Warning` to be included.

### Suppression

In certain circumstances it may be necessary to exclude or suppress rules from processing objects that are in a known failed state.

PSRule allows objects to be suppressed for a rule by TargetName.
Objects that are suppressed are not processed by the rule at all but will continue to be processed by other rules.

Rule suppression complements pre-filtering and pre-conditions.

This option can be specified using:

```powershell
# PowerShell: Using the SuppressTargetName option with a hashtable
$option = New-PSRuleOption -SuppressTargetName @{ 'storageAccounts.UseHttps' = 'TestObject1', 'TestObject3' };
```

```yaml
# YAML: Using the suppression property
suppression:
  storageAccounts.UseHttps:
    targetName:
    - TestObject1
    - TestObject3
```

In both of the above examples, `TestObject1` and `TestObject3` have been suppressed from being processed by a rule named `storageAccounts.UseHttps`.

When **to** use rule suppression:

- A temporary exclusion for an object that is in a known failed state.

When **not** to use rule suppression:

- An object should never be processed by any rule. Pre-filter the pipeline instead.
- The rule is not applicable because the object is the wrong type. Use pre-conditions on the rule instead.

An example of pre-filtering:

```powershell
# Define objects to validate
$items = @();
$items += [PSCustomObject]@{ Name = 'Fridge'; Type = 'Equipment'; Category = 'White goods'; };
$items += [PSCustomObject]@{ Name = 'Apple'; Type = 'Food'; Category = 'Produce'; };
$items += [PSCustomObject]@{ Name = 'Carrot'; Type = 'Food'; Category = 'Produce'; };

# Example of pre-filtering, only food items are sent to Invoke-PSRule
$items | Where-Object { $_.Type -eq 'Food' } | Invoke-PSRule;
```

An example of pre-conditions:

```powershell
# A rule with a pre-condition to only process produce
Rule 'isFruit' -If { $TargetObject.Category -eq 'Produce' } {
    # Condition to determine if the object is fruit
    $TargetObject.Name -in 'Apple', 'Orange', 'Pear'
}
```

## EXAMPLES

### Example ps-rule.yaml

```yaml
#
# PSRule example configuration
#

# Configures the repository
repository:
  url: https://github.com/microsoft/PSRule
  baseRef: main

# Configure required module versions
requires:
  PSRule.Rules.Azure: '>=1.1.0'

# Configure convention options
convention:
  include:
  - 'Convention1'

# Configure execution options
execution:
  hashAlgorithm: SHA256
  duplicateResourceId: Warn
  languageMode: ConstrainedLanguage
  suppressionGroupExpired: Error
  restrictScriptSource: ModuleOnly

# Configure include options
include:
  module:
  - 'PSRule.Rules.Azure'
  path: [ ]

# Configures input options
input:
  format: Yaml
  ignoreGitPath: false
  ignoreObjectSource: true
  ignoreRepositoryCommon: false
  ignoreUnchangedPath: true
  objectPath: items
  pathIgnore:
  - '*.Designer.cs'
  targetType:
  - Microsoft.Compute/virtualMachines
  - Microsoft.Network/virtualNetworks

# Configures outcome logging options
logging:
  limitDebug:
  - Rule1
  - Rule2
  limitVerbose:
  - Rule1
  - Rule2
  ruleFail: Error
  rulePass: Information

output:
  as: Summary
  banner: Minimal
  culture:
  - en-US
  encoding: UTF8
  footer: RuleCount
  format: Json
  jobSummaryPath: reports/summary.md
  outcome: Fail
  sarifProblemsOnly: false
  style: GitHubActions

# Overrides the severity level for rules
override:
  level:
    Rule1: Error
    Rule2: Warning

# Configure rule suppression
suppression:
  storageAccounts.UseHttps:
    targetName:
    - TestObject1
    - TestObject3

# Configure baseline options
binding:
  field:
    id:
    - ResourceId
    - AlternativeId
  ignoreCase: false
  nameSeparator: '::'
  preferTargetInfo: true
  targetName:
  - ResourceName
  - AlternateName
  targetType:
  - ResourceType
  - kind
  useQualifiedName: true

configuration:
  appServiceMinInstanceCount: 2

rule:
  include:
  - rule1
  - rule2
  includeLocal: true
  exclude:
  - rule3
  - rule4
  tag:
    severity:
    - Critical
    - Warning
```

### Default ps-rule.yaml

```yaml
#
# PSRule defaults
#

# Note: Only properties that differ from the default values need to be specified.

# Configure required module versions
requires: { }

# Configure convention options
convention:
  include: [ ]

# Configure execution options
execution:
  hashAlgorithm: SHA512
  aliasReference: Warn
  duplicateResourceId: Error
  invariantCulture: Warn
  languageMode: FullLanguage
  initialSessionState: BuiltIn
  restrictScriptSource: Unrestricted
  ruleInconclusive: Warn
  ruleSuppressed: Warn
  suppressionGroupExpired: Warn
  unprocessedObject: Warn

# Configure include options
include:
  module: [ ]
  path:
  - '.ps-rule/'

# Configures input options
input:
  format: Detect
  ignoreGitPath: true
  ignoreObjectSource: false
  ignoreRepositoryCommon: true
  ignoreUnchangedPath: false
  objectPath: null
  pathIgnore: [ ]
  targetType: [ ]

# Configures outcome logging options
logging:
  limitDebug: [ ]
  limitVerbose: [ ]
  ruleFail: None
  rulePass: None

output:
  as: Detail
  banner: Default
  culture: [ ]
  encoding: Default
  footer: Default
  format: None
  jobSummaryPath: null
  outcome: Processed
  sarifProblemsOnly: true
  style: Detect

override:
  level: { }

# Configure rule suppression
suppression: { }

# Configure baseline options
binding:
  field: { }
  ignoreCase: true
  nameSeparator: '/'
  preferTargetInfo: false
  targetName:
  - TargetName
  - Name
  targetType:
  - PSObject.TypeNames[0]
  useQualifiedName: false

configuration: { }

rule:
  include: [ ]
  includeLocal: false
  exclude: [ ]
  tag: { }
```

## NOTE

An online version of this document is available at <https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Options/>.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Invoke-PSRule/)
- [New-PSRuleOption](https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/New-PSRuleOption/)
- [Set-PSRuleOption](https://microsoft.github.io/PSRule/v2/commands/PSRule/en-US/Set-PSRuleOption/)

## KEYWORDS

- Options
- PSRule
- TargetInfo
- Binding

[about_PSRule_Assert]: about_PSRule_Assert.md
