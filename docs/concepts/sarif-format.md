# SARIF Output

PSRule uses a JSON structured output format called the

SARIF format to report results.
The SARIF format is a standard format for the output of static analysis tools.
The format is designed to be easily consumed by other tools and services.

## Runs

When running PSRule executed a run will be generated in `runs` containing details about PSRule and configuration details.

## Invocation

The `invocation` property reports runtime information about how the run started.

### RuleConfigurationOverrides

When a rule has been overridden in configuration this invocation property will contain any level overrides.
