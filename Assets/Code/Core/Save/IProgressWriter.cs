
namespace Code.Infrastructure.Save
{
    public interface IProgressWriter : IProgressReader
    {
        void SaveProgress(PlayerProgressData playerProgress);
    }
}