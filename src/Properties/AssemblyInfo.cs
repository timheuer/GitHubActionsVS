﻿using GitHubActionsVS;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(Vsix.Name)]
[assembly: AssemblyDescription(Vsix.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Vsix.Author)]
[assembly: AssemblyProduct(Vsix.Name)]
[assembly: AssemblyCopyright(Vsix.Author)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\LibGit2Sharp.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Sodium.Core.dll")]

namespace System.Runtime.CompilerServices;
public class IsExternalInit { }