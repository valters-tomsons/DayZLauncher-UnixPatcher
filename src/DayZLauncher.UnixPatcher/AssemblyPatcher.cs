using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DayZLauncher.UnixPatcher;

public static class AssemblyPatcher
{
    public static AssemblyDefinition PatchUtilsAssembly(string sourcePath, string payloadPath)
    {
        using var utilsPatchDef = AssemblyDefinition.ReadAssembly(payloadPath);
        var unixJunctionsTypeDef = utilsPatchDef.MainModule.GetType("DayZLauncher.UnixPatcher.Utils.UnixJunctions");

        using var targetDef = AssemblyDefinition.ReadAssembly(sourcePath);
        var unixJunctionsImport = targetDef.MainModule.ImportReference(unixJunctionsTypeDef);

        var targetJunctionsTypeDef = targetDef.MainModule.GetType("Utils.IO.Junctions");

        PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "Create", new List<OpCode> { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2 });
        PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "Delete", new List<OpCode> { OpCodes.Ldarg_0 });
        PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "Exists", new List<OpCode> { OpCodes.Ldarg_0 });
        PatchJunctionsMethod(unixJunctionsTypeDef, targetDef, targetJunctionsTypeDef, "GetTarget", new List<OpCode> { OpCodes.Ldarg_0 });

        return targetDef;
    }

    private static void PatchJunctionsMethod(TypeDefinition unixJunctionsType, AssemblyDefinition targetDefinition, TypeDefinition junctionsClass, string methodName, List<OpCode> args)
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