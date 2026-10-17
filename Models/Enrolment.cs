namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int Id { get; set; }

        public int ParticipantId { get; set; }
        public User Participant { get; set; } = null!;

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public string? FinishTime { get; set; } // Set by Organiser
        public int? Position { get; set; }      // Set by Organiser
    }
}