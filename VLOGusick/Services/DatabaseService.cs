using VLOGusick.Data;
using VLOGusick.Models;

namespace VLOGusick.Services;

// Сервіс-обгортка над AppDatabase.
// ViewModels звертаються сюди, а не напряму до AppDatabase —
// якщо в майбутньому БД зміниться (наприклад на іншу СУБД), правити доведеться лише цей файл.
public class DatabaseService
{
    private readonly AppDatabase _database;

    public DatabaseService(AppDatabase database)
    {
        _database = database;
    }

    public Task<List<ClipEntry>> GetAllAsync()
        => _database.GetAllAsync();

    public Task<ClipEntry> GetByIdAsync(int id)
        => _database.GetByIdAsync(id);

    public Task SaveAsync(ClipEntry clip)
        => _database.SaveAsync(clip);

    public Task DeleteAsync(ClipEntry clip)
        => _database.DeleteAsync(clip);
}

