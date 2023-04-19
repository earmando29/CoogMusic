using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CoogMusic.Data;
using System.Security.Claims;

namespace CoogMusic.Pages.Report
{
    public class Listener_ReportModel : PageModel
    {
        public List<AlbumInfo> Albums { get; set; }
        public int SelectedAlbumId { get; set; }
        public string SelectedAlbumTitle { get; set; }
        public string ReportHtml { get; set; }
        public int ArtistId;
        private readonly DbHelper _dbHelper;
        private readonly string connectionStr;


        private readonly IConfiguration _configuration;

        public Listener_ReportModel(IConfiguration configuration)
        {
            
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            connectionStr = connectionString;
            _dbHelper = new DbHelper(connectionString);

        }

        public async Task OnGetAsync()
        {
            ArtistId = await _dbHelper.GetArtistIdByUserId(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            using (var connection = new MySqlConnection(connectionStr))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT id,artist_id,title FROM album WHERE artist_id = @ArtistId", connection))
                {
                    command.Parameters.AddWithValue("@ArtistId", ArtistId);


                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        Albums = new List<AlbumInfo>();
                        while (await reader.ReadAsync())
                        {
                            Albums.Add(new AlbumInfo
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Title = reader.GetString(reader.GetOrdinal("title")),
                                ArtistId = reader.GetInt32(reader.GetOrdinal("artist_id"))

                            });

                        }
                    }
                }

            }
        }

        public async Task<string> GenerateListenerReport(int selectedAlbumId)
        {


            
            using (var connection = new MySqlConnection(connectionStr))
            using (var command = new MySqlCommand())
            {
                SelectedAlbumId = selectedAlbumId;

                command.Connection = connection;
                command.CommandText = "SELECT song.title, COUNT(streams.song_id) AS stream_count FROM song JOIN album_song ON song.id = album_song.song_id JOIN album ON album_song.album_id = album.id JOIN artist ON album.artist_id = artist.artist_id LEFT JOIN streams ON song.id = streams.song_id WHERE album.id = @AlbumId GROUP BY song.id, song.title";

                command.Parameters.AddWithValue("@AlbumId", selectedAlbumId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    StringBuilder html = new StringBuilder();
                    html.Append("<table>");
                    html.Append("<tr><th>Song Title</th><th>Streamed</th></tr>");

                    int totalAlbumStreams = 0;

                    while (reader.Read())
                    {
                        string songTitle = reader.GetString("title");
                        int streamCount = reader.GetInt32("stream_count");
                        totalAlbumStreams += streamCount;

                        html.Append("<tr>");
                        html.Append("<td>" + songTitle + "</td>");
                        html.Append("<td>" + streamCount.ToString() + "</td>");
                        html.Append("</tr>");
                    }

                    html.Append("<tr><td>Total album streams:</td><td>" + totalAlbumStreams.ToString() + "</td></tr>");

                    html.Append("</table>");

                    // Initialize Albums list before accessing it
                    await OnGetAsync();

                    SelectedAlbumTitle = Albums.FirstOrDefault(a => a.Id == selectedAlbumId)?.Title ?? "";
                    ReportHtml = html.ToString();

                    return ReportHtml;
                }
            }
        }


        public async Task<IActionResult> OnPostGenerateReportAsync(int selectedAlbumId)
        {
            await GenerateListenerReport(selectedAlbumId);
            return Page();
        }

    }



}
