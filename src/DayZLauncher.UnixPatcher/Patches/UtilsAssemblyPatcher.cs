using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DayZLauncher.UnixPatcher.Patches;

public static class UtilsAssemblyPatcher
{
    private static readonly Dictionary<string, OpCode[]> FunctionsArguments = new() {
        {"Create", new OpCode[] {OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2}},
        {"Delete", new OpCode[] {OpCodes.Ldarg_0}},
        {"Exists", new OpCode[] {OpCodes.Ldarg_0}},
        {"GetTarget", new OpCode[] {OpCodes.Ldarg_0}},
    };

    public static AssemblyDefinition PatchAssembly(string sourcePath, string payloadPath)
    {
        using var utilsPatchDef = AssemblyDefinition.ReadAssembly(payloadPath);
        var unixJunctionsTypeDef = utilsPatchDef.MainModule.GetType("DayZLauncher.UnixPatcher.Utils.UnixJunctions");

        var targetDef = AssemblyDefinition.ReadAssembly(sourcePath);
        var unixJunctionsImport = targetDef.MainModule.ImportReference(unixJunctionsTypeDef);
        var targetJunctionsTypeDef = targetDef.MainModule.GetType("Utils.IO.Junctions");

        foreach (var def in FunctionsArguments)
        {
            PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, def.Key, def.Value);
        }

        return targetDef;
    }

    private static void PatchJunctionsMethod(TypeDefinition unixJunctionsType, AssemblyDefinition targetDefinition, TypeDefinition junctionsClass, string methodName, IEnumerable<OpCode> args)
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
}