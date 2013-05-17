using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Media;

using Virtual.Interface.Golf.Models;

namespace Virtual.Interface.Golf.DataAccess
{
    public class DbConnection
    {

        #region Private Members

        private static int _currentRoundID;
        private static int _currentRoundNum;

        #endregion 

        #region Database

        private static MySqlConnection createConnectionMySqlStats()
        {

            MySqlConnection cn = null;

            try
            {
                cn = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlStatsDb"].ConnectionString);
                cn.Open();
            }
            catch (MySqlException ex)
            {

            }            

            return cn;
        }
        
        #endregion

        #region Scoring

        public static Tournament GetTournament()
        {
            Tournament tournament = null;
            DataTable tbl = null;
            int i;

            MySqlConnection cn = null;

            try
            {
                using (cn = createConnectionMySqlStats())
                {
                    using (MySqlCommand cmd = new MySqlCommand("select *, (select roundid from rounds where roundnum = currentround) as currentroundid from tournament where tournamentid = 1", cn))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                tbl = new DataTable();
                                tbl.Load(rdr);
                                rdr.Close();

                                DataRow row = tbl.Rows[0];

                                tournament = new Tournament();
                                tournament.TournamentName = row["tournamentname"].ToString();
                                tournament.CurrentRound = int.TryParse(row["currentround"].ToString(), out i) ? i : 0;

                                _currentRoundNum = tournament.CurrentRound;
                                _currentRoundID = int.TryParse(row["currentroundid"].ToString(), out i) ? i : 0;

                                tournament.Event = GetEvent(cn, int.Parse(row["tournamentid"].ToString()));
                            }

                            
                        } //using rdrTourney
                    } //using cmdTourney

