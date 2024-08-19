using Microsoft.AspNetCore.SignalR;

namespace UFCApp.Models
{
    public class UserPick
    {
        public int FightId { get; set; }
        public string Pick { get; set; }

        public UserPick() 
        { 
            Pick = string.Empty;
        }
    }

}
