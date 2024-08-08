using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{ 
    public class CategoryController : Controller
    {
        private readonly INoteRepository _noteRepository;

        public CategoryController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public IActionResult All(int n = 0, string sort = null)
        {
            var categoriesQuery = _noteRepository.GetCategories();

            // Применяем сортировку, если задано свойство для сортировки
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
                        categoriesQuery = categoriesQuery.OrderBy(c => c.Id);
                        break;
                    default:
                        break;
                }
            }

            // Применяем ограничение по количеству объектов
            if (n > 0)
            {
                categoriesQuery = categoriesQuery.Take(n);
            }

            var categories = categoriesQuery.ToList();
            ViewData["Sort"] = sort;
            ViewData["N"] = n;

            return View(categories);
        }

        public IActionResult Details(int id)
        {
            var category = _noteRepository.GetCategories().FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public IActionResult Add()
        {
            var categories = _noteRepository.GetCategories().ToList();
            ViewBag.Categories = categories;
            return View();
        }


        [HttpPost]
        public IActionResult Add(Category category)
        {
            // Проверка совпадения ключа
            if (_noteRepository.GetCategories().Any(c => c.Id == category.Id))
            {
                ModelState.AddModelError("", "A category with the same ID already exists.");
            }

            // Проверка совпадения всех свойств кроме ключа
            if (_noteRepository.GetCategories().Any(c =>
                c.Title == category.Title &&
                c.Description == category.Description))
            {
                ModelState.AddModelError("", "A category with the same properties already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _noteRepository.AddCategory(category);
            return RedirectToAction("Stat", "Home");
        }

        public IActionResult Remove(int id)
        {
            var category = _noteRepository.GetCategories().FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            _noteRepository.RemoveCategory(category);
            return RedirectToAction("All");
        }

        public IActionResult Edit(int id)
        {
            var category = _noteRepository.GetCategories().FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _noteRepository.UpdateCategory(category);
                return RedirectToAction("All");
            }

            return View(category);
        }




    }
}
