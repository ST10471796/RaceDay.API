namespace RaceDay.API.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., Under 20, 10km

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
    }
}