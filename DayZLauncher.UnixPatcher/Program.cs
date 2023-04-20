using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DayZLauncher.UnixPatcher;

internal static class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1 || !Directory.Exists(args[0]))
        {
            Console.WriteLine("Must provide path to DayZ installation folder as argument!");
            Console.ReadKey();
            return;
        }

        var targetAssembly = @$"{args[0].Trim()}\Launcher\Utils.dll";
        if (!File.Exists(targetAssembly))
        {
            Console.WriteLine(@"Could not find 'Launcher\Utils.dll' in target folder!");
            Console.ReadKey();
            return;
        }

        var backupAssembly = targetAssembly + ".bak";

        if (File.Exists(backupAssembly))
        {
            File.Delete(backupAssembly);
        }

        File.Move(targetAssembly, targetAssembly + ".bak");

        var patchAssembly = "DayZLauncher.UnixPatcher.Utils.dll";

        using var patchDefinition = AssemblyDefinition.ReadAssembly(patchAssembly);
        var unixJunctionsType = patchDefinition.MainModule.GetType("DayZLauncher.UnixPatcher.Utils.UnixJunctions");

        using var targetDefinition = AssemblyDefinition.ReadAssembly(targetAssembly + ".bak");
        var importedUnixJunctionsType = targetDefinition.MainModule.ImportReference(unixJunctionsType);

        var junctionsClass = targetDefinition.MainModule.GetType("Utils.IO.Junctions");

        PatchJunctionsMethod(unixJunctionsType, targetDefinition, junctionsClass, "Create", new List<OpCode> { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2 });
        PatchJunctionsMethod(unixJunctionsType, targetDefinition, junctionsClass, "Delete", new List<OpCode> { OpCodes.Ldarg_0 });
        PatchJunctionsMethod(unixJunctionsType, targetDefinition, junctionsClass, "Exists", new List<OpCode> { OpCodes.Ldarg_0 });
        PatchJunctionsMethod(unixJunctionsType, targetDefinition, junctionsClass, "GetTarget", new List<OpCode> { OpCodes.Ldarg_0 });

        Console.WriteLine("Writing patches to disk...");

        targetDefinition.Write(targetAssembly);
        File.Copy("DayZLauncher.UnixPatcher.Utils.dll", @$"{args[0].Trim()}\Launcher\DayZLauncher.UnixPatcher.Utils.dll", true);

        Console.WriteLine("Patch applied!");
        Console.ReadKey();
    }

    private static void PatchJunctionsMethod(TypeDefinition unixJunctionsType, AssemblyDefinition targetDefinition, TypeDefinition junctionsClass, string methodName, List<OpCode> args)
    {
        var originalMethod = junctionsClass.Methods.FirstOrDefault(m => m.Name == methodName);
        var patchedMethod = unixJunctionsType.Methods.FirstOrDefault(m => m.Name == methodName);
        var importedPatchedMethod = targetDefinition.MainModule.ImportReference(patchedMethod);
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