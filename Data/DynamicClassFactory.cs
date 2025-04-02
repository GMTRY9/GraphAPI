using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

public static class DynamicClassFactory2
{
    public static Type CreateType(Dictionary<string, Type> properties)
    {
        var typeBuilder = GetTypeBuilder();
        foreach (var property in properties)
        {
            CreateProperty(typeBuilder, property.Key, property.Value);
        }
        return typeBuilder.CreateType();
    }

    private static TypeBuilder GetTypeBuilder()
    {
        var typeSignature = "DynamicType";
        var an = new AssemblyName(typeSignature);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType(typeSignature,
            TypeAttributes.Public |
            TypeAttributes.Class |
            TypeAttributes.AutoClass |
            TypeAttributes.AnsiClass |
            TypeAttributes.BeforeFieldInit |
            TypeAttributes.AutoLayout,
            null);
        return typeBuilder;
    }

    private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
    {
        var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

        var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        var getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            propertyType, Type.EmptyTypes);

        var getIl = getPropMthdBldr.GetILGenerator();
        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        var setPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            null, new[] { propertyType });

        var setIl = setPropMthdBldr.GetILGenerator();
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);
        setIl.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getPropMthdBldr);
        propertyBuilder.SetSetMethod(setPropMthdBldr);
    }
}