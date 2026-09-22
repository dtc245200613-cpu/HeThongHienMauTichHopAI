namespace HeThongHienMauTichHopAI.Models
{
    public class Campaign
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Location { get; set; } = "";

        public DateTime Date { get; set; }

        public string Description { get; set; } = "";

        public int MaxParticipants { get; set; }
    }
}