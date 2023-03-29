using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace CoogMusic.Pages.Search
{
    public class IndexModel : PageModel
    {
        public List<SongInfo> listSongs = new List<SongInfo>();
        public void OnGet()
        {
            try
            {
                String connectionStr = "Server=coogmusic.mysql.database.azure.com;User ID=qalksktvpv;Password=coogmusic1!;Database=coogmusicdb";
                using (MySqlConnection connection = new MySqlConnection(connectionStr))
                {
                    connection.Open();
                    String sql = "SELECT * FROM song AS S";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SongInfo songInfo = new SongInfo();
                                songInfo.title = reader.GetString(2);
                                songInfo.genre = reader.GetString(3);
                                songInfo.CreateDate = reader.GetDateTime(6).ToString();

                                listSongs.Add(songInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnPostAsync()
        {

        }
    }
    public class SongInfo
    {
        public String RecordLabel;
        public String CreateDate;
        public String Name;
        public int songId;
        public int userId;
        public int? artistId;
        public String artist;
        public String genre;
        public String title;
        public IFormFile songFile;
    }
}
