namespace WorkflowToCSharp.Converter.Extensions
{
	public static class StringExtension
	{
		public static string FirstLetterLower(this string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			return char.ToLower(text[0]) + text.Substring(1);
		}
	}
}
