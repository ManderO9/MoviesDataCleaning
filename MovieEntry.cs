namespace MoviesDataCleaning;

public class MovieEntry
{
    public string? PanelisId { get; set; }
    public string? Batch { get; set; }
    public string? Market { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? Region { get; set; }
    public string? Access { get; set; }
    public string? Usage { get; set; }
    public string? Day { get; set; }
    public DateTime? Date { get; set; }
    public string? Content { get; set; }
    public string? Title { get; set; }
    public string? Platform { get; set; }
    public string? EpisodeName { get; set; }
    public string? Season { get; set; }
    public string? ViewTime { get; set; }
    public string? DurationViewed { get; set; }
    public string? DeviceViewed { get; set; }


    public string? O_CleanedPlatform { get; set; }
    public string? O_CleanedTitle { get; set; }
    public bool O_IsGenericTermAndNotTitle { get; set; }
    public string? O_AllocatedTitle { get; set; }

    public bool C_TitlesEqual => Title == O_CleanedTitle;
    public bool C_TitlesEqualIgnoringCase => string.Equals(Title, O_CleanedTitle, StringComparison.OrdinalIgnoreCase);
    public bool C_TitlesEqualIgnoringPunctuationAndSpace
    {
        get
        {
            if(Title == null || O_CleanedTitle == null)
                return false;

            return Title.Where(x => !char.IsPunctuation(x) && !char.IsWhiteSpace(x)).Select(x => char.ToLower(x))
                    .SequenceEqual(O_CleanedTitle.Where(x => !char.IsPunctuation(x) && !char.IsWhiteSpace(x)).Select(x => char.ToLower(x)));
        }
    }
    public bool C_AiEqual => Ai_Title == O_CleanedTitle;
    public bool C_AiEqualIgnoringCase => string.Equals(Ai_Title, O_CleanedTitle, StringComparison.OrdinalIgnoreCase);

    public string? Ai_Title { get; set; }
    public double Ai_Confidence { get; set; }
    public bool Ai_IsGenericTermAndNotTitle { get; set; }
}
