namespace WebApplication1.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
