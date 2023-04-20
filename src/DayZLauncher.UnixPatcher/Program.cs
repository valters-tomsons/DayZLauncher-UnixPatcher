using Mono.Cecil;
using Mono.Cecil.Cil;

var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory + '/');

var userSelection = args[0].Trim();
if (string.IsNullOrWhiteSpace(userSelection))
{
    Console.WriteLine("Must provide path to DayZ installation folder as argument!");
    return;
}

var targetAssembly = $"{userSelection}/Launcher/Utils.dll";
if (!File.Exists(targetAssembly))
{
    Console.WriteLine("Could not find 'Launcher/Utils.dll' in target folder!");
    return;
}

var backupAssembly = MoveFileToBackup(targetAssembly);

var utilsPatchPath = baseDirectory + "/DayZLauncher.UnixPatcher.Utils.dll";
using var utilsPatchDef = AssemblyDefinition.ReadAssembly(utilsPatchPath);
var unixJunctionsTypeDef = utilsPatchDef.MainModule.GetType("DayZLauncher.UnixPatcher.Utils.UnixJunctions");

using var targetDef = AssemblyDefinition.ReadAssembly(backupAssembly);
var unixJunctionsImport = targetDef.MainModule.ImportReference(unixJunctionsTypeDef);

var targetJunctionsTypeDef = targetDef.MainModule.GetType("Utils.IO.Junctions");

PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "Create", new List<OpCode> { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2 });
PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "Delete", new List<OpCode> { OpCodes.Ldarg_0 });
PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "Exists", new List<OpCode> { OpCodes.Ldarg_0 });
PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "GetTarget", new List<OpCode> { OpCodes.Ldarg_0 });

Console.WriteLine("Writing patches to disk...");

try
{
    File.Copy(utilsPatchPath, args[0].Trim() + "/Launcher/DayZLauncher.UnixPatcher.Utils.dll", true);
    targetDef.Write(targetAssembly);
}
catch
{
    Console.WriteLine("Failed to write files into DayZ directory!");
    Console.WriteLine("Aborting...");
    File.Move(backupAssembly, targetAssembly);
}

Console.WriteLine("Patch applied!");

void PatchJunctionsMethod(TypeDefinition unixJunctionsType, AssemblyDefinition targetDefinition, TypeDefinition junctionsClass, string methodName, List<OpCode> args)
{
    var originalMethod = junctionsClass.Methods.FirstOrDefault(m => m.Name == methodName);
    var patchedMethod = unixJunctionsType.Methods.FirstOrDefault(m => m.Name == methodName);
    var importedPatchedMethod = targetDefinition.MainModule.ImportReference(patchedMethod);

    if (originalMethod is null)
    {
        Console.WriteLine("Failed to patch");
        return;
    }

    originalMethod.Body = new MethodBody(originalMethod);

    var il = originalMethod.Body.GetILProcessor();

    il.Emit(OpCodes.Nop);

    foreach (var op in args)
    {
        il.Emit(op);
    }

    il.Emit(OpCodes.Call, importedPatchedMethod);
    il.Emit(OpCodes.Ret);
}

string MoveFileToBackup(string filePath)
{
    var newPath = filePath + ".bak";

    if (File.Exists(newPath))
    {
        Console.WriteLine("Warning! Backup Utils.dll already exists, overwriting!");
    }

    File.Move(filePath, newPath, true);
    return newPath;
}