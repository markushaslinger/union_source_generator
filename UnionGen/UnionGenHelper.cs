﻿using System.Text;

namespace UnionGen;

internal readonly struct UnionGenHelper(UnionToGenerate union)
{
    private const string InteropNamespace = "System.Runtime.InteropServices";
    private const string PointerSizeGuardTypeName = $"{GenNamespace}.PointerSizeGuard";
    private const string PointerSizeGuardMethodName = "EnsureInitialized";
    private const string TypeLookupFunc = "GetActualTypeName";
    private const string IndexParameterName = "index";
    private const string ActualTypeIndexParameterName = "actualTypeIndex";
    private const string StateByteFieldName = "_state";
    private const string IndexPropertyName = $"{StateByteFieldName}.Index";
    private const string ActualTypeIndexPropertyName = $"{StateByteFieldName}.ActualTypeIndex";
    private const string ValueParameterName = "value";
    private const string ValueFieldNamePrefix = $"_{ValueParameterName}";
    private const string RefValueFieldName = $"_{ValueParameterName}Ref";
    private const string GenNamespace = "UnionGen";
    private const string StateByteTypeName = $"{GenNamespace}.StateByte";
    private const string RefTypeIndex = $"{StateByteTypeName}.RefTypeIndex";

    public string GeneratePartialStruct()
    {
        var (typeFields, typeProperties) = GenerateFieldsAndProperties();

        var code = $$"""
                     {{UnionSourceGen.AutoGeneratedComment}}
                     #nullable enable
                     namespace {{union.Namespace}}
                     {
                         [{{InteropNamespace}}.StructLayout({{InteropNamespace}}.LayoutKind.Explicit)]
                         public readonly partial struct {{union.Name}} : IEquatable<{{union.Name}}>
                         {
                     {{typeFields}}
                     {{GenerateConstructors()}}
                     {{typeProperties}}
                     {{GenerateAccessors()}}
                     {{GenerateImplicitOperators()}}
                     {{GenerateMatch()}}
                     {{GenerateSwitch()}}
                     {{GenerateToString()}}
                     {{GenerateEqualityMembers()}}
                     {{GenerateGetActualTypeName()}}
                         }
                     }
                     """;

        return code;
    }

    private string GenerateEqualityMembers()
    {
        var members = new StringBuilder();

        members.AppendLine(GenerateTypedEquals());
        members.AppendLine(GenerateObjectEquals());
        members.Append(GenerateGetHashCode());

        return members.ToString();
    }

    private string GenerateObjectEquals()
    {
        var equalsMethod
            = new IndentedStringBuilder(2, $"public override bool Equals(object? obj){IndentedStringBuilder.NewLine}");
        equalsMethod.AppendLine("{");
        equalsMethod.AppendLine("if (ReferenceEquals(null, obj))", 1);
        equalsMethod.AppendLine("{", 1);
        equalsMethod.AppendLine("return false;", 2);
        equalsMethod.AppendLine("}", 1);
        equalsMethod.AppendLine($"return obj is {union.Name} other && Equals(other);", 1);
        equalsMethod.AppendLine("}");

        return equalsMethod.ToString();
    }

    private string GenerateTypedEquals()
    {
        var method
            = new IndentedStringBuilder(2,
                                        $"public bool Equals({union.Name} other) => {IndentedStringBuilder.NewLine}");
        method.AppendLine($"{IndexPropertyName} == other.{IndexPropertyName}", 1);
        method.AppendLine($"&& {IndexPropertyName} switch ", 2);
        method.AppendLine("{", 2);

        var anyRefType = false;
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            if (type.IsReferenceType)
            {
                anyRefType = true;
            }
            else
            {
                method.AppendLine($"{i} => {ValueFieldNamePrefix}{i}.Equals(other.{ValueFieldNamePrefix}{i}),", 3);
            }
        }

        if (anyRefType)
        {
            method.AppendLine($"{RefTypeIndex} => {RefValueFieldName}?.Equals(other.{RefValueFieldName}) ?? false,", 3);
        }

        method.AppendLine("_ => false", 3);
        method.AppendLine("};", 2);

        return method.ToString();
    }

    private string GenerateSwitch()
    {
        var parameters = new StringBuilder();
        var cases = new IndentedStringBuilder(4);
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            var actionName = $"for{type.TitleCaseName}";
            parameters.Append($"Action<{type.FullName}> {actionName}");
            if (i < union.TypeParameters.Count - 1)
            {
                parameters.Append(", ");
            }

            cases.AppendLine(type.IsReferenceType
                                 ? $"case {i}: {actionName}(({type.FullName}){RefValueFieldName}!); break;"
                                 : $"case {i}: {actionName}({ValueFieldNamePrefix}{i}); break;");
        }

        cases.AppendLine($"default: throw new InvalidOperationException($\"Unknown type index {{{IndexPropertyName}}}\");");

        var switchMethod = new IndentedStringBuilder(2, "public void Switch(");
        switchMethod.Append(parameters.ToString());
        switchMethod.Append(")");
        switchMethod.AppendLine(string.Empty);
        switchMethod.AppendLine("{");
        switchMethod.AppendLine($"switch ({ActualTypeIndexPropertyName})", 1);
        switchMethod.AppendLine("{", 1);
        switchMethod.Append(cases.ToString());
        switchMethod.AppendLine("}", 1);
        switchMethod.AppendLine("}");

        return switchMethod.ToString();
    }

    private string GenerateMatch()
    {
        var parameters = new StringBuilder();
        var cases = new IndentedStringBuilder(4);
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            var funcName = $"with{type.TitleCaseName}";
            parameters.Append($"Func<{type.FullName}, TResult> {funcName}");
            if (i < union.TypeParameters.Count - 1)
            {
                parameters.Append(", ");
            }

            cases.AppendLine(type.IsReferenceType
                                 ? $"{i} => {funcName}(({type.FullName}){RefValueFieldName}!),"
                                 : $"{i} => {funcName}({ValueFieldNamePrefix}{i}),");
        }

        cases.AppendLine($"_ => throw new InvalidOperationException($\"Unknown type index {{{IndexPropertyName}}}\")");

        var matchMethod = new IndentedStringBuilder(2, $"public TResult Match<TResult>({parameters})");
        matchMethod.Append(" => ");
        matchMethod.AppendLine(string.Empty);
        matchMethod.AppendLine($"{ActualTypeIndexPropertyName} switch", 1);
        matchMethod.AppendLine("{", 1);
        matchMethod.Append(cases);
        matchMethod.AppendLine("};", 1);

        return matchMethod.ToString();
    }

    private string GenerateToString()
    {
        var cases = new IndentedStringBuilder(4);

        var anyRefType = false;
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            if (type.IsReferenceType)
            {
                anyRefType = true;
            }
            else
            {
                cases.AppendLine($"{i} => {ValueFieldNamePrefix}{i}{type.CallOperator}ToString()!,");
            }
        }

        if (anyRefType)
        {
            cases.AppendLine($"{RefTypeIndex} => {RefValueFieldName}?.ToString() ?? \"null\",");
        }

        cases.AppendLine($"_ => throw new InvalidOperationException($\"Unknown type index {{{IndexPropertyName}}}\")");

        var toStringMethod = new IndentedStringBuilder(2, "public override string ToString()");
        toStringMethod.Append(" => ");
        toStringMethod.AppendLine(string.Empty);
        toStringMethod.AppendLine($"{IndexPropertyName} switch", 1);
        toStringMethod.AppendLine("{", 1);
        toStringMethod.Append(cases);
        toStringMethod.AppendLine("};", 1);

        return toStringMethod.ToString();
    }

    private string GenerateGetHashCode()
    {
        var cases = new IndentedStringBuilder(5);
        var anyRefType = false;
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            if (type.IsReferenceType)
            {
                anyRefType = true;
            }
            else
            {
                cases.AppendLine($"{i} => {ValueFieldNamePrefix}{i}{type.CallOperator}GetHashCode(),");
            }
        }

        if (anyRefType)
        {
            cases.AppendLine($"{RefTypeIndex} => {RefValueFieldName}?.GetHashCode(),");
        }

        cases.AppendLine("_ => 0");

        var hashCodeMethod = new IndentedStringBuilder(2, "public override int GetHashCode()");
        hashCodeMethod.Append("{");
        hashCodeMethod.AppendLine(string.Empty);
        hashCodeMethod.AppendLine("unchecked", 1);
        hashCodeMethod.AppendLine("{", 1);
        hashCodeMethod.AppendLine($"var hash = {IndexPropertyName} switch", 2);
        hashCodeMethod.AppendLine("{", 2);
        hashCodeMethod.Append(cases);
        hashCodeMethod.AppendLine($"}}{(anyRefType ? " ?? 0" : string.Empty)};", 2);
        hashCodeMethod.AppendLine($"return (hash * 397) ^ {IndexPropertyName};", 2);
        hashCodeMethod.AppendLine("}", 1);
        hashCodeMethod.AppendLine("}");

        return hashCodeMethod.ToString();
    }

    private string GenerateImplicitOperators()
    {
        var operators = new IndentedStringBuilder(2);

        foreach (var type in union.TypeParameters)
        {
            operators.AppendLine($"public static implicit operator {union.Name}({type.FullName} {ValueParameterName}) => new {union.Name}({ValueParameterName});");
        }

        operators.AppendLine($"public static bool operator ==({union.Name} left, {union.Name} right) => left.Equals(right);");
        operators.AppendLine($"public static bool operator !=({union.Name} left, {union.Name} right) => !left.Equals(right);");

        return operators.ToString();
    }

    private string GenerateConstructors()
    {
        var constructors = new IndentedStringBuilder(2);
        constructors.AppendLine(GenerateStaticConstructor());
        constructors.AppendLine(GeneratePrivateConstructor());

        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];

            string index;
            string valueField;
            if (type.IsReferenceType)
            {
                index = RefTypeIndex;
                valueField = RefValueFieldName;
            }
            else
            {
                index = i.ToString();
                valueField = $"{ValueFieldNamePrefix}{i}";
            }

            var constructor
                = new IndentedStringBuilder(0, $"public {union.Name}({type.FullName} {ValueParameterName})");
            constructor.Append($": this({index}, {i}){IndentedStringBuilder.NewLine}");
            constructor.AppendLine("{", 2);
            constructor.AppendLine($"{valueField} = {ValueParameterName};", 3);
            constructor.AppendLine("}", 2);
            constructors.AppendLine(constructor.ToString());
        }

        constructors.AppendLine("[Obsolete(\"Use one of the constructors with a parameter, this one will configure the union incorrectly\", true)]");
        constructors.AppendLine($"public {union.Name}(): this(0, 0) {{}}");

        return constructors.ToString();
    }

    private string GeneratePrivateConstructor()
    {
        var constructor
            = new IndentedStringBuilder(0,
                                        $"private {union.Name}(int {IndexParameterName}, int {ActualTypeIndexParameterName}){IndentedStringBuilder.NewLine}");
        constructor.AppendLine("{", 2);
        constructor.AppendLine($"{StateByteFieldName} = new {StateByteTypeName}({IndexParameterName}, {ActualTypeIndexParameterName});",
                               3);
        constructor.AppendLine("}", 2);

        return constructor.ToString();
    }

    private string GenerateStaticConstructor()
    {
        var constructor = new IndentedStringBuilder(0, $"static {union.Name}(){IndentedStringBuilder.NewLine}");
        constructor.AppendLine("{", 2);
        constructor.AppendLine($"{PointerSizeGuardTypeName}.{PointerSizeGuardMethodName}();", 3);
        constructor.AppendLine("}", 2);

        return constructor.ToString();
    }

    private string GenerateGetActualTypeName()
    {
        var func = new IndentedStringBuilder(2, $"public string {TypeLookupFunc}() =>{IndentedStringBuilder.NewLine}");
        func.AppendLine($"{ActualTypeIndexPropertyName} switch ", 1);
        func.AppendLine("{", 1);
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            func.AppendLine($"{i} => \"{type.FullName}\",", 2);
        }

        func.AppendLine($"_ => throw new InvalidOperationException($\"Unknown type index {{{ActualTypeIndexPropertyName}}}\")",
                        2);
        func.AppendLine("};", 1);

        return func.ToString();
    }

    private string GenerateAccessors()
    {
        var accessors = new IndentedStringBuilder(2);
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            accessors.AppendLine($"public {type.FullName} As{type.TitleCaseName}() =>");
            accessors.AppendLine($"Is{type.TitleCaseName}", 1);
            accessors.AppendLine(type.IsReferenceType
                                     ? $"? ({type.FullName}) {RefValueFieldName}!"
                                     : $"? {ValueFieldNamePrefix}{i}",
                                 2);
            accessors.AppendLine($": throw new InvalidOperationException($\"Is not of type {type.FullName} but type {{{TypeLookupFunc}()}}\");",
                                 2);

            if (i < union.TypeParameters.Count - 1)
            {
                accessors.AppendLine(string.Empty);
            }
        }

        return accessors.ToString();
    }

    private (string TypeFields, string TypeProperties) GenerateFieldsAndProperties()
    {
        var (stateOffset, valueOffset) = GetOffsets();
        var typeProperties = new IndentedStringBuilder(2);
        var typeFields = new IndentedStringBuilder(2);
        typeFields.AppendLine($"[{InteropNamespace}.FieldOffset({stateOffset})]");
        typeFields.AppendLine($"private readonly {StateByteTypeName} {StateByteFieldName};");

        var refFieldCreated = false;
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];

            if (type.IsReferenceType)
            {
                if (!refFieldCreated)
                {
                    // if we have a ref type, it always goes first due to the fixed size (we assume 8 byte)
                    typeFields.AppendLine($"[{InteropNamespace}.FieldOffset(0)]");
                    typeFields.AppendLine($"private readonly object? {RefValueFieldName};");
                    refFieldCreated = true;
                }
            }
            else
            {
                typeFields.AppendLine($"[{InteropNamespace}.FieldOffset({valueOffset})]");
                typeFields.AppendLine($"private readonly {type.FullName} {ValueFieldNamePrefix}{i};");
            }

            var index = type.IsReferenceType ? RefTypeIndex : i.ToString();
            typeProperties.AppendLine($"public bool Is{type.TitleCaseName} => {IndexPropertyName} == {index};");
        }

        return (typeFields.ToString(), typeProperties.ToString());
    }

    private (int StateOffset, int ValueOffset) GetOffsets()
    {
        // for simplicity, we assume 64-bit system with 8 byte pointer size, because most common these days
        // 32-bit systems will waste 4 bytes here
        // larger pointer sizes are not supported and <see cref="PointerSizeGuard"/> will throw an exception
        var stateOffset = union.AnyReferenceType()
            ? 8
            : 0;
        
        // the state is stored in a single byte
        var valueOffset = stateOffset + 1;

        return (stateOffset, valueOffset);
    }
}
