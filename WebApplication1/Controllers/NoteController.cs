using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;
using static Azure.Core.HttpHeader;

namespace WebApplication1.Controllers
{
    public class NoteController : Controller
    {
        private readonly INoteRepository _noteRepository;

        public NoteController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public IActionResult All(int n = 0, string sort = null)
        {
            var notesQuery = _noteRepository.GetAllNotes();

            // Применяем сортировку, если задано свойство для сортировки
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

            // Применяем ограничение по количеству объектов
            if (n > 0)
            {
                notesQuery = notesQuery.Take(n);
            }

            var notes = notesQuery.ToList();
            ViewData["Sort"] = sort;
            ViewData["N"] = n;

            return View(notes);
        }

        public IActionResult Add()
            {
                var categories = _noteRepository.GetCategories().ToList();
                ViewBag.Categories = categories;
                return View();
            }

        [HttpPost]
        public IActionResult Add(Note note, List<int> selectedCategoryIds)
        {
            // Проверка совпадения ключа
            if (_noteRepository.GetAllNotes().Any(n => n.Id == note.Id))
            {
                ModelState.AddModelError("", "A note with the same ID already exists.");
            }

            // Проверка совпадения всех свойств кроме ключа
            if (_noteRepository.GetAllNotes().Any(n =>
                n.Title == note.Title &&
                n.Description == note.Description &&
                n.Date == note.Date &&
                n.Categories.Select(c => c.Id).OrderBy(id => id).SequenceEqual(selectedCategoryIds.OrderBy(id => id))))
            {
                ModelState.AddModelError("", "A note with the same properties already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_noteRepository.GetCategories(), "Id", "Title");
                return View(note);
            }

            // Добавление выбранных категорий
            if (selectedCategoryIds != null && selectedCategoryIds.Count > 0)
            {
                note.Categories = _noteRepository.GetCategories()
                    .Where(c => selectedCategoryIds.Contains(c.Id))
                    .ToList();
            }

            _noteRepository.AddNote(note);
            return RedirectToAction("Stat", "Home");
        }

        public IActionResult Remove(int id)
        {
            var note = _noteRepository.GetAllNotes().FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            _noteRepository.RemoveNote(note);
            return RedirectToAction("All");
        }

        public IActionResult Details(int id)
        {
            var note = _noteRepository.GetAllNotes().FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        public IActionResult Edit(int id)
        {
            var note = _noteRepository.GetAllNotes().FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }


        [HttpPost]
        public IActionResult Edit(Note note)
        {
            if (ModelState.IsValid)
            {
                // Обновление данных в БД
                _noteRepository.UpdateNote(note);
                return RedirectToAction("All");
            }

            // Если модель не валидна, повторно отображаем форму редактирования с сообщениями об ошибках
            return View(note);
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Filter(string searchWords)
        {
            // Разделяем введенные ключевые слова по запятым и удаляем лишние пробелы
            var keywords = searchWords.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(word => word.Trim())
                                      .ToList();

            var allNotes = _noteRepository.GetAllNotes();

            if (keywords == null || keywords.Count == 0)
            {
                return View("Filter", allNotes);
            }

            var filteredNotes = allNotes.Where(note =>
                keywords.Any(word =>
                    note.Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    note.Description.Contains(word, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return View("Filter", filteredNotes);
        }



    }
}
