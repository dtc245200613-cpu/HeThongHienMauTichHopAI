using System.ComponentModel.DataAnnotations;

namespace HeThongHienMauTichHopAI.Models
{
    public class DonationRegistration
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string VolunteerName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn nhóm máu")]
        public string BloodType { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn chiến dịch")]
        public int CampaignId { get; set; }

        public string Status { get; set; } = "Chờ duyệt";

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}