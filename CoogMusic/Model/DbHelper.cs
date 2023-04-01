﻿using System;
using MySql.Data.MySqlClient;
using System.Data;
using Azure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Reflection.PortableExecutable;

namespace CoogMusic.Pages
{
	public class DbHelper
    {
        String connectionString = "Server=coogmusic.mysql.database.azure.com;User ID=qalksktvpv;Password=coogmusic1!;Database=coogmusicdb";
        public async Task CreateLogin(ApplicationUser user, String password, String userType)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    MySqlTransaction mySqlTransaction = conn.BeginTransaction();
                    String sql = "INSERT INTO login (email, passwrd) VALUES (@Email, @Password)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", password);
                        int affectedRows = await cmd.ExecuteNonQueryAsync();
                    }

                    mySqlTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR Writing to Database: " + ex.ToString());
            }
        }

        public async Task CreateUser(ApplicationUser user, String userType)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    MySqlTransaction mySqlTransaction = conn.BeginTransaction();
                    
                    String sql = "INSERT INTO users (name, email, mobile, sex, age) VALUES (@Name, @Email, @Mobile, @Sex, @Age)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", user.Name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Mobile", user.Mobile);
                        cmd.Parameters.AddWithValue("@Sex", user.Sex);
                        cmd.Parameters.AddWithValue("@Age", user.Age);
                        int affectedRows = await cmd.ExecuteNonQueryAsync();
                    }
                    mySqlTransaction.Commit();
                }
            }
            catch(Exception ex)
            {
                Console.Write("ERROR Writing to Database: " + ex.ToString());
            }
        }

        public async Task CreateArtistOrListener(ApplicationUser user, String userType)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    MySqlTransaction mySqlTransaction = conn.BeginTransaction();
                    String sql = "SELECT id FROM users WHERE email=@Email";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user.Id = reader.GetInt32("id");
                            }
                        }
                    }
                        if (userType == "Artist")
                        {
                            sql = "INSERT INTO artist (user_id, name, record_label) VALUES (@UserId, @Name, @RecordLabel)";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@Name", user.Name);
                                cmd.Parameters.AddWithValue("@RecordLabel", user.recordLabel);
                                cmd.Parameters.AddWithValue("@UserId", user.Id);
                                int affectedRows = await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            sql = "INSERT INTO listener (name, id) VALUES (@Name, @UserId)";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@Name", user.Name);
                                cmd.Parameters.AddWithValue("@UserId", user.Email);
                                int affectedRows = await cmd.ExecuteNonQueryAsync();
                            }
                        }

                    mySqlTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR Writing to Database: " + ex.ToString());
            }
        }

        public async Task<ApplicationUser> GetUserByEmailAndPassword(String email, String pwd)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                String sql = "SELECT * FROM login AS l, users AS u WHERE l.email=u.email AND l.email=@Email AND l.passwrd=@Password";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", pwd);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ApplicationUser
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Email = reader.GetString("email"),
                                Mobile = reader.IsDBNull("mobile") ? null : reader.GetString("mobile"),
                                CreateDate = reader.GetDateTime("created_at"),
                                Sex = reader.GetChar("sex"),
                                Age = reader.GetInt32("age")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> IsUserArtist(int userId)
        {
            int count = 0;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                String sql = "SELECT COUNT(*) FROM artist WHERE user_id=@UserId;";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    count = int.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            return count > 0;
        }

        public async Task<bool> IsUserListener(int userId)
        {
            int count = 0;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                String sql = "SELECT COUNT(*) FROM listener WHERE id=@UserId;";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    count = int.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            return count > 0;
        }

        public async Task<bool> EmailExists(string email)
        {
            int count = 0;
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                String sql = "SELECT COUNT(*) FROM login WHERE email=@Email;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);

                    count = int.Parse(cmd.ExecuteScalar().ToString());
                    return count > 0;
                }
            }
        }
    }
}

