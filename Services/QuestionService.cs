using System.Text.Json;
using SIPrep.Models;

namespace SIPrep.Services;

public class QuestionService
{
    private static QuestionService? _instance;
    public static QuestionService Instance => _instance ??= new QuestionService();

    private List<Question> _allQuestions = new();
    private bool _isLoaded;

    public bool IsLoaded => _isLoaded;
    public int TotalCount => _allQuestions.Count;

    public async Task EnsureLoadedAsync()
    {
        if (_isLoaded && _allQuestions.Count > 0)
            return;

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("questions.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var questions = JsonSerializer.Deserialize<List<Question>>(json, options);
            if (questions != null)
            {
                _allQuestions = questions;
                _isLoaded = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading questions: {ex.Message}");
        }
    }

    public List<Question> GetAllQuestions()
    {
        return _allQuestions;
    }

    public List<Question> GetQuestionsByPaper(int paperNumber)
    {
        return _allQuestions.Where(q => q.Paper == paperNumber).OrderBy(q => q.No).ToList();
    }

    public List<Question> GetQuestionsBySubject(string subject)
    {
        return _allQuestions.Where(q => q.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase)).OrderBy(q => q.Paper).ThenBy(q => q.No).ToList();
    }

    public List<Question> GetPracticeBatch(int count, string? subject = null, int? paper = null)
    {
        var pool = _allQuestions.AsEnumerable();
        if (!string.IsNullOrEmpty(subject))
            pool = pool.Where(q => q.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase));
        if (paper.HasValue && paper.Value > 0)
            pool = pool.Where(q => q.Paper == paper.Value);

        return pool.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
    }

    public List<SubjectInfo> GetSubjectStats()
    {
        var subjectColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Arithmetic", "#0288D1" },
            { "Reasoning", "#7B1FA2" },
            { "General Science", "#388E3C" },
            { "Current Events", "#E65100" },
            { "Indian History", "#C2185B" },
            { "Geography", "#00796B" },
            { "Polity", "#303F9F" },
            { "Telangana Movement", "#D32F2F" },
            { "Telangana Culture", "#F57C00" },
            { "Telangana Economy & Governance", "#455A64" }
        };

        return _allQuestions
            .GroupBy(q => q.Subject)
            .Select(g => new SubjectInfo
            {
                Name = g.Key,
                Count = g.Count(),
                ColorHex = subjectColors.TryGetValue(g.Key, out var color) ? color : "#616161"
            })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    public List<PaperInfo> GetPaperStats()
    {
        var list = new List<PaperInfo>();

        // Series 1: Master Series (1 to 10)
        for (int p = 1; p <= 10; p++)
        {
            list.Add(new PaperInfo
            {
                PaperNumber = p,
                Series = "Master Series",
                SeriesBadgeColor = "#1E3A8A",
                Title = $"Model Paper {p:D2}",
                BookletCode = p switch { 1 or 5 or 9 => "Booklet A", 2 or 6 or 10 => "Booklet B", 3 or 7 => "Booklet C", _ => "Booklet D" },
                PdfFileName = $"TSLPRB_SI_Model_Paper_{p:D2}.pdf",
                QuestionCount = _allQuestions.Count(q => q.Paper == p)
            });
        }

        // Series 2: Prelims Pattern Series (11 to 20)
        for (int p = 11; p <= 20; p++)
        {
            int subSet = p - 10;
            list.Add(new PaperInfo
            {
                PaperNumber = p,
                Series = "Prelims Pattern Series",
                SeriesBadgeColor = "#0D9488",
                Title = $"Prelims Paper SET-{subSet:D2}",
                BookletCode = subSet switch { 1 or 5 or 9 => "Booklet A", 2 or 6 or 10 => "Booklet B", 3 or 7 => "Booklet C", _ => "Booklet D" },
                PdfFileName = $"TSLPRB_SI_Prelims_Model_Paper_SET-{subSet:D2}.pdf",
                QuestionCount = _allQuestions.Count(q => q.Paper == p)
            });
        }

        // Series 3: Codex High-Yield Series (21 to 30)
        for (int p = 21; p <= 30; p++)
        {
            int subSet = p - 20;
            list.Add(new PaperInfo
            {
                PaperNumber = p,
                Series = "High-Yield Practice",
                SeriesBadgeColor = "#7C3AED",
                Title = $"Practice Paper {subSet:D2}",
                BookletCode = subSet switch { 1 or 5 or 9 => "Booklet A", 2 or 6 or 10 => "Booklet B", 3 or 7 => "Booklet C", _ => "Booklet D" },
                PdfFileName = $"TSLPRB-SI-Model-Paper-{subSet:D2}.pdf",
                QuestionCount = _allQuestions.Count(q => q.Paper == p)
            });
        }

        return list;
    }

    public async Task<string?> OpenPdfPaperAsync(string pdfFileName)
    {
        try
        {
            var targetDir = Path.Combine(FileSystem.CacheDirectory, "SIPrep_PDFs");
            Directory.CreateDirectory(targetDir);
            var targetPath = Path.Combine(targetDir, pdfFileName);

            if (!File.Exists(targetPath) || new FileInfo(targetPath).Length == 0)
            {
                using var inStream = await FileSystem.OpenAppPackageFileAsync($"pdfs/{pdfFileName}");
                using var outStream = File.Create(targetPath);
                await inStream.CopyToAsync(outStream);
            }

            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                Title = pdfFileName,
                File = new ReadOnlyFile(targetPath)
            });

            return targetPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open PDF: {ex.Message}");
            return null;
        }
    }
}
