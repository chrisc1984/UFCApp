using Microsoft.AspNetCore.SignalR;

namespace UFCApp.Models
{
    public class UserPicks
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public List<UserPick> Picks { get; set; } 
        public UserPicks()
        {
            Picks = new List<UserPick>(); 
        }
    }

}
