using VLOGusick.Models;

namespace VLOGusick.Services;

public interface IVideoRecordService
{
    Task<ClipEntry> RecordAsync();
}
