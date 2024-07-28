# Options

Options are used to customize how rules are evaluated and the resulting output.
You can set options in multiple ways, including:

- Parameters
- Environment variables
- Configuration files

Rules or modules could also have a defaults configured by the rule or module author.

## Setting options

## Option precedence

When setting options, you may have a situation where an option is set to different values.
For example, you may set an option in a configuration file and also set the same option as a parameter.

When this happens, PSRule will use the option with the highest precedence.

Option precedence is as follows:

1. Parameters
2. Explicit baselines
3. Environment variables
4. Configuration files
5. Default baseline
6. Module defaults
