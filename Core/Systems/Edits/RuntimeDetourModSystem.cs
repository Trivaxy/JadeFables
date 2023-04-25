namespace JadeFables.Core.Systems.Edits;

/// <summary>
///     Abstraction over <see cref="ErrorCollectingModSystem"/> with utilities for handling runtime detouring (IL edits and detours).
/// </summary>
public abstract class RuntimeDetourModSystem : ErrorCollectingModSystem
{
    protected class OpCodeError : SystemLoadError
    {
        private readonly string TypeName;
        private readonly string MethodName;
        private readonly string OpCode;
        private readonly object? Value;
        private readonly int? Iteration;

        public OpCodeError(string typeName, string methodName, string opCode, object? value = null, int? iteration = null)
        {
            TypeName = typeName;
            MethodName = methodName;
            OpCode = opCode;
            Value = value;
            Iteration = iteration;
        }

        protected override string AsLoggableImpl()
        {
            return ToString();
        }

        protected override string AsReportableImpl()
        {
            return ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(TypeName)
              .Append("::")
              .Append(MethodName)
              .Append(' ')
              .Append(OpCode);

            if (Value is not null)
            {
                sb.Append(' ')
                  .Append(Value);
            }

            if (Iteration is not null)
            {
                sb.Append(" (")
                  .Append(Iteration)
                  .Append(')');
            }

            return sb.ToString();
        }
    }

    protected class MissingMemberError : SystemLoadError
    {
        private readonly string Member;

        public MissingMemberError(string member)
        {
            Member = member;
        }

        protected override string AsLoggableImpl()
        {
            return ToString();
        }

        protected override string AsReportableImpl()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "Failed to find missing member: " + Member;
        }
    }

    protected void AddOpCodeError(string typeName, string methodName, string opCode, object? value = null, int? iteration = null)
    {
        AddError(new OpCodeError(typeName, methodName, opCode, value, iteration));
    }

    protected void AddMissingMemberError(string member)
    {
        AddError(new MissingMemberError(member));
    }
}