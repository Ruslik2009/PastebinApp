using System.Threading.Tasks;
using Npgsql;
using Npgsql.Replication;
using Tmds.DBus.Protocol;
using System.ComponentModel;
using System;
using System.Collections.Generic;

using Microsoft.Data.Sqlite;

using PastebinApp;

public class DBServiceForPostgreSql
{
    private NpgsqlConnection connection;

    public DBServiceForPostgreSql(string connectionString)
    {
        connection = new NpgsqlConnection(connectionString);
    }

    // Методы для users таблицы
    public async Task AddUserAsync(string username, string password) 
    { 
        try
        {
            await connection.OpenAsync();
            using NpgsqlCommand command = new NpgsqlCommand("Insert Into users (username, password) " 
                                                           +"Values(@username, @password)", connection);

            string privatePassword = HashHelper.GenerateSha256(password);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", privatePassword);

            await command.ExecuteNonQueryAsync();
        }
        finally { await connection.CloseAsync(); }

        
    }


    public async Task<bool> VerificationUserAsync(string username, string password )
    {
        try
        {
            await connection.OpenAsync();
            using NpgsqlCommand command = new NpgsqlCommand("Select password "
                                                + "from users " 
                                                + "where username = @username", connection);
            

            command.Parameters.AddWithValue("@username", username);
            var pass = await command.ExecuteScalarAsync();

            if (pass == null ) { return false; }
            
            return (HashHelper.GenerateSha256(password) == pass.ToString())? true : false;
            
        }
        finally { await connection.CloseAsync(); }
    }

    public async Task<int?> SearchUserAsync(string username)
    {
        try
        {
            await connection.OpenAsync();
            using NpgsqlCommand command = new NpgsqlCommand("Select id "
                                                + "from users " 
                                                + "where username = @username", connection);
            

            command.Parameters.AddWithValue("@username", username);
            var pass = await command.ExecuteScalarAsync();

            if (pass == null ) { return null; }
            return (int)pass;
        }
        finally { await connection.CloseAsync(); }
    }




    // Метод для pastes таблицы  

    public async Task AddPasteAsync(int user_id, string content, bool is_public, DateTime? delete_at, string url )
    { 

        try
        {
            await connection.OpenAsync();
            using NpgsqlCommand command = new NpgsqlCommand("Insert Into posts (user_id, content, is_public, delete_at, url) " 
                                                           +"Values(@user_id, @content, @is_public, @delete_at, @url)", connection);

            command.Parameters.AddWithValue("@user_id", user_id);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@is_public", is_public);
            command.Parameters.AddWithValue("@delete_at", delete_at.HasValue ? (object)delete_at.Value : DBNull.Value);
            command.Parameters.AddWithValue("@url", url);

            await command.ExecuteNonQueryAsync();
        }
        finally { await connection.CloseAsync(); }
    }

    
    public async Task<List<PostDisplay>> GetPostsAsync()
    { 

        var posts = new List<PostDisplay>();
        
        try
        {
            await connection.OpenAsync();
            
            using var deleteCmd = new NpgsqlCommand("DELETE FROM posts WHERE delete_at IS NOT NULL AND delete_at <= CURRENT_TIMESTAMP", connection);
            await deleteCmd.ExecuteNonQueryAsync();

            using var command = new NpgsqlCommand(   "SELECT p.id, u.username, p.content, p.created, p.likes, p.dislikes "
                                               + "FROM posts p "
                                               + "INNER JOIN users u ON p.user_id = u.id "
                                               + "WHERE p.is_public = true " 
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


    public async Task<PostDisplay?> SearchPasteAsync(string url )
    { 
        try
        {
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(   "SELECT u.username, p.content, p.created " 
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
                    Username = reader.GetString(0),
                    Content = reader.GetString(1),
                    Created = reader.GetDateTime(2)
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
            using var transaction = await connection.BeginTransactionAsync();
    
            // Проверяем существующую реакцию
            using var checkCmd = new NpgsqlCommand("SELECT reaction_type FROM post_reactions WHERE post_id = @post_id AND user_id = @user_id", connection, transaction);
            checkCmd.Parameters.AddWithValue("@post_id", postId);
            checkCmd.Parameters.AddWithValue("@user_id", userId);
            var existingReaction = await checkCmd.ExecuteScalarAsync();
            Console.WriteLine($"Существующая реакция: {existingReaction}");
    
            // Обновляем реакцию
            if (existingReaction == null || existingReaction == DBNull.Value)
            {
                using var insertCmd = new NpgsqlCommand("INSERT INTO post_reactions (post_id, user_id, reaction_type) VALUES (@post_id, @user_id, @reaction_type)", connection, transaction);
                insertCmd.Parameters.AddWithValue("@post_id", postId);
                insertCmd.Parameters.AddWithValue("@user_id", userId);
                insertCmd.Parameters.AddWithValue("@reaction_type", reactionType);
                await insertCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"✓ Реакция добавлена: type={reactionType}");
            }
            else if ((int)existingReaction == reactionType)
            {
                using var deleteCmd = new NpgsqlCommand("DELETE FROM post_reactions WHERE post_id = @post_id AND user_id = @user_id", connection, transaction);
                deleteCmd.Parameters.AddWithValue("@post_id", postId);
                deleteCmd.Parameters.AddWithValue("@user_id", userId);
                await deleteCmd.ExecuteNonQueryAsync();
                reactionType = 0;
                Console.WriteLine($"✓ Реакция удалена");
            } 
            else
            {
                using var updateCmd = new NpgsqlCommand("UPDATE post_reactions SET reaction_type = @reaction_type WHERE post_id = @post_id AND user_id = @user_id", connection, transaction);
                updateCmd.Parameters.AddWithValue("@post_id", postId);
                updateCmd.Parameters.AddWithValue("@user_id", userId);
                updateCmd.Parameters.AddWithValue("@reaction_type", reactionType);
                await updateCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"✓ Реакция обновлена: type={reactionType}");
            }
    
            // Считаем актуальные счётчики
            int likes, dislikes;
            using (var statsCmd = new NpgsqlCommand(@"SELECT 
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
            Console.WriteLine($"Подсчитано: Likes={likes}, Dislikes={dislikes}");
    
            // Обновляем таблицу posts
            using var updatePostCmd = new NpgsqlCommand("UPDATE posts SET likes = @likes, dislikes = @dislikes WHERE id = @post_id", connection, transaction);
            updatePostCmd.Parameters.AddWithValue("@likes", likes);
            updatePostCmd.Parameters.AddWithValue("@dislikes", dislikes);
            updatePostCmd.Parameters.AddWithValue("@post_id", postId);
            int rowsAffected = await updatePostCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"UPDATE posts: затронуто строк = {rowsAffected}");
    
            await transaction.CommitAsync();
            Console.WriteLine($"✓ Транзакция успешно закоммичена!");
        
            return (likes, dislikes, reactionType == 0 ? null : reactionType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ ОШИБКА: {ex.Message}");
            Console.WriteLine($"✗ StackTrace: {ex.StackTrace}");
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

                using var cmd = new NpgsqlCommand(@"SELECT p.likes, p.dislikes, (SELECT reaction_type FROM post_reactions 
                                                    WHERE post_id = p.id AND user_id = @userId) as user_reaction
                                                    FROM posts p
                                                    WHERE p.id = @postId", connection);

            cmd.Parameters.AddWithValue("@postId", postId);
            cmd.Parameters.AddWithValue("@userId", userId);

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

}