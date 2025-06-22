namespace Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ReverseString(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            string reversed = string.Create(
                str.Length,
                str,
                (chars, state) =>
                {
                    int index = 0;
                    for (int i = state.Length - 1; i >= 0; i--)
                    {
                        chars[index++] = state[i];
                    }
                });

            return reversed;
        }
    }
}
