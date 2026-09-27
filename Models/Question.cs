using System.Text.Json.Serialization;

namespace SIPrep.Models;

public class Question
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("paper")]
    public int Paper { get; set; }

    [JsonPropertyName("no")]
    public int No { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("q")]
    public string Q { get; set; } = string.Empty;

    [JsonPropertyName("opts")]
    public List<string> Opts { get; set; } = new();

    [JsonPropertyName("ans")]
    public int Ans { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("tip")]
    public string Tip { get; set; } = string.Empty;

    [JsonPropertyName("series")]
    public string Series { get; set; } = string.Empty;

    [JsonPropertyName("pdf_file")]
    public string PdfFile { get; set; } = string.Empty;

    // Runtime Quiz tracking
    [JsonIgnore]
    public int? SelectedAnswerIndex { get; set; }

    [JsonIgnore]
    public bool IsAnswered => SelectedAnswerIndex.HasValue;

    [JsonIgnore]
    public bool IsCorrect => SelectedAnswerIndex.HasValue && SelectedAnswerIndex.Value == Ans;

    [JsonIgnore]
    public bool IsFlagged { get; set; }
}

public class SubjectInfo
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ColorHex { get; set; } = "#1976D2";
    public string Icon { get; set; } = "📋";
}

public class PaperInfo
{
    public int PaperNumber { get; set; }
    public string Series { get; set; } = string.Empty;
    public string SeriesBadgeColor { get; set; } = "#1E3A8A";
    public string Title { get; set; } = string.Empty;
    public string BookletCode { get; set; } = string.Empty;
    public string PdfFileName { get; set; } = string.Empty;
    public int QuestionCount { get; set; } = 200;
}
