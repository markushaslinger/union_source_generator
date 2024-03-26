﻿using System.Text;

namespace UnionGen;

internal readonly struct UnionGenHelper(UnionToGenerate union)
{
    private const string InteropNamespace = "System.Runtime.InteropServices";
    private const string PointerSizeGuardTypeName = $"{GenNamespace}.InternalUtil.PointerSizeGuard";
    private const string PointerSizeGuardMethodName = "EnsureAlignment";
    private const string TypeLookupFunc = "GetTypeName";
    private const string IndexParameterName = "index";
    private const string ActualTypeIndexParameterName = "actualTypeIndex";
    private const string StateByteFieldName = "_state";
    private const string IndexPropertyName = $"{StateByteFieldName}.Index";
    private const string ActualTypeIndexPropertyName = $"{StateByteFieldName}.ActualTypeIndex";
    private const string ValueParameterName = "value";
    private const string ValueFieldNamePrefix = $"_{ValueParameterName}";
    private const string RefValueFieldName = $"_{ValueParameterName}Ref";
    private const string GenNamespace = "UnionGen";
    private const string StateByteTypeName = $"{GenNamespace}.InternalUtil.StateByte";
    private const string RefTypeIndex = $"{StateByteTypeName}.RefTypeIndex";
    private const string ThrowHelperType = $"{GenNamespace}.InternalUtil.ThrowHelper";
    private const string ConstantsType = $"{GenNamespace}.InternalUtil.UnionGenInternalConst";
    private const int MinReferenceTypeSize = 8;

    public string GeneratePartialStruct()
    {
        var (prefixNesting, postfixNesting) = GetParentTypeNesting();
        var (typeFields, typeProperties) = GenerateFieldsAndProperties();

        var code = $$"""
                     {{UnionSourceGen.AutoGeneratedComment}}
                     #nullable enable
                     namespace {{union.Namespace}}
                     {
                     {{prefixNesting}}
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
                     {{GenerateGetTypeName()}}
                         }
                     {{postfixNesting}}
                     }
                     """;

        return code;
    }

    private (string? PrefixNesting, string? PostfixNesting) GetParentTypeNesting()
    {
        if (union.ParentTypes.Count == 0)
        {
            return (string.Empty, string.Empty);
        }
        
        var prefix = new StringBuilder();
        foreach (var parentType in union.ParentTypes.Reverse())
        {
            prefix.AppendLine($"public partial {parentType.Type} {parentType.Name} {{");
        }
        var postfix = new StringBuilder();
        for (var i = 0; i < union.ParentTypes.Count; i++)
        {
            postfix.AppendLine("}");
        }
        
        return (prefix.ToString(), postfix.ToString());
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

        cases.AppendLine($"default: throw {ThrowHelperType}.ThrowUnknownTypeIndex({ActualTypeIndexPropertyName});");

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

        cases.AppendLine($"_ => throw {ThrowHelperType}.ThrowUnknownTypeIndex({ActualTypeIndexPropertyName})");

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
            cases.AppendLine($"{RefTypeIndex} => {RefValueFieldName}?.ToString() ?? {ConstantsType}.NullString,");
        }

        cases.AppendLine($"_ => throw {ThrowHelperType}.ThrowUnknownTypeIndex({IndexPropertyName})");

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

        constructors.AppendLine($"[Obsolete({ConstantsType}.DefaultConstructorWarning, true)]");
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
        
        // we use at least 8 byte for a reference type, but potentially more
        constructor.AppendLine($"{PointerSizeGuardTypeName}.{PointerSizeGuardMethodName}({Math.Max(MinReferenceTypeSize, union.RequestedAlignment)});", 3);
        
        constructor.AppendLine("}", 2);

        return constructor.ToString();
    }

    private string GenerateGetTypeName()
    {
        var func = new IndentedStringBuilder(2, $"public string {TypeLookupFunc}(int index) =>{IndentedStringBuilder.NewLine}");
        func.AppendLine($"index switch ", 1);
        func.AppendLine("{", 1);
        for (var i = 0; i < union.TypeParameters.Count; i++)
        {
            var type = union.TypeParameters[i];
            func.AppendLine($"{i} => \"{type.FullName}\",", 2);
        }

        func.AppendLine($"_ => throw {ThrowHelperType}.ThrowUnknownTypeIndex(index)",
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
            accessors.AppendLine($": throw {ThrowHelperType}.ThrowNotOfType({TypeLookupFunc}({i}), {TypeLookupFunc}({ActualTypeIndexPropertyName}));",
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
        // for safety, we assume 64-bit system with 8 byte pointer size, because most common these days
        // 32-bit systems will waste 4 bytes here
        var stateOffset = union.AnyReferenceType()
            ? union.RequestedAlignment > MinReferenceTypeSize
                ? union.RequestedAlignment
                : MinReferenceTypeSize
            : 0;
        
        // the state is stored in a single byte
        var valueOffset = stateOffset + Math.Max(1, union.RequestedAlignment);

        return (stateOffset, valueOffset);
    }
}