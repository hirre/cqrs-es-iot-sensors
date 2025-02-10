using IoT.Common;

namespace IoT.Extensions
{
    public static class EventTypeExtensions
    {
        public static string ToEventPrefix(this EventTypes eventType)
        {
            var name = Enum.GetName(eventType);

            if (name == null)
            {
                return string.Empty;
            }

            // Find the second capital letter, create a prefix from the first letter to the second capital letter
            for (var i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    return name[..i];
                }
            }

            return name;
        }
    }
}
