// DataService.cs
using Npgsql;
using UFCApp.Models;

namespace UFCApp.Data
{
    public class DataService
    {
        private readonly string _connString;

        public DataService(IConfiguration configuration)
        {
            // Read the connection string from the configuration
            _connString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Player>> GetPlayersAsync()
        {
            List<Player> players = new List<Player>();

            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                using var command = new NpgsqlCommand("SELECT player_id,player_name, player_score FROM GetPlayerScores();", conn);
  
                using var reader = command.ExecuteReader();

                int i = 1;
                while (reader.Read())
                {
                    var player = new Player
                    {
                        Id = Convert.ToInt32(reader["player_id"]),
                        Name = reader["player_name"].ToString(),
                        Score = Convert.ToDecimal(reader["player_score"])
                    };
                    players.Add(player);
                    i++;
                }

                conn.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            return players;
        }

        public async Task<List<Event>> GetPastEventIds()
        {
            List<Event> pastEvents = new List<Event>();

            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT id, name FROM tblEvent ORDER BY starttime DESC", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                { 
                    Event e = new Event();
                    e.Id = reader.GetInt32(0);
                    e.Name = reader.GetString(1);
                    pastEvents.Add(e);               
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            return pastEvents;
        }

        public async Task<Dictionary<int, List<HistoricalRecord>>> GetHistoricalRecords()
        {
            Dictionary<int, List<HistoricalRecord>> recordByYear  = new Dictionary<int, List<HistoricalRecord>>();
            List<HistoricalRecord> historicalRecords = new List<HistoricalRecord>();

            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                                SELECT year, H.userId, U.name, H.Score
                                FROM tblHistory H 
                                    JOIN tblUser U ON H.userId = U.id
                                ORDER BY year, score DESC"
                                , conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    HistoricalRecord r = new HistoricalRecord();
                    r.Year = reader.GetInt32(0);
                    r.UserId = reader.GetInt32(1);
                    r.Name = reader.GetString(2);
                    r.Score = reader.GetDecimal(3);

                    historicalRecords.Add(r);
                }

                // Group the records by Year and convert to Dictionary<int, List<HistoricalRecord>>
                recordByYear = historicalRecords
                    .GroupBy(r => r.Year)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            return recordByYear;
        }

