namespace UFCApp.Models
{
    public class HistoricalRecord
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public decimal Score { get; set; }

        public HistoricalRecord() { }
    }
}
