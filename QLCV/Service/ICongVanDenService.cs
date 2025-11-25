using QLCV.Models;
using WebApp.Models;
using WebApp.ModelUI;

namespace WebApp.Services
{
    public interface ICongVanDenService
    {
        Task<PagedResult<CongVanDetailModel>> GetAllAsync(string keyword,int page, int pageSize);
        Task<congVan?> GetByIdAsync(int id);
        Task CreateAsync(CongVanCreateModel model);
        Task UpdateAsync(int id, CongVanUpdateModel model);
        Task DeleteAsync(int id);
        Task<(string FilePath, string ExtractedText)> UploadFileAsync(IFormFile? file);
        Task<byte[]> DownloadAsync(int id);
        Task SendDocumentCountUpdate();
        Task<List<NoiPhatHanh>> GetNoiPhatHanhAsync();
        Task<string> ExtractTextFromFileAsync(string filePath);
    }
}
