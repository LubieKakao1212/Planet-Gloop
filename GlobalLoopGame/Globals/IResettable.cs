using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Globals
{
    public interface IResettable
    {
        void OnGameEnd();

        void Reset();
    }
}
