using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// DXLog.net auto-injects these 'using' directives into every script.
// Including them explicitly causes a CS0105 ("duplicate using") error
// when the script is compiled inside DXLog.net.
//var forbiddenUsings = new[] {};*

// Locate the repository root by walking up until a .git folder is found.
var repoRoot = FindRepoRoot(AppContext.BaseDirectory);

var checkToolDir = Path.Combine(repoRoot, ".github", "scripts", "CheckScripts");

var scriptsDir = Path.Combine(repoRoot, "Scripts");

var csFiles = Directory.EnumerateFiles(scriptsDir, "*.cs", SearchOption.AllDirectories)
    .Where(f => !f.StartsWith(checkToolDir))
    .Where(f => !f.Split(Path.DirectorySeparatorChar).Contains("bin"))
    .Where(f => !f.Split(Path.DirectorySeparatorChar).Contains("obj"))
    .OrderBy(f => f)
    .ToList();

Console.WriteLine($"Checking {csFiles.Count} .cs file(s)...\n");

var hasErrors = false;
var classNames = new Dictionary<string, List<string>>(StringComparer.Ordinal);

foreach (var file in csFiles)
{
    var relPath = Path.GetRelativePath(repoRoot, file);
    var text = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(text, path: file);
    var root = tree.GetCompilationUnitRoot();

    // 1. Syntax errors
    var syntaxErrors = tree.GetDiagnostics()
        .Where(d => d.Severity == DiagnosticSeverity.Error)
        .ToList();

    foreach (var diag in syntaxErrors)
    {
        hasErrors = true;
        var pos = diag.Location.GetLineSpan().StartLinePosition;
        Console.WriteLine($"::error file={relPath},line={pos.Line + 1},col={pos.Character + 1}::Syntax error: {diag.GetMessage()}");
    }

    // 2. Forbidden 'using' directives (DXLog.net injects these automatically)
    //foreach (var u in root.Usings)
    //{
    //    var name = u.Name?.ToString();
    //    if (name != null && forbiddenUsings.Contains(name))
    //    {
    //        hasErrors = true;
    //        var pos = u.GetLocation().GetLineSpan().StartLinePosition;
    //        Console.WriteLine($"::error file={relPath},line={pos.Line + 1}::Explicit 'using {name};' is auto-injected by DXLog.net and will cause CS0105. Remove this line.");
    //    }
    //}

    // 3. Collect class names for duplicate check across the repo
    var classes = root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .Select(c => c.Identifier.Text);

    foreach (var className in classes)
    {
        if (!classNames.TryGetValue(className, out var files))
        {
            files = new List<string>();
            classNames[className] = files;
        }
        files.Add(relPath);
    }

    if (syntaxErrors.Count == 0)
        Console.WriteLine($"OK   {relPath}");
}

// 4. Duplicate class names (DXLog.net requires a unique class name per script file)
Console.WriteLine();
foreach (var (className, files) in classNames)
{
    if (files.Count > 1)
    {
        hasErrors = true;
        Console.WriteLine($"::error::Duplicate class name '{className}' found in: {string.Join(", ", files)}");
        Console.WriteLine("Each DXLog.net script file needs a unique class name.");
    }
}

Console.WriteLine();
if (hasErrors)
{
    Console.WriteLine("Check FAILED.");
    return 1;
}

Console.WriteLine("All checks passed.");
return 0;

static string FindRepoRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, ".git")))
        dir = dir.Parent;

    return dir?.FullName ?? startDir;
}
