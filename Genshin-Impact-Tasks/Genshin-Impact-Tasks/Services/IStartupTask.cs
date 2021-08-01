using System.Threading.Tasks;

namespace Genshin_Impact_Tasks.Services
{
    public interface IStartupTask
    {
        // 자동 시작 활성화
        Task<bool> Enable();

        // 자동 시작 비활성화
        Task Disable();

        // 자동 시작 상태 가져오기
        Task<bool> GetState();
    }
}
