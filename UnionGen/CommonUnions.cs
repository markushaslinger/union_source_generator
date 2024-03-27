namespace UnionGen.Types;

[Union<Yes, No>]
public readonly partial struct YesOrNo;

[Union<True, False>]
public readonly partial struct TrueOrFalse;

[Union<Success, Failure>]
public readonly partial struct SuccessOrFailure;

[Union<Success, Error>]
public readonly partial struct OperationResult;

[Union<Success, NotFound, Error>]
public readonly partial struct ResOperationResult;

[Union<Found, NotFound>]
public readonly partial struct FoundOrNot;