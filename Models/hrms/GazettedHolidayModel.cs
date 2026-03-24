using System.ComponentModel.DataAnnotations;

namespace supmtigroupe.Models.hrms
{
    public class GazettedHolidayModel
    {
        [Key]
        public int Id { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public int companyId { get; set; }
        public CompanyModel? company { get; set; }
    }
    public class GazettedHolidayViewModel
    {
        public int Id { get; set; }
        public DateTime? date { get; set; }
        public string name { get; set; }
        public int companyId { get; set; }
        public CompanyModel? company { get; set; }
        public List<GazettedHolidayModel> gazettedHolidays { get; set; }
    }
}
