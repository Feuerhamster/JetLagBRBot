using JetLagBRBot.Models;
using LiteDB;

namespace JetLagBRBot.Services;

public interface IDatabaseService
{
    public void InsertImageCache(ImageCache img);
    public ImageCache? GetImageCache(string fileName);
}

public class DatabaseService : IDatabaseService
{
    private readonly LiteDatabase db;
    
    private readonly ILiteCollection<ImageCache> imageCache;
    
    public DatabaseService(IConfiguration config)
    {
        string dbPath = config.GetValue<string>("DatabasePath");
        this.db = new LiteDatabase(dbPath);
        
        this.imageCache = this.db.GetCollection<ImageCache>("image_cache");
    }

    public void InsertImageCache(ImageCache img)
    {
        this.imageCache.Insert(img);
    }

    public ImageCache? GetImageCache(string fileName)
    {
        return this.imageCache.FindOne(f => f.FileName == fileName);
    }
}