        public async Task<List<Event>> GetPastEvents()
        {
            List <Event> pastEvents = await GetPastEventIds();
  
            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                foreach (var pastEvent in pastEvents)
                {
                    using var cmd = new NpgsqlCommand(@"
                    SELECT 
                        F.id,
                        fighter1name, 
                        fighter1odds, 
                        fighter2name, 
                        fighter2odds, 
                        winner, 
                        starttime 
                    FROM tblEvent E 
                        JOIN tblFight F ON F.eventId = E.id 
                    WHERE E.id = @eventId
                    ORDER BY F.id", conn);

                    cmd.Parameters.AddWithValue("@eventId", pastEvent.Id); 
                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        Fight fight = new Fight();
                        fight.Id = reader.GetInt32(0);
                        fight.Fighter1Name = reader.GetString(1);
                        fight.Fighter1Points = reader.GetDecimal(2);
                        fight.Fighter2Name = reader.GetString(3);
                        fight.Fighter2Points = reader.GetDecimal(4);
                        fight.Winner = reader.GetString(5);
                        pastEvent.StartTime = reader.GetDateTime(6);

                        pastEvent.Fights.Add(fight);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            return pastEvents;
        }

        public async Task<int> AddEvent(Event newEvent)
        {

            int newEventId = 0;

            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"INSERT INTO tblEvent (name, starttime, endtime)
                                            VALUES (@name, @starttime, @endtime)
                                            RETURNING id", conn);
                cmd.Parameters.AddWithValue("@name", newEvent.Name);
                cmd.Parameters.AddWithValue("@starttime", newEvent.StartTime);
                cmd.Parameters.AddWithValue("@endtime", newEvent.StartTime.AddHours(3));

                newEventId = (int) await cmd.ExecuteScalarAsync();
                await conn.CloseAsync();

            }
            catch (Exception ex)
            {
               
                Console.WriteLine(ex.ToString());
            }

            return newEventId;
        }

        public async Task<List<int>> AddFights(List<Fight> fights, int eventId)
        {
            List<int> newFightIds = new List<int>();

            try
            {
                foreach (var fight in fights)
                {
                    using var conn = new NpgsqlConnection(_connString);
                    await conn.OpenAsync();

                    using var cmd = new NpgsqlCommand(@"INSERT INTO tblFight (fighter1name, fighter1odds, fighter2name, fighter2odds, eventid, winner)
                                            VALUES (@fighter1name, @fighter1odds, @fighter2name, @fighter2odds, @eventid, @winner)
                                            RETURNING id", conn);
                    cmd.Parameters.AddWithValue("@fighter1name", fight.Fighter1Name.Trim());
                    cmd.Parameters.AddWithValue("@fighter1odds", fight.Fighter1Points);
                    cmd.Parameters.AddWithValue("@fighter2name", fight.Fighter2Name.Trim());
                    cmd.Parameters.AddWithValue("@fighter2odds", fight.Fighter2Points);
                    cmd.Parameters.AddWithValue("@eventid", eventId);
                    cmd.Parameters.AddWithValue("@winner", string.Empty);

                    var newFightId = (int)await cmd.ExecuteScalarAsync();
                    await conn.CloseAsync();

                    newFightIds.Add(newFightId);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

            return newFightIds;
        }

        public async Task AddEmptyUserPicks(List<int> fightIds)
        {
            List<Player> players = await GetPlayersAsync();

            try
            {
                foreach (var player in players)
                {
                    foreach (var fightid in fightIds)
                    {
                        using var conn = new NpgsqlConnection(_connString);
                        await conn.OpenAsync();

                        using var cmd = new NpgsqlCommand(@"INSERT INTO tblUserPicks (userid, fightd, name)
                                                VALUES (@userId, @fightId, @name)"
                                                , conn);
                        cmd.Parameters.AddWithValue("@userId", player.Id);
                        cmd.Parameters.AddWithValue("@fightId", fightid);
                        cmd.Parameters.AddWithValue("@name", "TBD");

                        await cmd.ExecuteScalarAsync();
                        await conn.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        
        }

        public async Task UpdateEvent(Event e) 
        {
            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"UPDATE tblEvent 
                                                    SET name = @eventname,
                                                        starttime = @starttime
                                                    WHERE id = @eventid", conn);
                cmd.Parameters.AddWithValue("@eventname", e.Name);
                cmd.Parameters.AddWithValue("@starttime", e.StartTime);
                cmd.Parameters.AddWithValue("@eventid", e.Id);

                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task UpdateFights(List<Fight> fights)
        {
            foreach (Fight f in fights)
            {
                await UpdateFight(f);
            }
        }

        public async Task UpdateFight(Fight f)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"UPDATE tblFight 
                                                    SET fighter1name = @fighter1name,
                                                        fighter2name = @fighter2name,
	                                                    fighter1odds = @fighter1odds,
	                                                    fighter2odds = @fighter2odds,
	                                                    winner = @winner
                                                    WHERE id = @fightid", conn);

                cmd.Parameters.AddWithValue("@fighter1name", f.Fighter1Name.Trim());
                cmd.Parameters.AddWithValue("@fighter2name", f.Fighter2Name.Trim());
                cmd.Parameters.AddWithValue("@fighter1odds", f.Fighter1Points);
                cmd.Parameters.AddWithValue("@fighter2odds", f.Fighter2Points);
                cmd.Parameters.AddWithValue("@winner", f.Winner);
                cmd.Parameters.AddWithValue("@fightid", f.Id);

                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task UpdateAllUsersPicks(List<UserPicks>? allUserPicks)
        {
            foreach (UserPicks userPicks in allUserPicks)
            {
                await UpdateUserPicks(userPicks);
            }
        }


        public async Task UpdateUserPicks(UserPicks userPicks)
        {
            try
            {
                foreach (UserPick pick in userPicks.Picks)
                {
                    using var conn = new NpgsqlConnection(_connString);
                    await conn.OpenAsync();

                    using var cmd = new NpgsqlCommand(@"UPDATE tblUserPicks 
                                                        SET name = @pickname
                                                        WHERE fightd = @fightid 
                                                            AND userid = @userId", conn);
                    cmd.Parameters.AddWithValue("@pickname", pick.Pick.Trim());
                    cmd.Parameters.AddWithValue("@fightid", pick.FightId);
                    cmd.Parameters.AddWithValue("@userId", userPicks.UserId);

                    await cmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<List<UserPicks>> GetUserPicks()
        {

            List<Player> players = await GetPlayersAsync();
            List<UserPicks> userPicksList = new List<UserPicks>();

            try
            {
                foreach (var player in players)
                {
                    using var conn = new NpgsqlConnection(_connString);
                    await conn.OpenAsync();

                    using var cmd = new NpgsqlCommand(@"SELECT fightd, name 
                                                        FROM tblUserPicks 
                                                        WHERE userId = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", player.Id);
                    using var reader = await cmd.ExecuteReaderAsync();

                    UserPicks userPicks = new UserPicks();
                    userPicks.UserId = player.Id;
                    userPicks.Name = player.Name;   

                    while (await reader.ReadAsync())
                    {
                        var newPick = new UserPick();
                        newPick.FightId = reader.GetInt32(0);
                        newPick.Pick = reader.GetString(1);
 
                        userPicks.Picks.Add(newPick);
                    }

                    userPicksList.Add(userPicks);
                }               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            return userPicksList;
        }
    }
}