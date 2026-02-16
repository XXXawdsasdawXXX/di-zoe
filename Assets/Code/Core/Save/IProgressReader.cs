using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Save
{
    public interface IProgressReader
    {
        UniTask LoadProgress(PlayerProgressData playerProgress);
    }
}