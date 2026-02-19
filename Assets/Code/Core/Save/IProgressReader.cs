using Code.Core.Save.SavedData;
using Cysharp.Threading.Tasks;

namespace Code.Core.Save
{
    public interface IProgressReader
    {
        UniTask LoadProgress(PlayerProgressData playerProgress);
    }
}