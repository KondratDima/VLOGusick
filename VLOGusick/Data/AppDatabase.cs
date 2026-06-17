using SQLite;
using VLOGusick.Models;

namespace VLOGusick.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection _db;

    // Створює підключення і таблицю Clips, якщо вони ще не існують.
    // Викликається перед будь-якою операцією з БД — повторні виклики безпечні (нічого не робить, якщо вже існує).
    public async Task Init()
    {
        if (_db is not null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vlog.db3");
        _db = new SQLiteAsyncConnection(dbPath);
        await _db.CreateTableAsync<ClipEntry>();
    }

    // Повертає всі кліпи, відсортовані за часом створення (від найстарішого до найновішого)
    public async Task<List<ClipEntry>> GetAllAsync()
    {
        await Init();
        return await _db.Table<ClipEntry>()
                        .OrderBy(c => c.CreatedAt)
                        .ToListAsync();
    }

    // Повертає один кліп за Id — потрібно для ClipPreviewPage (Етап 5)
    public async Task<ClipEntry> GetByIdAsync(int id)
    {
        await Init();
        return await _db.Table<ClipEntry>()
                        .Where(c => c.Id == id)
                        .FirstOrDefaultAsync();
    }

    // Додає новий запис у БД. Id присвоюється автоматично (AutoIncrement).
    public async Task SaveAsync(ClipEntry clip)
    {
        await Init();
        await _db.InsertAsync(clip);
    }

    // Видаляє запис з БД за його Id
    public async Task DeleteAsync(ClipEntry clip)
    {
        await Init();
        await _db.DeleteAsync(clip);
    }
}
