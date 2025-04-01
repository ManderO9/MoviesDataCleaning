namespace MoviesDataCleaning;

public class ApiResponse
{
    public List<Candidate> Candidates { get; set; } = [];
    public class Candidate
    {
        public ContentT Content { get; set; } = new();

        public class ContentT
        {
            public List<Part> Parts { get; set; } = [];
            public class Part
            {
                public string Text { get; set; } = string.Empty;
            }

        }
    }
}
