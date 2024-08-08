using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using WebApplication1.Models;
using System.Text;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly INoteRepository _noteRepository;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, INoteRepository noteRepository)
        {
            _logger = logger;
            _noteRepository = noteRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Stat()
        {
            // �������� ���������� ������� � ���������
            var noteCount = _noteRepository.GetAllNotes().Count();
            var categoryCount = _noteRepository.GetCategories().Count();
            var notesPerCategory = new Dictionary<string, int>();

            // �������� ���������� ������� � ������ ��������� � ��������� � �������
            foreach (var category in _noteRepository.GetCategories())
            {

                notesPerCategory[category.Title] = category.Notes.Count();
            }

            // �������� ����������� � ������������ ���� �������
            var minDate = _noteRepository.GetAllNotes().Min(n => n.Date);
            var maxDate = _noteRepository.GetAllNotes().Max(n => n.Date);

            // �������� ������ � ������������� ����� ViewData
            ViewData["NoteCount"] = noteCount;
            ViewData["CategoryCount"] = categoryCount;
            // �������� ������ � ������������� ����� ViewData
            ViewData["CategoryCount"] = categoryCount;
            ViewData["NotesPerCategory"] = notesPerCategory;
            // ����������� � ������������ ���� �������
            ViewData["Date"] = new DateTime[] { minDate, maxDate };
            return View();
        }

        public IActionResult Export(int n = 0, string sort = null)
        {
            var notes = GetNotes(n, sort);
            var categories = GetCategories(n, sort);

            var jsonData = new
            {
                Notes = notes.Select(note => new
                {
                    Id = note.Id,
                    Title = note.Title,
                    Description = note.Description,
                    Date = note.Date,
                    Categories = note.Categories.Select(category => category.Title).ToList()
                }).ToList(),

                Categories = categories.Select(category => new
                {
                    Id = category.Id,
                    Title = category.Title,
                    Description = category.Description,
                    NotesIds = category.Notes.Select(note => note.Id).ToList()
                }).ToList()
            };

            var json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
            {
                WriteIndented = true // ��� ��������� �������������� JSON
            });

            var fileName = "exported_data.json";
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", fileName);
        }
        private IEnumerable<Note> GetNotes(int n, string sort)
        {
            var notesQuery = _noteRepository.GetAllNotes();

            // ��������� ����������, ���� ������ �������� ��� ����������
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "title":
                        notesQuery = notesQuery.OrderBy(n => n.Title);
                        break;
                    case "description":
                        notesQuery = notesQuery.OrderBy(n => n.Description);
                        break;
                    case "date":
                        notesQuery = notesQuery.OrderBy(n => n.Date);
                        break;
                    case null:
                        notesQuery = notesQuery.OrderBy(n => n.Id);
                        break;
                    default:
                        break;
                }
            }

            // ��������� ����������� �� ���������� ��������
            if (n > 0)
            {
                notesQuery = notesQuery.Take(n);
            }

            return notesQuery;
        }

        private IEnumerable<Category> GetCategories(int n, string sort)
        {
            var categoriesQuery = _noteRepository.GetCategories();

            // ��������� ����������, ���� ������ �������� ��� ����������
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "title":
                        categoriesQuery = categoriesQuery.OrderBy(c => c.Title);
                        break;
                    case "description":
                        categoriesQuery = categoriesQuery.OrderBy(c => c.Description);
                        break;
                    case null:
                        categoriesQuery = categoriesQuery.OrderBy(n => n.Id);
                        break;
                    default:
                        break;
                }
            }

            // ��������� ����������� �� ���������� ��������
            if (n > 0)
            {
                categoriesQuery = categoriesQuery.Take(n);
            }

            return categoriesQuery;
        }
    }

}
