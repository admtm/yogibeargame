using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Persistence
{
    public interface IYogiGameDataAccess
    {
        Task<IYogiBoard> LoadAsync(string path);
        Task SaveAsync(string path, IYogiBoard board, int collectedBasketCount);
    }
}
