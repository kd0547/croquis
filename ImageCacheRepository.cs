using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public  class ImageCacheRepository
{
    private readonly string path = @"image_cache.db";

    public ImageCacheRepository()
    {

    }

    public SQLiteConnection CreateConnection(string connectionString)
    {
        SQLiteConnection connection = new SQLiteConnection(connectionString);
        connection.Open();
        return connection;
    }

    public void ExecuteNonQuery(SQLiteConnection connection, string query)
    {
        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    public DataTable ExecuteQuery(SQLiteConnection connection, string query)
    {
        DataTable dataTable = new DataTable();
        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                dataTable.Load(reader);
            }
        }
        return dataTable;
    }

    public void UpdateImageCache(ImageCache imageCache)
    {
        string updateQuery = @"
        UPDATE ImageCache 
        SET 
            ImageName = COALESCE(@ImageName, ImageName), 
            IsFlip = COALESCE(@IsFlip, IsFlip),
            IsFlipXY = COALESCE(@IsFlipXY, IsFlipXY),
            IsGrayscale = COALESCE(@IsGrayscale, IsGrayscale),
            Angle = COALESCE(@Angle, Angle)
            WHERE ImagePath = @ImagePath";

        using (var command = new SQLiteCommand(updateQuery, CreateConnection(path)))
        {
            command.Parameters.AddWithValue("@ImageName", (object)imageCache.ImageName ?? DBNull.Value);
            command.Parameters.AddWithValue("@ImagePath", imageCache.ImagePath);
            command.Parameters.AddWithValue("@IsFlip", imageCache.IsFlip != null ? (imageCache.IsFlip ? 1 : 0) : (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsFlipXY", imageCache.IsFlipXY != null ? (imageCache.IsFlipXY ? 1 : 0) : (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsGrayscale", imageCache.IsGrayscale != null ? (imageCache.IsGrayscale ? 1 : 0) : (object)DBNull.Value);
            command.Parameters.AddWithValue("@Angle", imageCache.Angle != null ? (int)imageCache.Angle : (object)DBNull.Value);

            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// ImageCache 테이블을 생성한다. 
    /// </summary>
    public void CreateImageCacheTable()
    {
        string createTableQuery = @"
            CREATE TABLE ImageCache (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                ImageName TEXT UNIQUE,
                ImagePath TEXT UNIQUE,  
                IsFlip INTEGER DEFAULT 0,    
                IsFlipXY INTEGER DEFAULT 0,
                IsGrayscale INTEGER DEFAULT 0,
                Angle INTEGER DEFAULT 0      
        )";


        using (SQLiteConnection connection = CreateConnection(path))
        {
            bool exists = TableExists(connection, "ImageCache");
            if (!exists)
            {
                ExecuteNonQuery(connection, createTableQuery);
            }
        }
    }

    //나중에 분리하기 
    public void InsertImageCache(ImageCache imageCache)
    {
        string insertQuery = @"
        INSERT INTO ImageCache (ImageName, ImagePath, IsFlip, IsFlipXY, IsGrayscale, Angle)
        VALUES (@ImageName, @ImagePath, @IsFlip, @IsFlipXY, @IsGrayscale, @Angle)";

        using (SQLiteCommand command = new SQLiteCommand(insertQuery, CreateConnection(path)))
        {
            command.Parameters.AddWithValue("@ImageName", imageCache.ImageName);
            command.Parameters.AddWithValue("@ImagePath", imageCache.ImagePath);
            command.Parameters.AddWithValue("@IsFlip", imageCache.IsFlip ? 1 : 0); // Convert boolean to integer
            command.Parameters.AddWithValue("@IsFlipXY", imageCache.IsFlipXY ? 1 : 0); // Convert boolean to integer
            command.Parameters.AddWithValue("@IsGrayscale", imageCache.IsGrayscale ? 1 : 0); // Convert boolean to integer
            command.Parameters.AddWithValue("@Angle", (int)imageCache.Angle);

            command.ExecuteNonQuery();
        }
    }



    public ImageCache FindByImageName(string imageName)
    {
        string findFileName = @"SELECT * FROM ImageCache WHERE ImageName = @ImageName";

        DataTable result = ExecuteQuery(CreateConnection(path), findFileName);

        if (result.Rows.Count > 0)
        {
            DataRow row = result.Rows[0]; // 첫 번째 행에 직접 접근

            ImageCache imageCache = new ImageCache
            {
                ImageName = row["ImageName"].ToString(),
                ImagePath = row["ImagePath"].ToString(),
                IsFlip = Convert.ToBoolean(row["IsFlip"]),
                IsFlipXY = Convert.ToBoolean(row["IsFlipXY"]),
                IsGrayscale = Convert.ToBoolean(row["IsGrayscale"]),
                Angle = double.Parse(row["Angle"].ToString())
        };

            return imageCache;
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public bool TableExists(SQLiteConnection connection, string tableName)
    {
        using (SQLiteCommand command = new SQLiteCommand(connection))
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName";
            command.Parameters.AddWithValue("@TableName", tableName);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                return reader.HasRows;
            }
        }
    }
}
