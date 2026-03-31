using System;

namespace Code.Core.ServiceLocator
{
    public interface IAssemblyInstaller
    {
        public int Order { get; }
        Type[] GetServiceOrder();
    }
}