namespace GitHubActionsVS.Helpers
{
    internal class StringHelpers
    {
        public static bool IsBase64(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length % 4 != 0)
                return false;

            foreach (char c in input)
            {
                if (!(char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '='))
                    return false;
            }

            try
            {
                _ = Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
