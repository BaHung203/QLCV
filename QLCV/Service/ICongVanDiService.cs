using WebApp.Models;
using WebApp.ModelUI;

namespace WebApp.Services
{
    public interface ICongVanDiService
    {
        Task<PagedResult<CongVanDetailModel>> GetAllAsync(string keyword,int page, int pageSize);
        Task<congVan?> GetByIdAsync(int id);
        Task CreateAsync(CongVanCreateModel model);
        Task UpdateAsync(int id, CongVanCreateModel model);
        Task DeleteAsync(int id);
        Task<(string filePath, string extractedText)> UploadFileAsync(IFormFile? file);
        Task<byte[]> DownloadAsync(int id);
        Task<List<NoiNhan>> GetNoiNhanAsync();
        Task SendDocumentCountUpdate();
        Task<string> ExtractTextFromFileAsync(string filePath);

    }
}
