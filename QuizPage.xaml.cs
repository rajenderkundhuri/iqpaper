using SIPrep.Models;

namespace SIPrep;

public partial class QuizPage : ContentPage
{
    private readonly List<Question> _questions;
    private readonly string _title;
    private int _currentIndex = 0;

    private static readonly Color NeutralBackground = Colors.White;
    private static readonly Color NeutralBorder = Color.FromArgb("#D1D5DB");
    private static readonly Color CorrectBackground = Color.FromArgb("#DCFCE7");
    private static readonly Color CorrectBorder = Color.FromArgb("#16A34A");
    private static readonly Color IncorrectBackground = Color.FromArgb("#FEE2E2");
    private static readonly Color IncorrectBorder = Color.FromArgb("#DC2626");

    public QuizPage(List<Question> questions, string title)
    {
        InitializeComponent();
        _questions = questions ?? new List<Question>();
        _title = title;
        SessionTitleLabel.Text = _title;

        if (_questions.Count > 0)
        {
            LoadQuestion(_currentIndex);
        }
    }

    private void LoadQuestion(int index)
    {
        if (index < 0 || index >= _questions.Count)
            return;

        _currentIndex = index;
        var q = _questions[_currentIndex];

        QuestionCounterLabel.Text = $"Q {_currentIndex + 1} / {_questions.Count}";
        QuizProgressBar.Progress = (double)(_currentIndex + 1) / _questions.Count;

        SubjectLabel.Text = q.Subject;
        FlagBtn.Text = q.IsFlagged ? "★" : "☆";
        FlagBtn.TextColor = q.IsFlagged ? Colors.Yellow : Colors.White;

        QuestionTextLabel.Text = q.Q;

        // Populate options
        OptTextA.Text = q.Opts.Count > 0 ? q.Opts[0] : "";
        OptTextB.Text = q.Opts.Count > 1 ? q.Opts[1] : "";
        OptTextC.Text = q.Opts.Count > 2 ? q.Opts[2] : "";
        OptTextD.Text = q.Opts.Count > 3 ? q.Opts[3] : "";

        PrevBtn.IsEnabled = _currentIndex > 0;
        NextBtn.Text = _currentIndex == _questions.Count - 1 ? "Finish" : "Next ▶";

        UpdateOptionVisuals(q);
        UpdateScoreSummary();
    }

    private void UpdateOptionVisuals(Question q)
    {
        // Reset all borders
        ResetBorder(OptBorderA);
        ResetBorder(OptBorderB);
        ResetBorder(OptBorderC);
        ResetBorder(OptBorderD);

        if (!q.IsAnswered)
        {
            ExplanationCard.IsVisible = false;
            return;
        }

        // Highlight correct and selected
        int selected = q.SelectedAnswerIndex!.Value;
        int correct = q.Ans;

        HighlightBorder(GetBorderByIndex(correct), CorrectBackground, CorrectBorder);

        if (selected != correct)
        {
            HighlightBorder(GetBorderByIndex(selected), IncorrectBackground, IncorrectBorder);
            ResultHeaderLabel.Text = $"✗ Incorrect. Correct is Option {(char)('A' + correct)}";
            ResultHeaderLabel.TextColor = Color.FromArgb("#DC2626");
        }
        else
        {
            ResultHeaderLabel.Text = $"✓ Correct! Option {(char)('A' + correct)}";
            ResultHeaderLabel.TextColor = Color.FromArgb("#16A34A");
        }

        var explanation = string.IsNullOrWhiteSpace(q.Explanation)
            ? $"Option {(char)('A' + correct)} is the correct verified answer."
            : q.Explanation;

        ExplanationTextLabel.Text = explanation
            .Replace("<br/>", "\n")
            .Replace("<br>", "\n")
            .Replace("<br />", "\n");

        if (!string.IsNullOrWhiteSpace(q.Tip))
        {
            TipBox.IsVisible = true;
            TipTextLabel.Text = q.Tip;
        }
        else
        {
            TipBox.IsVisible = false;
        }

        ExplanationCard.IsVisible = true;
    }

    private Border GetBorderByIndex(int idx) => idx switch
    {
        0 => OptBorderA,
        1 => OptBorderB,
        2 => OptBorderC,
        3 => OptBorderD,
        _ => OptBorderA
    };

    private void ResetBorder(Border border)
    {
        border.BackgroundColor = NeutralBackground;
        border.Stroke = NeutralBorder;
    }

    private void HighlightBorder(Border border, Color bg, Color stroke)
    {
        border.BackgroundColor = bg;
        border.Stroke = stroke;
    }

    private void SelectOption(int optionIndex)
    {
        if (_currentIndex < 0 || _currentIndex >= _questions.Count)
            return;

        var q = _questions[_currentIndex];
        q.SelectedAnswerIndex = optionIndex;
        UpdateOptionVisuals(q);
        UpdateScoreSummary();
    }

    private void UpdateScoreSummary()
    {
        int attempted = _questions.Count(q => q.IsAnswered);
        int correct = _questions.Count(q => q.IsCorrect);
        double accuracy = attempted > 0 ? (double)correct / attempted * 100 : 0;
        ScoreSummaryLabel.Text = $"Score: {correct}/{attempted} ({accuracy:F0}%)";
    }

    private void OnOptionATapped(object? sender, EventArgs e) => SelectOption(0);
    private void OnOptionBTapped(object? sender, EventArgs e) => SelectOption(1);
    private void OnOptionCTapped(object? sender, EventArgs e) => SelectOption(2);
    private void OnOptionDTapped(object? sender, EventArgs e) => SelectOption(3);

    private void OnFlagClicked(object? sender, EventArgs e)
    {
        if (_currentIndex >= 0 && _currentIndex < _questions.Count)
        {
            var q = _questions[_currentIndex];
            q.IsFlagged = !q.IsFlagged;
            FlagBtn.Text = q.IsFlagged ? "★" : "☆";
            FlagBtn.TextColor = q.IsFlagged ? Colors.Yellow : Colors.White;
        }
    }

    private void OnPrevClicked(object? sender, EventArgs e)
    {
        if (_currentIndex > 0)
        {
            LoadQuestion(_currentIndex - 1);
        }
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (_currentIndex < _questions.Count - 1)
        {
            LoadQuestion(_currentIndex + 1);
        }
        else
        {
            await ShowFinalResultsAsync();
        }
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        await ShowFinalResultsAsync();
    }

    private async Task ShowFinalResultsAsync()
    {
        int total = _questions.Count;
        int attempted = _questions.Count(q => q.IsAnswered);
        int correct = _questions.Count(q => q.IsCorrect);
        int wrong = attempted - correct;
        double accuracy = attempted > 0 ? (double)correct / attempted * 100 : 0;

        string message = $"Total Questions: {total}\n" +
                         $"Attempted: {attempted}\n" +
                         $"Correct: {correct}\n" +
                         $"Incorrect: {wrong}\n" +
                         $"Accuracy: {accuracy:F1}%\n\n" +
                         $"Would you like to return to the Dashboard or keep reviewing?";

        bool exit = await DisplayAlert("Session Summary", message, "Return Home", "Review Answers");
        if (exit)
        {
            await Navigation.PopAsync();
        }
    }
}
