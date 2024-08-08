using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public interface INoteRepository
    {
        IEnumerable<Note> GetAllNotes();
        bool AddNote(Note note);
        bool RemoveNote(Note note);
        bool UpdateNote(Note note);

        IEnumerable<Category> GetCategories();
        bool AddCategory(Category category);
        bool RemoveCategory(Category category);
        bool UpdateCategory(Category category);
    }


        public class EFNoteRepository : INoteRepository
        {
            private readonly ApplicationDbContext _context;

            public EFNoteRepository(ApplicationDbContext context)
            {
                _context = context;
            }

            public IEnumerable<Note> GetAllNotes()
            {
                return _context.Notes
                               .Include(n => n.Categories)
                               .ToList();
            }

            public bool AddNote(Note note)
            {
                _context.Notes.Add(note);
                return _context.SaveChanges() > 0;
            }

            public bool RemoveNote(Note note)
            {
                _context.Notes.Remove(note);
                return _context.SaveChanges() > 0;
            }

            public bool UpdateNote(Note note)
            {
                _context.Notes.Update(note);
                return _context.SaveChanges() > 0;
            }

            public IEnumerable<Category> GetCategories()
            {
                return _context.Categories.Include(c => c.Notes).ToList();
            }

        public bool AddCategory(Category category)
            {
                _context.Categories.Add(category);
                return _context.SaveChanges() > 0;
            }

            public bool RemoveCategory(Category category)
            {
                _context.Categories.Remove(category);
                return _context.SaveChanges() > 0;
            }

            public bool UpdateCategory(Category category)
            {
                _context.Categories.Update(category);
                return _context.SaveChanges() > 0;
            }
        }
    }