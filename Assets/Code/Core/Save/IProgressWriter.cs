
using Code.Core.Save.SavedData;

namespace Code.Core.Save
{
    public interface IProgressWriter : IProgressReader
    {
        void SaveProgress(PlayerProgressData playerProgress);
    }
}