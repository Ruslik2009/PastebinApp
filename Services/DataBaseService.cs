using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using System.ComponentModel;
using System;
using System.Collections.Generic;

//using Npgsql;
//using Npgsql.Replication;
using Microsoft.Data.Sqlite;

using PastebinApp;

public class DatabaseService
{
    private SqliteConnection connection;

    public DatabaseService(string connectionString)
    {
        connection = new SqliteConnection(connectionString);
    }

    // Методы для users таблицы
    public async Task AddUserAsync(string username, string password)
    {
        try
        {
            await connection.OpenAsync();

            using SqliteCommand command = new SqliteCommand("INSERT INTO users (username, password) "
                                                           + "VALUES (@username, @password)", connection);

            string privatePassword = HashHelper.GenerateSha256(password);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", privatePassword);

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }




    public async Task<bool> VerificationUserAsync(string username, string password)
    {
        try
        {
            await connection.OpenAsync();

            using SqliteCommand command = new SqliteCommand("SELECT password "
                                                + "FROM users "
                                                + "WHERE username = @username", connection);

            command.Parameters.AddWithValue("@username", username);

            var pass = await command.ExecuteScalarAsync();

            if (pass == null) { return false; }


            return HashHelper.GenerateSha256(password) == pass.ToString();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }



    public async Task<int?> SearchUserAsync(string username)
    {
        try
        {
            await connection.OpenAsync();
            using SqliteCommand command = new SqliteCommand("SELECT id "
                                                + "FROM users "
                                                + "WHERE username = @username", connection);

            command.Parameters.AddWithValue("@username", username);
            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value) { return null; }


            return Convert.ToInt32(result);
        }
        finally { await connection.CloseAsync(); }
    }




    // Методы для получения информации о пользователе
    public async Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        try
        {
            await connection.OpenAsync();

            using var cmd = new SqliteCommand(@"SELECT id, username, created 
                                                FROM users 
                                                WHERE id = @userId", connection);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserProfile
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                };
            }

            return null;
        }
        finally { await connection.CloseAsync(); }
    }


    public async Task<List<PostDisplay>> GetUserPostsAsync(int userId)
    {
        var posts = new List<PostDisplay>();

        try
        {
            await connection.OpenAsync();

            using var command = new SqliteCommand(@"SELECT p.id, u.username, p.content, p.created, p.likes, p.dislikes 
                                                    FROM posts p 
                                                    INNER JOIN users u ON p.user_id = u.id 
                                                    WHERE p.user_id = @userId 
                                                    AND (p.delete_at IS NULL OR p.delete_at > CURRENT_TIMESTAMP) 
                                                    ORDER BY p.created DESC", connection);

            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                posts.Add(new PostDisplay
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Content = reader.GetString(2),
                    Created = reader.GetDateTime(3),
                    LikesCount = reader.GetInt32(4),
                    DislikesCount = reader.GetInt32(5)
                });
            }

            return posts;
        }
        finally { await connection.CloseAsync(); }
    }


    public async Task<(int totalLikes, int totalDislikes)> GetUserReactionsAsync(int userId)
    {
        try
        {
            await connection.OpenAsync();

            using var cmd = new SqliteCommand(@"SELECT 
                                                    COUNT(CASE WHEN reaction_type = 1 THEN 1 END) as total_likes,
                                                    COUNT(CASE WHEN reaction_type = 2 THEN 1 END) as total_dislikes
                                                FROM post_reactions pr
                                                INNER JOIN posts p ON pr.post_id = p.id
                                                WHERE p.user_id = @userId", connection);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int totalLikes = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                int totalDislikes = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                return (totalLikes, totalDislikes);
            }

            return (0, 0);
        }
        finally { await connection.CloseAsync(); }
    }
    // Метод для pastes таблицы  

    public async Task AddPasteAsync(int user_id, string content, bool is_public, DateTime? delete_at, string url)
    {
        try
        {
            await connection.OpenAsync();
            // Меняем команду на SqliteCommand
            using SqliteCommand command = new SqliteCommand("INSERT INTO posts (user_id, content, is_public, delete_at, url) "
                                                           + "VALUES (@user_id, @content, @is_public, @delete_at, @url)", connection);

            command.Parameters.AddWithValue("@user_id", user_id);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@is_public", is_public);
            command.Parameters.AddWithValue("@delete_at", delete_at.HasValue ? (object)delete_at.Value : DBNull.Value);
            command.Parameters.AddWithValue("@url", url);

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }



    public async Task<List<PostDisplay>> GetPostsAsync()
    {
        var posts = new List<PostDisplay>();

        try
        {
            await connection.OpenAsync();


            using var deleteCmd = new SqliteCommand("DELETE FROM posts WHERE delete_at IS NOT NULL AND delete_at <= CURRENT_TIMESTAMP", connection);
            await deleteCmd.ExecuteNonQueryAsync();


            using var command = new SqliteCommand("SELECT p.id, u.username, p.content, p.created, p.likes, p.dislikes "
                                               + "FROM posts p "
                                               + "INNER JOIN users u ON p.user_id = u.id "
                                               + "WHERE p.is_public = 1 "
                                               + "AND (p.delete_at IS NULL OR p.delete_at > CURRENT_TIMESTAMP) "
                                               + "ORDER BY p.created DESC", connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                posts.Add(new PostDisplay
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Content = reader.GetString(2),
                    Created = reader.GetDateTime(3),
                    LikesCount = reader.GetInt32(4),
                    DislikesCount = reader.GetInt32(5)
                });
            }
            return posts;
        }
        finally { await connection.CloseAsync(); }
    }



    public async Task<PostDisplay?> SearchPasteAsync(string url)
    {
        try
        {
            await connection.OpenAsync();

            using var command = new SqliteCommand("SELECT p.id, u.username, p.content, p.created, p.likes, p.dislikes  "
                                                   + "FROM posts p "
                                                   + "INNER JOIN users u ON p.user_id = u.id "
                                                   + "WHERE p.url = @url "
                                                   + "AND (p.delete_at IS NULL OR p.delete_at > CURRENT_TIMESTAMP)", connection);

            command.Parameters.AddWithValue("@url", url);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new PostDisplay
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Content = reader.GetString(2),
                    Created = reader.GetDateTime(3),
                    LikesCount = reader.GetInt32(4),
                    DislikesCount = reader.GetInt32(5)
                };
            }

            return null;
        }
        finally { await connection.CloseAsync(); }
    }



    public async Task<(int likes, int dislikes, int? userReaction)> ToggleReactionAsync(int postId, int userId, int reactionType)
    {
        try
        {
            await connection.OpenAsync();

            using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

            // Проверяем существующую реакцию
            using var checkCmd = new SqliteCommand("SELECT reaction_type FROM post_reactions WHERE post_id = @post_id AND user_id = @user_id", connection, transaction);
            checkCmd.Parameters.AddWithValue("@post_id", postId);
            checkCmd.Parameters.AddWithValue("@user_id", userId);
            var existingReaction = await checkCmd.ExecuteScalarAsync();

            // Обновляем реакцию
            if (existingReaction == null || existingReaction == DBNull.Value)
            {
                using var insertCmd = new SqliteCommand("INSERT INTO post_reactions (post_id, user_id, reaction_type) VALUES (@post_id, @user_id, @reaction_type)", connection, transaction);
                insertCmd.Parameters.AddWithValue("@post_id", postId);
                insertCmd.Parameters.AddWithValue("@user_id", userId);
                insertCmd.Parameters.AddWithValue("@reaction_type", reactionType);
                await insertCmd.ExecuteNonQueryAsync();
            }
            else if (Convert.ToInt32(existingReaction) == reactionType)
            {
                using var deleteCmd = new SqliteCommand("DELETE FROM post_reactions WHERE post_id = @post_id AND user_id = @user_id", connection, transaction);
                deleteCmd.Parameters.AddWithValue("@post_id", postId);
                deleteCmd.Parameters.AddWithValue("@user_id", userId);
                await deleteCmd.ExecuteNonQueryAsync();
                reactionType = 0;
            }
            else
            {
                using var updateCmd = new SqliteCommand("UPDATE post_reactions SET reaction_type = @reaction_type WHERE post_id = @post_id AND user_id = @user_id", connection, transaction);
                updateCmd.Parameters.AddWithValue("@post_id", postId);
                updateCmd.Parameters.AddWithValue("@user_id", userId);
                updateCmd.Parameters.AddWithValue("@reaction_type", reactionType);
                await updateCmd.ExecuteNonQueryAsync();
            }

            // Считаем актуальные счётчики
            int likes, dislikes;
            using (var statsCmd = new SqliteCommand(@"SELECT 
                                                    COUNT(CASE WHEN reaction_type = 1 THEN 1 END) as likes,
                                                    COUNT(CASE WHEN reaction_type = 2 THEN 1 END) as dislikes
                                                    FROM post_reactions
                                                    WHERE post_id = @post_id", connection, transaction))
            {
                statsCmd.Parameters.AddWithValue("@post_id", postId);
                using (var reader = await statsCmd.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    likes = reader.GetInt32(0);
                    dislikes = reader.GetInt32(1);
                }
            }

            // Обновляем таблицу posts
            using var updatePostCmd = new SqliteCommand("UPDATE posts SET likes = @likes, dislikes = @dislikes WHERE id = @post_id", connection, transaction);
            updatePostCmd.Parameters.AddWithValue("@likes", likes);
            updatePostCmd.Parameters.AddWithValue("@dislikes", dislikes);
            updatePostCmd.Parameters.AddWithValue("@post_id", postId);
            await updatePostCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return (likes, dislikes, reactionType == 0 ? null : reactionType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ ОШИБКА: {ex.Message}");
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }


    public async Task<(int? userReaction, int likes, int dislikes)> GetPostStatsAsync(int? postId, int? userId)
    {
        try
        {
            await connection.OpenAsync();

            using var cmd = new SqliteCommand(@"SELECT p.likes, p.dislikes, 
                                                (SELECT reaction_type FROM post_reactions 
                                                 WHERE post_id = p.id AND user_id = @userId) as user_reaction
                                                FROM posts p
                                                WHERE p.id = @postId", connection);


            cmd.Parameters.AddWithValue("@postId", postId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@userId", userId ?? (object)DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int likes = reader.GetInt32(0);
                int dislikes = reader.GetInt32(1);
                int? userReaction = reader.IsDBNull(2) ? null : reader.GetInt32(2);

                return (userReaction, likes, dislikes);
            }

            return (null, 0, 0);
        }
        finally { await connection.CloseAsync(); }
    }

    public async Task<bool> DeletePostAsync(int postId, int userId)
    {
        try
        {
            await connection.OpenAsync();

            // удаляем реакции на этот пост
            using (var cmd = new SqliteCommand(@"DELETE FROM post_reactions
                                                WHERE post_id = @postId", connection))
            {
                cmd.Parameters.AddWithValue("@postId", postId);
                await cmd.ExecuteNonQueryAsync();
            }

            // удаляем пост
            using (var cmd = new SqliteCommand(@"DELETE FROM posts
                                                WHERE id = @postId AND user_id = @userId", connection))
            {
                cmd.Parameters.AddWithValue("@postId", postId);
                cmd.Parameters.AddWithValue("@userId", userId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        finally { await connection.CloseAsync(); }
    }


}