using SQLite;

namespace VLOGusick.Models;

[Table("Clips")]
public class ClipEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Повний шлях до .mp4 файлу на диску
    public string FilePath { get; set; }

    // Час початку запису у форматі "hh:mm" — відображається на відео
    public string RecordedAt { get; set; }

    // Для сортування кліпів у правильному хронологічному порядку
    public DateTime CreatedAt { get; set; }
}
