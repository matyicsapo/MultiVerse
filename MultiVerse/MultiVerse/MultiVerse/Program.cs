namespace MultiVerse
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
			GameMultiVerse.Instance.Run();
        }
    }
#endif
}

