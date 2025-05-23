// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using PSRule.CommandLine;
using PSRule.CommandLine.Commands;
using PSRule.CommandLine.Models;
using PSRule.EditorServices.Commands;
using PSRule.EditorServices.Models;
using PSRule.EditorServices.Resources;
using PSRule.Pipeline;

namespace PSRule.EditorServices;

/// <summary>
/// A helper to build the command-line commands and options offered to the caller.
/// </summary>
internal sealed class ClientBuilder
{
    private const string ARG_FORCE = "--force";

    private static readonly string? _Version = typeof(PipelineBuilder).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    private readonly Option<string> _Global_Option;
    private readonly Option<bool> _Global_Verbose;
    private readonly Option<bool> _Global_Debug;
    private readonly Option<bool> _Module_Restore_Force;
    private readonly Option<bool> _Module_Init_Force;
    private readonly Option<string> _Module_Add_Version;
    private readonly Option<bool> _Module_Add_Force;
    private readonly Option<bool> _Module_Add_SkipVerification;
    private readonly Option<string[]> _Global_Path;
    private readonly Option<string> _Run_OutputPath;
    private readonly Option<string> _Run_OutputFormat;
    private readonly Option<string[]> _Run_InputPath;
    private readonly Option<string[]> _Run_Module;
    private readonly Option<string> _Run_Baseline;
    private readonly Option<string[]> _Run_Formats;
    private readonly Option<string[]> _Run_Outcome;
    private readonly Option<bool> _Run_NoRestore;
    private readonly Option<bool> _Listen_Stdio;
    private readonly Option<string> _Listen_Pipe;

    private ClientBuilder(RootCommand cmd)
    {
        Command = cmd;

        // Global options.
        _Global_Option = new Option<string>(
            ["--option"],
            getDefaultValue: () => "ps-rule.yaml",
            description: CmdStrings.Global_Option_Description
        );
        _Global_Verbose = new Option<bool>(
            ["--verbose"],
            description: CmdStrings.Global_Verbose_Description
        );
        _Global_Debug = new Option<bool>(
            ["--debug"],
            description: CmdStrings.Global_Debug_Description
        );
        _Global_Path = new Option<string[]>(
            ["-p", "--path"],
            description: CmdStrings.Global_Path_Description
        );

        // Options for the run command.
        _Run_OutputPath = new Option<string>(
            ["--output-path"],
            description: CmdStrings.Run_OutputPath_Description
        );
        _Run_OutputFormat = new Option<string>(
            ["-o", "--output"],
            description: CmdStrings.Run_OutputFormat_Description
        ).FromAmong("Yaml", "Json", "Markdown", "NUnit3", "Csv", "Sarif");
        _Run_InputPath = new Option<string[]>(
            ["-f", "--input-path"],
            description: CmdStrings.Run_InputPath_Description
        );
        _Run_Module = new Option<string[]>(
            ["-m", "--module"],
            description: CmdStrings.Run_Module_Description
        );
        _Run_Baseline = new Option<string>(
            ["--baseline"],
            description: CmdStrings.Run_Baseline_Description
        );
        _Run_Formats = new Option<string[]>(
            ["--formats"],
            description: CmdStrings.Run_Formats_Description
        );
        _Run_Formats.AllowMultipleArgumentsPerToken = true;
        _Run_Outcome = new Option<string[]>(
            ["--outcome"],
            description: CmdStrings.Run_Outcome_Description
        ).FromAmong("Pass", "Fail", "Error", "Processed", "Problem");
        _Run_Outcome.Arity = ArgumentArity.ZeroOrMore;
        _Run_NoRestore = new Option<bool>(
            "--no-restore",
            description: CmdStrings.Run_NoRestore_Description
        );

        // Options for the module command.
        _Module_Init_Force = new Option<bool>(
            [ARG_FORCE],
            description: CmdStrings.Module_Init_Force_Description
        );
        _Module_Add_Version = new Option<string>
        (
            ["--version"],
            description: CmdStrings.Module_Add_Version_Description
        );
        _Module_Add_Force = new Option<bool>(
            [ARG_FORCE],
            description: CmdStrings.Module_Add_Force_Description
        );
        _Module_Add_SkipVerification = new Option<bool>(
            ["--skip-verification"],
            description: CmdStrings.Module_Add_SkipVerification_Description
        );
        _Module_Restore_Force = new Option<bool>(
            [ARG_FORCE],
            description: CmdStrings.Module_Restore_Force_Description
        );

        // Options for the listen command.
        _Listen_Stdio = new Option<bool>(
            ["--stdio"],
            description: CmdStrings.Listen_Stdio_Description
        );

        _Listen_Pipe = new Option<string>(
            ["--pipe"],
            description: CmdStrings.Listen_Pipe_Description
        );

        cmd.AddGlobalOption(_Global_Option);
        cmd.AddGlobalOption(_Global_Verbose);
        cmd.AddGlobalOption(_Global_Debug);
    }

