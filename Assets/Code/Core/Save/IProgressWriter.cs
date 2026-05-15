
namespace Code.Core.Save
{
    public interface IProgressWriter : IProgressReader
    {
        void SaveProgress(PlayerProgressData playerProgress);
    }
}