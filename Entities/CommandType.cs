namespace CalMan.Entities
{
    public enum CommandType
    {
        Help,
        Add,
    }

    public static class CommandTypeExtensions
    {
        public static string ToCommandString(this CommandType command) => nameof(command).ToLower();
    }
}