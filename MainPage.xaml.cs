using Microsoft.Maui.Controls.Shapes;
using SIPrep.Models;
using SIPrep.Services;

namespace SIPrep;

public partial class MainPage : ContentPage
{
    private bool _initialized;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        await InitializeDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        LoadingLayout.IsVisible = true;
        MainContentScroll.IsVisible = false;

        await Task.Run(async () =>
        {
            await QuestionService.Instance.EnsureLoadedAsync();
        });

        TotalQuestionsLabel.Text = QuestionService.Instance.TotalCount.ToString("N0");

        PopulatePapers();
        PopulateSubjects();

        LoadingLayout.IsVisible = false;
        MainContentScroll.IsVisible = true;
        _initialized = true;
    }

    private void PopulatePapers()
    {
        PapersContainer.Children.Clear();
        var papers = QuestionService.Instance.GetPaperStats();
        string currentSeries = "";

        foreach (var paper in papers)
        {
            if (paper.Series != currentSeries)
            {
                currentSeries = paper.Series;
                var seriesHeader = new Border
                {
                    BackgroundColor = Color.FromArgb(paper.SeriesBadgeColor),
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                    Padding = new Thickness(14, 10),
                    Margin = new Thickness(0, 12, 0, 4),
                    Content = new Label
                    {
                        Text = $"📚 {currentSeries.ToUpper()}",
                        TextColor = Colors.White,
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold
                    }
                };
                PapersContainer.Children.Add(seriesHeader);
            }

            var border = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E5E7EB"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                Padding = new Thickness(14, 12)
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb("#EFF6FF"),
                Stroke = Color.FromArgb("#BFDBFE"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                Padding = new Thickness(10, 6),
                Content = new Label
                {
                    Text = $"Set {paper.PaperNumber:D2}",
                    TextColor = Color.FromArgb(paper.SeriesBadgeColor),
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold
                }
            };
            grid.Add(badge, 0, 0);

            var infoStack = new VerticalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(12, 0, 0, 0),
                Children =
                {
                    new Label
                    {
                        Text = $"{paper.Title} ({paper.BookletCode})",
                        TextColor = Color.FromArgb("#1F2937"),
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = $"{paper.QuestionCount} Qs • 3 Hours • Part A & B",
                        TextColor = Color.FromArgb("#6B7280"),
                        FontSize = 12
                    }
                }
            };
            grid.Add(infoStack, 1, 0);

            int paperNum = paper.PaperNumber;
            string pdfName = paper.PdfFileName;
            string paperTitle = $"{paper.Title} ({paper.BookletCode})";

            async Task StartPaperTest()
            {
                var questions = QuestionService.Instance.GetQuestionsByPaper(paperNum);
                await Navigation.PushAsync(new QuizPage(questions, paperTitle));
            }

            async Task OpenPdfDoc()
            {
                var opened = await QuestionService.Instance.OpenPdfPaperAsync(pdfName);
                if (opened == null)
                {
                    await DisplayAlert("PDF Viewer", $"Could not open {pdfName}. You can also find all PDFs in /sdcard/Download/SIPrep_Papers/.", "OK");
                }
            }

            var buttonsLayout = new HorizontalStackLayout
            {
                Spacing = 6,
                VerticalOptions = LayoutOptions.Center
            };

            var pdfBtn = new Button
            {
                Text = "📄 PDF",
                BackgroundColor = Color.FromArgb("#F1F5F9"),
                TextColor = Color.FromArgb("#334155"),
                BorderColor = Color.FromArgb("#CBD5E1"),
                BorderWidth = 1,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 8,
                HeightRequest = 36,
                Padding = new Thickness(10, 0)
            };
            pdfBtn.Clicked += async (s, e) => await OpenPdfDoc();
            buttonsLayout.Children.Add(pdfBtn);

            var actionBtn = new Button
            {
                Text = "Start Test",
                BackgroundColor = Color.FromArgb(paper.SeriesBadgeColor),
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 8,
                HeightRequest = 36,
                Padding = new Thickness(12, 0)
            };
            actionBtn.Clicked += async (s, e) => await StartPaperTest();
            buttonsLayout.Children.Add(actionBtn);

            grid.Add(buttonsLayout, 2, 0);

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await StartPaperTest();
            border.GestureRecognizers.Add(tap);

            border.Content = grid;
            PapersContainer.Children.Add(border);
        }
    }

    private void PopulateSubjects()
    {
        SubjectsContainer.Children.Clear();
        var subjects = QuestionService.Instance.GetSubjectStats();

        foreach (var sub in subjects)
        {
            var border = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E5E7EB"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                Padding = new Thickness(14, 12)
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(4) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var colorBar = new BoxView
            {
                Color = Color.FromArgb(sub.ColorHex),
                WidthRequest = 4,
                CornerRadius = 2
            };
            grid.Add(colorBar, 0, 0);

            var infoStack = new VerticalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(12, 0, 0, 0),
                Children =
                {
                    new Label
                    {
                        Text = sub.Name,
                        TextColor = Color.FromArgb("#1F2937"),
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = $"{sub.Count} Questions across Sets 01–10",
                        TextColor = Color.FromArgb("#6B7280"),
                        FontSize = 12
                    }
                }
            };
            grid.Add(infoStack, 1, 0);

            string subName = sub.Name;
            async Task StartSubjectTest()
            {
                var questions = QuestionService.Instance.GetQuestionsBySubject(subName);
                await Navigation.PushAsync(new QuizPage(questions, $"Subject: {subName}"));
            }

            var practiceBtn = new Button
            {
                Text = "Practice",
                BackgroundColor = Color.FromArgb(sub.ColorHex),
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 8,
                HeightRequest = 36,
                Padding = new Thickness(12, 0)
            };
            practiceBtn.Clicked += async (s, e) => await StartSubjectTest();
            grid.Add(practiceBtn, 2, 0);

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await StartSubjectTest();
            border.GestureRecognizers.Add(tap);

            border.Content = grid;
            SubjectsContainer.Children.Add(border);
        }
    }

    private async void OnQuickBlitzClicked(object? sender, EventArgs e)
    {
        var questions = QuestionService.Instance.GetPracticeBatch(25);
        await Navigation.PushAsync(new QuizPage(questions, "⚡ 25-Question Quick Blitz"));
    }

    private async void OnMiniMockClicked(object? sender, EventArgs e)
    {
        var questions = QuestionService.Instance.GetPracticeBatch(50);
        await Navigation.PushAsync(new QuizPage(questions, "🎯 50-Question Mini Mock"));
    }
}