                    cn.Close();

                } // using cn
            }
            catch (Exception ex)
            { }
            finally
            { 
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return tournament;
        }

        public static Event GetEvent(MySqlConnection cn, int tournamentId)
        {
            Event golfEvent = null;
            DataTable tbl = null;
            int i;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand("select * from event where tournamentid = " + tournamentId, cn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            tbl = new DataTable();
                            tbl.Load(rdr);
                            rdr.Close();

                            DataRow row = tbl.Rows[0];

                            golfEvent = new Event();
                            golfEvent.EventID = int.TryParse(row["eventid"].ToString(), out i) ? i : 0;

                            golfEvent.Courses = GetCourses(cn, golfEvent.EventID);       
                        }

                        
                    } //using rdr
                } //using cmd

            }
            catch (Exception ex)
            { }
            finally
            { }

            return golfEvent;
        }

        public static List<Course> GetCourses(MySqlConnection cn, int eventId)
        {
            List<Course> courses = null;
            Course course = null;
            DataTable tbl = null;
            int i;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand("select * from course where eventid = " + eventId, cn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            courses = new List<Course>();

                            tbl = new DataTable();
                            tbl.Load(rdr);
                            rdr.Close();

                            foreach (DataRow row in tbl.Rows)
                            {
                                course = new Course();

                                course.CourseID = int.TryParse(row["courseid"].ToString(), out i) ? i : 0;
                                course.CourseName = row["coursename"].ToString();
                                course.CourseGpsLocation = row["coursegpslocation"].ToString();
                                course.CourseGpsHeading = double.Parse(row["coursegpsheading"].ToString());

                                course.Holes = GetHoles(cn, course.CourseID);

                                courses.Add(course);
                            }
                                    
                        }
                    } //using rdr
                } //using cmd

            }
            catch (Exception ex)
            { }
            finally
            { }

            return courses;
        }

        public static List<Round> GetRounds()
        {
            List<Round> rounds = null;
            Round round = null;
            DataTable tbl = null;
            int i;

            try
            {
                using (MySqlConnection cn = createConnectionMySqlStats())
                {
                    using (MySqlCommand cmd = new MySqlCommand("select * from rounds", cn))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                rounds = new List<Round>();

                                tbl = new DataTable();
                                tbl.Load(rdr);
                                rdr.Close();

                                foreach (DataRow row in tbl.Rows)
                                {
                                    round = new Round();

                                    round.RoundID = int.TryParse(row["roundid"].ToString(), out i) ? i : 0;
                                    round.RoundNum = int.TryParse(row["roundnum"].ToString(), out i) ? i : 0;
                                    round.Description = row["rounddesc"].ToString();

                                    rounds.Add(round);
                                }

                            }
                        } //using rdr
                    } //using cmd
                } //using cn
            }
            catch (Exception ex)
            { }
            finally
            { }

            return rounds;
        }

        public static List<Hole> GetHoles(MySqlConnection cn, int courseId)
        {
            List<Hole> holes = null;
            Hole hole = null;
            DataTable tbl = null;
            int i;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand("select * from holes where courseid = " + courseId, cn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            holes = new List<Hole>();

                            tbl = new DataTable();
                            tbl.Load(rdr);
                            rdr.Close();

                            foreach (DataRow row in tbl.Rows)
                            {
                                hole = new Hole();

                                hole.HoleID = int.TryParse(row["holeid"].ToString(), out i) ? i : 0;
                                hole.Par = int.TryParse(row["par"].ToString(), out i) ? i : 0;
                                hole.Yardage = int.TryParse(row["yardage"].ToString(), out i) ? i : 0;
                                hole.HoleNum = int.TryParse(row["holenum"].ToString(), out i) ? i : 0;
                                hole.TeeLocation = row["gpstee"].ToString();
                                hole.PinLocation = row["gpspin"].ToString();
                                hole.ApproachLocation = row["gpsapproach"].ToString();

                                holes.Add(hole);                                
                            }

                            //for (var round = 0; round <= _currentRoundNum; round++)
                            //{
                            //    GetHoleStatLine(ref holes, cn, round);
                            //}
                        }                        
                    } //using rdr
                } //using cmd

            }
            catch (Exception ex)
            { }
            finally
            { }

            return holes;
        }

        public static ObservableCollection<Player> GetPlayers(int? pairingId = null)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;
            Int16 number;
            int status;
            //bool parseResult;

            ObservableCollection<Player> players = null;

            //Stopwatch swAll = new Stopwatch();

            //swAll.Start();

            try
            {
                cn = createConnectionMySqlStats();

                if (cn != null)
                {
                    string sql = "";
                    string currentTimestamp = "";

                    try
                    {
                        sql = "select CURRENT_TIMESTAMP as currenttime from dual";
                        cmd = new MySqlCommand(sql, cn);
                        rdr = cmd.ExecuteReader();

                        tbl = new DataTable();

                        tbl.Load(rdr);
                        rdr.Close();
                        rdr.Dispose();

                        if (tbl.Rows.Count > 0)
                        {
                            currentTimestamp = tbl.Rows[0]["currenttime"].ToString();
                        }
                    }
                    finally
                    {
                        tbl.Dispose();
                        cmd.Dispose();
                    }
                    
                    sql = "select b.PlayerID as ID, a.*, b.*,"; // e.*, ";
                    sql += "(select count(*) from playerholes where playerid = b.playerid and roundid = (select roundid from rounds where roundnum = (select currentround from tournament)) and (score > 0 or score != null)) as ThruNum, ";
                    sql += "(select count(*) from playerholes where playerid = b.playerid and (score > 0 or score != null)) as TotalHolesPlayed, ";
                    sql += "(select starttime from pairings p join playerpairings pp on p.pairingid = pp.pairingid ";
                    sql += "where pp.playerid = b.playerid and p.roundid = (select roundid from rounds where roundnum = (select currentround from tournament))) as StartTime ";

                    sql += "from leaderboard a ";
                    sql += "right join players b on a.playerid = b.playerid ";
                    sql += "join countries c on b.countryid = c.countryid ";

                    //sql += "join playerpairings d on b.playerid = d.playerid ";
                    //sql += "join pairings e on d.pairingid = e.pairingid ";

                    //sql += "where e.roundid = (select currentround from tournament) ";

                    //if (pairingId != null)
                    //{
                    //    sql += "where e.pairingid = " + pairingId + " ";
                    //}

                    sql += "order by coalesce(rankorder, 99999) asc, priority, lastname, firstname";

                    cmd = new MySqlCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);
                    rdr.Close();
                    rdr.Dispose();

                    if (tbl.Rows.Count > 0)
                    {
                        players = new ObservableCollection<Player>();

                        foreach (DataRow row in tbl.Rows)
                        {
                            Player player = new Player();

                            player.PlayerID = Convert.ToInt32(row["ID"]);
                            player.FirstName = row["firstname"].ToString();
                            player.LastName = row["lastname"].ToString();
                            player.TvName = row["tvname"].ToString();
                            player.Headshot = new Uri(ConfigurationManager.AppSettings["HeadshotDirectory"].ToString() + "\\" + row["headshot"].ToString() + ".TGA");
                            player.Country = getCountry(Convert.ToInt16(row["countryid"]));
                            player.Status = int.TryParse(row["status"].ToString(), out status) ? status : 0;
                            player.Position = Int16.TryParse(row["rankorder"].ToString(), out number) ? number : 0;
                                                        
                            player.PositionStr = row["rank"].ToString();
                            player.DisplayPosition = row["rank"].ToString();

                            player.TotalScore = Int16.TryParse(row["totalscore"].ToString(), out number) ? number : 0;
                                                       
                            player.TotalScoreStr = row["total"].ToString();                      

                            //player.Shots = GetShots(player.PlayerID);

                            players.Add(player);
                        }
                        
                    }                    
                } 
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (rdr != null) rdr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            //swAll.Stop();

            //Debug.WriteLine("Got Players in: " + swAll.Elapsed);

            return players;
        }

        public static ObservableCollection<Pairing> GetPairings(List<Player> players, int roundID)
        {
            //Stopwatch sw = new Stopwatch();

            //sw.Start();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            ObservableCollection<Pairing> pairings = null;

            try
            {
                if (players != null & players.Count > 0)
                {
                    cn = createConnectionMySqlStats();

                    if (cn != null)
                    {
                        string sql = "select * from pairings a join rounds b on a.roundid = b.roundid where a.roundid = " + roundID + " order by a.starttime, a.starthole";

                        cmd = new MySqlCommand(sql, cn);
                        rdr = cmd.ExecuteReader();

                        tbl = new DataTable();

                        tbl.Load(rdr);
                        rdr.Close();
                        rdr.Dispose();

                        if (tbl.Rows.Count > 0)
                        {
                            pairings = new ObservableCollection<Pairing>();

                            foreach (DataRow row in tbl.Rows)
                            {
                                Pairing pairing = new Pairing();

                                pairing.PairingID = Convert.ToInt32(row["pairingid"]);
                                pairing.Round = Convert.ToInt16(row["roundnum"]);
                                pairing.TeeTime = Convert.ToDateTime(row["starttime"]);
                                pairing.FormattedTeeTime = Convert.ToDateTime(row["starttime"]).ToString("h:mm");
                                pairing.StartHole = Convert.ToInt16(row["starthole"]);
                                pairing.CourseID = Convert.ToInt16(row["courseid"]);
                                pairing.OnCourse = row["oncourse"].ToString();

                                pairing.Players = new List<Player>();

                                DataTable tblPlayers = new DataTable();

                                try
                                {
                                    sql = "select * from playerpairings where pairingid = " + row["pairingid"].ToString();
                                    cmd = new MySqlCommand(sql, cn);
                                    rdr = cmd.ExecuteReader();

                                    tblPlayers.Load(rdr);
                                    rdr.Close();
                                    rdr.Dispose();

                                    foreach (DataRow rowPlayer in tblPlayers.Rows)
                                    {
                                        pairing.Players.Add(players.FirstOrDefault(p => p.PlayerID == Convert.ToInt32(rowPlayer["playerid"])));
                                    }

                                    string playerNames = "";

                                    foreach (Player player in pairing.Players)
                                    {
                                        playerNames = playerNames + player.LastName + "/"; 
                                    }

                                    pairing.PlayerNames = playerNames.Substring(0, playerNames.Length - 1);

                                    //pairing.PlayerNames = pairing.Players.ToArray()
                                }
                                finally
                                {
                                    if (cmd != null) cmd.Dispose();
                                    if (tblPlayers != null) tblPlayers.Dispose();
                                }

                                //pairing.Players = GetPlayers(pairing.PairingID);                                  

                                pairings.Add(pairing);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (rdr != null) rdr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            //sw.Stop();

            //Debug.WriteLine("GetPairings: " + sw.Elapsed.Milliseconds);

            return pairings;
        }

        public static List<Shot> GetShots(int holeId, int shotNum, string playerId, List<Player> players, int roundNum = 0)
        {
            List<Shot> shots;

            using (MySqlConnection cn = createConnectionMySqlStats())
            {
                string sql = "select * from playershots a join clubs b on a.clubid = b.clubid join holes c on a.holeid = c.holeid join rounds d on a.roundid = d.roundid ";
                sql += "where a.shot = " + shotNum + " and a.holeid = " + holeId + " and a.playerid = " + playerId;

                if (roundNum > 0)
                {
                    sql += " and a.roundid = (select roundid from rounds where roundnum = " + roundNum + ")";
                }

                using (MySqlCommand cmd = new MySqlCommand(sql, cn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        DataTable tbl = new DataTable();

                        tbl.Load(rdr);                        
                        rdr.Close();

                        shots = new List<Shot>();

                        foreach (DataRow row in tbl.Rows)
                        {
                            Player player = players.SingleOrDefault(p => p.PlayerID.ToString() == row["playerid"].ToString());
                            shots.Add(createShot(row, player));
                        }
                    }
                }

                cn.Close();
            }

            return shots;
        }

        public static List<Shot> GetShots(int holeId, int shotNum, int toPar, List<Player> players, int roundNum = 0)
        {
            List<Shot> shots;

            using (MySqlConnection cn = createConnectionMySqlStats())
            {
                string sql = "select * from playershots a join clubs b on a.clubid = b.clubid join holes c on a.holeid = c.holeid join rounds d on a.roundid = d.roundid ";
                sql += "where a.shot = " + shotNum + " and a.holeid = " + holeId;

                if (toPar >= 2)
                {
                    sql += " and ((select score from playerholes where playerid = a.playerid and holeid = a.holeid and roundid = a.roundid) - c.par) >= " + toPar;
                }
                else
                {
                    sql += " and ((select score from playerholes where playerid = a.playerid and holeid = a.holeid and roundid = a.roundid) - c.par) = " + toPar;
                }
                
                if (roundNum > 0)
                {
                    sql += " and a.roundid = (select roundid from rounds where roundnum = " + roundNum + ")";
                }

                using (MySqlCommand cmd = new MySqlCommand(sql, cn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        DataTable tbl = new DataTable();

                        tbl.Load(rdr);
                        rdr.Close();

                        shots = new List<Shot>();

                        foreach (DataRow row in tbl.Rows)
                        {
                            Player player = players.SingleOrDefault(p => p.PlayerID.ToString() == row["playerid"].ToString());
                            shots.Add(createShot(row, player));
                        }
                    }
                }

                cn.Close();
            }

            return shots;
        }

        public static List<Shot> GetLongestDrive(int holeId, int shotNum, List<Player> players, int roundNum = 0)
        {
            List<Shot> shots;

            using (MySqlConnection cn = createConnectionMySqlStats())
            {
                string sql = "select * from playershots a join clubs b on a.clubid = b.clubid join holes c on a.holeid = c.holeid join rounds d on a.roundid = d.roundid ";
                sql += "where a.shot = " + shotNum + " and a.holeid = " + holeId + " ";

                if (roundNum > 0)
                {
                    sql += "and a.virtualdistance = (select max(virtualdistance) from playershots where roundid = (select roundid from rounds where roundnum = " + roundNum + ") and holeid = " + holeId + ")";
                }
                else
                {
                    sql += "and a.virtualdistance = (select max(virtualdistance) from playershots where holeid = " + holeId + ")";
                }

                if (roundNum > 0)
                {
                    sql += " and a.roundid = (select roundid from rounds where roundnum = " + roundNum + ")";
                }

                using (MySqlCommand cmd = new MySqlCommand(sql, cn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        DataTable tbl = new DataTable();

                        tbl.Load(rdr);
                        rdr.Close();

                        shots = new List<Shot>();

                        foreach (DataRow row in tbl.Rows)
                        {
                            Player player = players.SingleOrDefault(p => p.PlayerID.ToString() == row["playerid"].ToString());
                            shots.Add(createShot(row, player));
                        }
                    }
                }

                cn.Close();
            }

            return shots;
        }

        private static Shot createShot(DataRow row, Player player)
        {
            int i;

            Shot shot = new Shot();

            Round round = new Round();
            round.RoundID = int.Parse(row["roundid"].ToString());
            round.Description = row["rounddesc"].ToString();
            round.RoundNum = int.Parse(row["roundnum"].ToString());

            Hole hole = new Hole();
            hole.HoleID = int.Parse(row["holeid"].ToString());
            hole.HoleNum = int.Parse(row["holenum"].ToString());
            hole.Par = int.Parse(row["par"].ToString());

            shot.Player = player;
            shot.Round = round;
            shot.Hole = hole;
            shot.ShotNum = int.Parse(row["shot"].ToString());
            shot.ShotLocation = row["gpslocation"].ToString();
            shot.DistanceTraveled = int.TryParse(row["distancetraveled"].ToString(), out i) ? i : 0;
            shot.DistanceToPin = int.TryParse(row["distancetopin"].ToString(), out i) ? i : 0;
            shot.VirtualLocation = row["virtuallocation"].ToString();
            shot.VirtualDistance = row["virtualdistance"].ToString();

            return shot;
        }

        private static Country getCountry(int countryId)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            Country country = null;

            try
            {
                cn = createConnectionMySqlStats();

                string sql = "select * from countries where countryid = " + countryId;

                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                if (tbl.Rows.Count == 1)
                {
                    DataRow row = tbl.Rows[0];
                    country = new Country();
                    country.CountryID = countryId;
                    country.CountryName = row["country"].ToString();
                    country.Abbrev = row["abbrev"].ToString();
                    //country.Flag = new Uri(ConfigurationManager.AppSettings["FlagFolder"].ToString() + "\\" + row["vizflag"].ToString() + ".png");
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (rdr != null) rdr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return country;
        }

        #endregion

    }
}