    public RootCommand Command { get; }

    public static Command New()
    {
        var cmd = new RootCommand(string.Concat(CmdStrings.Cmd_Description, " v", _Version))
        {
            Name = "ps-rule"
        };

        var builder = new ClientBuilder(cmd);
        builder.AddRun();
        builder.AddModule();
        builder.AddListen();
        return builder.Command;
    }

    /// <summary>
    /// Add the <c>run</c> command.
    /// </summary>
    private void AddRun()
    {
        var cmd = new Command("run", CmdStrings.Run_Description);
        cmd.AddOption(_Global_Path);
        cmd.AddOption(_Run_OutputPath);
        cmd.AddOption(_Run_OutputFormat);
        cmd.AddOption(_Run_InputPath);
        cmd.AddOption(_Run_Module);
        cmd.AddOption(_Run_Baseline);
        cmd.AddOption(_Run_Formats);
        cmd.AddOption(_Run_Outcome);
        cmd.AddOption(_Run_NoRestore);
        cmd.SetHandler(async (invocation) =>
        {
            var option = new RunOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                InputPath = invocation.ParseResult.GetValueForOption(_Run_InputPath),
                Module = invocation.ParseResult.GetValueForOption(_Run_Module),
                Baseline = invocation.ParseResult.GetValueForOption(_Run_Baseline),
                Formats = invocation.ParseResult.GetValueForOption(_Run_Formats),
                Outcome = invocation.ParseResult.GetValueForOption(_Run_Outcome).ToRuleOutcome(),
                OutputPath = invocation.ParseResult.GetValueForOption(_Run_OutputPath),
                OutputFormat = invocation.ParseResult.GetValueForOption(_Run_OutputFormat).ToOutputFormat(),
                NoRestore = invocation.ParseResult.GetValueForOption(_Run_NoRestore),
            };
            var client = GetClientContext(invocation);

            Environment.TryPathEnvironmentVariable("PSModulePath", out var searchPaths);
            var sp = searchPaths == null ? string.Empty : string.Join(',', searchPaths);

            if (client.Verbose)
            {
                invocation.Console.WriteLine($"VERBOSE: Using workspace: {Environment.GetWorkingPath()}");
                invocation.Console.WriteLine($"VERBOSE: Using module search path: {sp}");
                invocation.Console.WriteLine($"VERBOSE: Using language server: {client.Path}");
                invocation.Console.WriteLine($"VERBOSE: Using cache path: {client.CachePath}");
            }

            var result = await RunCommand.RunAsync(option, client);
            invocation.ExitCode = result.ExitCode;
        });
        Command.AddCommand(cmd);
    }

    /// <summary>
    /// Add the <c>module</c> command.
    /// </summary>
    private void AddModule()
    {
        var cmd = new Command("module", CmdStrings.Module_Description);

        var moduleArg = new Argument<string[]>
        (
            "module",
            CmdStrings.Module_Module_Description
        );
        moduleArg.Arity = ArgumentArity.OneOrMore;

        // Init
        var init = new Command
        (
            "init",
            CmdStrings.Module_Init_Description
        );
        init.AddOption(_Module_Init_Force);
        init.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Version = invocation.ParseResult.GetValueForOption(_Module_Add_Version),
                Force = invocation.ParseResult.GetValueForOption(_Module_Init_Force),
                SkipVerification = invocation.ParseResult.GetValueForOption(_Module_Add_SkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleInitAsync(option, client);
        });

        // List
        var list = new Command
        (
            "list",
            CmdStrings.Module_List_Description
        );
        list.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Version = invocation.ParseResult.GetValueForOption(_Module_Add_Version),
                Force = invocation.ParseResult.GetValueForOption(_Module_Add_Force),
                SkipVerification = invocation.ParseResult.GetValueForOption(_Module_Add_SkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleListAsync(option, client);
        });

        // Add
        var add = new Command
        (
            "add",
            CmdStrings.Module_Add_Description
        );
        add.AddArgument(moduleArg);
        add.AddOption(_Module_Add_Version);
        add.AddOption(_Module_Add_Force);
        add.AddOption(_Module_Add_SkipVerification);
        add.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Module = invocation.ParseResult.GetValueForArgument(moduleArg),
                Version = invocation.ParseResult.GetValueForOption(_Module_Add_Version),
                Force = invocation.ParseResult.GetValueForOption(_Module_Add_Force),
                SkipVerification = invocation.ParseResult.GetValueForOption(_Module_Add_SkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleAddAsync(option, client);
        });

        // Remove
        var remove = new Command
        (
            "remove",
            CmdStrings.Module_Remove_Description
        );
        remove.AddArgument(moduleArg);
        remove.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Module = invocation.ParseResult.GetValueForArgument(moduleArg),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleRemoveAsync(option, client);
        });

        // Upgrade
        var upgrade = new Command
        (
            "upgrade",
            CmdStrings.Module_Upgrade_Description
        );
        upgrade.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleUpgradeAsync(option, client);
        });

        // Restore
        var restore = new Command("restore", CmdStrings.Module_Restore_Description);
        // restore.AddOption(_Path);
        restore.AddOption(_Module_Restore_Force);
        restore.SetHandler(async (invocation) =>
        {
            var option = new RestoreOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Force = invocation.ParseResult.GetValueForOption(_Module_Restore_Force),
            };
            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleRestoreAsync(option, client);
        });

        cmd.AddCommand(init);
        cmd.AddCommand(list);
        cmd.AddCommand(add);
        cmd.AddCommand(remove);
        cmd.AddCommand(upgrade);
        cmd.AddCommand(restore);

        cmd.AddOption(_Global_Path);
        Command.AddCommand(cmd);
    }

    private void AddListen()
    {
        var cmd = new Command("listen", CmdStrings.Listen_Description);
        cmd.AddOption(_Global_Path);
        cmd.AddOption(_Listen_Stdio);
        cmd.AddOption(_Listen_Pipe);
        cmd.SetHandler(async (invocation) =>
        {
            var option = new ListenOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Pipe = invocation.ParseResult.GetValueForOption(_Listen_Pipe),
                Stdio = invocation.ParseResult.GetValueForOption(_Listen_Stdio),
            };
            var client = GetClientContext(invocation);
            invocation.ExitCode = await ListenCommand.ListenAsync(option, client);
        });
        Command.AddCommand(cmd);
    }

    private ClientContext GetClientContext(InvocationContext invocation)
    {
        var option = invocation.ParseResult.GetValueForOption(_Global_Option).TrimQuotes();
        var verbose = invocation.ParseResult.GetValueForOption(_Global_Verbose);
        var debug = invocation.ParseResult.GetValueForOption(_Global_Debug);

        if (!string.IsNullOrEmpty(option))
        {
            option = Environment.GetRootedPath(option);
        }

        option ??= Path.Combine(Environment.GetWorkingPath(), "ps-rule.yaml");

        return new ClientContext
        (
            invocation: invocation,
            option: option,
            verbose: verbose,
            debug: debug
        );
    }
}
