# PSRule_Baseline

## about_PSRule_Baseline

## SHORT DESCRIPTION

Describes usage of baselines within PSRule.

## LONG DESCRIPTION

PSRule lets you define a baseline.
A baseline includes a set of rule and configuration options that are used for evaluating objects.

The following baseline options can be configured:

- [Binding.Field](about_PSRule_Options.md#bindingfield)
- [Binding.IgnoreCase](about_PSRule_Options.md#bindingignorecase)
- [Binding.NameSeparator](about_PSRule_Options.md#bindingnameseparator)
- [Binding.PreferTargetInfo](about_PSRule_Options.md#bindingprefertargetinfo)
- [Binding.TargetName](about_PSRule_Options.md#bindingtargetname)
- [Binding.TargetType](about_PSRule_Options.md#bindingtargettype)
- [Binding.UseQualifiedName](about_PSRule_Options.md#bindingusequalifiedname)
- [Configuration](about_PSRule_Options.md#configuration)
- [Override.Level](about_PSRule_Options.md#overridelevel)
- [Rule.Include](about_PSRule_Options.md#ruleinclude)
- [Rule.IncludeLocal](about_PSRule_Options.md#ruleincludelocal)
- [Rule.Exclude](about_PSRule_Options.md#ruleexclude)
- [Rule.Tag](about_PSRule_Options.md#ruletag)

Baseline options can be:

- Included as a baseline spec within a YAML or JSON file.
  - When using this method, multiple baseline specs can be defined within the same YAML/JSON file.
  - Each YAML baseline spec is separated using `---`.
  - Each JSON baseline spec is separated by JSON objects in a JSON array.
- Set within a workspace options file like `ps-rule.yaml` or `ps-rule.json`.
  - Only a single baseline can be specified.
  - See [about_PSRule_Options](about_PSRule_Options.md) for details on using this method.

### Baseline specs

YAML baseline specs are saved within a YAML file with a `.Rule.yaml` or `.Rule.yml` extension, for example `Baseline.Rule.yaml`.

JSON baseline specs are saved within a file with a `.Rule.json` or `.Rule.jsonc` extension, for example `Baseline.Rule.json`.
Use `.jsonc` to view [JSON with Comments](https://code.visualstudio.com/docs/languages/json#_json-with-comments) in Visual Studio Code.

To define a YAML baseline spec use the following structure:

```yaml
---
# Synopsis: <synopsis>
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: <name>
  annotations: { }
spec:
  # One or more baseline options
  binding: { }
  configuration: { }
  override: {}
  rule: { }
```

For example:

```yaml
---
# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: Baseline1
spec:
  binding:
    field:
      id:
      - ResourceId
    targetName:
    - Name
    - ResourceName
    - ResourceGroupName
    targetType:
    - ResourceType
  rule:
    include:
    - Rule1
    - Rule2
  configuration:
    allowedLocations:
    - 'Australia East'
    - 'Australia South East'

---
# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: Baseline2
spec:
  binding:
    targetName:
    - Name
    - ResourceName
    - ResourceGroupName
    targetType:
    - ResourceType
  rule:
    include:
    - Rule1
    - Rule3
  configuration:
    allowedLocations:
    - 'Australia East'
```

To define a JSON baseline spec use the following structure:

```json
[
  {
    // Synopsis: <synopsis>
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Baseline",
    "metadata": {
      "name": "<name>",
      "annotations": {}
    },
    "spec": {
      "binding": {},
      "configuration": {},
      "rule": {},
      "override": {}
    }
  }
]
```

For example:

```json
[
  {
    // Synopsis: This is an example baseline
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Baseline",
    "metadata": {
      "name": "Baseline1"
    },
    "spec": {
      "binding": {
        "field": {
          "id": [
            "ResourceId"
          ]
        },
        "targetName": [
          "Name",
          "ResourceName",
          "ResourceGroupName"
        ],
        "targetType": [
          "ResourceType"
        ]
      },
      "rule": {
        "include": [
          "Rule1",
          "Rule2"
        ]
      },
      "configuration": {
        "allowedLocations": [
          "Australia East",
          "Australia South East"
        ]
      }
    }
  },
  {
    // Synopsis: This is an example baseline
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Baseline",
    "metadata": {
      "name": "Baseline2"
    },
    "spec": {
      "binding": {
        "targetName": [
          "Name",
          "ResourceName",
          "ResourceGroupName"
        ],
        "targetType": [
          "ResourceType"
        ]
      },
      "rule": {
        "include": [
          "Rule1",
          "Rule3"
        ]
      },
      "configuration": {
        "allowedLocations": [
          "Australia East"
        ]
      }
    }
  }
]
```

### Baseline scopes

When baseline options are set, PSRule uses the following order to determine precedence.

1. Parameter - `-Name` and `-Tag`.
2. Explicit - A named baseline specified with `-Baseline`.
3. Workspace - Included in `ps-rule.yaml` or specified on the command line with `-Option`.
4. Module - A baseline object included in a `.Rule.yaml` or `.Rule.json` file.

After precedence is determined, baselines are merged and null values are ignored, such that:

### Annotations

Additional baseline annotations can be provided as key/ value pairs.
Annotations can be used to provide additional information that is available in `Get-PSRuleBaseline` output.

The following reserved annotation exists:

- `obsolete` - Marks the baseline as obsolete when set to `true`.
PSRule will generate a warning when an obsolete baseline is used.

YAML example:

```yaml
---
# Synopsis: This is an example baseline that is obsolete
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: ObsoleteBaseline
  annotations:
    obsolete: true
spec: { }
```

JSON example:

```json
[
  {
    // Synopsis: This is an example baseline that is obsolete
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Baseline",
    "metadata": {
      "name": "ObsoleteBaseline",
      "annotations": {
        "obsolete": true
      }
    },
    "spec": {}
  }
]
```

## EXAMPLES

### Example Baseline.Rule.yaml

```yaml
# Example Baseline.Rule.yaml

---
# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline1
spec:
  binding:
    targetName:
    - AlternateName
    targetType:
    - kind
  rule:
    include:
    - 'WithBaseline'
  configuration:
    key1: value1

---
# Synopsis: This is an example baseline
apiVersion: github.com/microsoft/PSRule/v1
kind: Baseline
metadata:
  name: TestBaseline2
spec:
  binding:
    targetName:
    - AlternateName
    targetType:
    - kind
  rule:
    include:
    - 'WithBaseline'
  configuration:
    key1: value1
```

### Example Baseline.Rule.json

```json
// Example Baseline.Rule.json

[
  {
    // Synopsis: This is an example baseline
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Baseline",
    "metadata": {
      "name": "TestBaseline1"
    },
    "spec": {
      "binding": {
        "targetName": [
          "AlternateName"
        ],
        "targetType": [
          "kind"
        ]
      },
      "rule": {
        "include": [
          "WithBaseline"
        ]
      },
      "configuration": {
        "key1": "value1"
      }
    }
  },
  {
    // Synopsis: This is an example baseline
    "apiVersion": "github.com/microsoft/PSRule/v1",
    "kind": "Baseline",
    "metadata": {
      "name": "TestBaseline2"
    },
    "spec": {
      "binding": {
        "targetName": [
          "AlternateName"
        ],
        "targetType": [
          "kind"
        ]
      },
      "rule": {
        "include": [
          "WithBaseline"
        ]
      },
      "configuration": {
        "key1": "value1"
      }
    }
  }
]
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/v2/concepts/PSRule/en-US/about_PSRule_Baseline/.

## KEYWORDS

- Options
- PSRule
- Baseline
- Binding
