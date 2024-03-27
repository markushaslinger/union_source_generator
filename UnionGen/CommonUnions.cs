namespace UnionGen.Types;

[Union<Yes, No>]
public readonly partial struct YesOrNo;

[Union<True, False>]
public readonly partial struct TrueOrFalse
{
    public static implicit operator TrueOrFalse(bool success) => success ? new True() : new False();
    public static implicit operator bool(TrueOrFalse result) => result.IsTrue;
}

[Union<Success, Failure>]
public readonly partial struct SuccessOrFailure;

[Union<Success, Error>]
public readonly partial struct OperationResult
{
    public static implicit operator OperationResult(bool success) => success ? new Success() : new Error();
    public static implicit operator bool(OperationResult result) => result.IsSuccess;
}

[Union<Success, NotFound, Error>]
public readonly partial struct ResOperationResult;

[Union<Found, NotFound>]
public readonly partial struct FoundOrNot;