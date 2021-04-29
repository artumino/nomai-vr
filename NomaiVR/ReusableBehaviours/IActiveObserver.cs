using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NomaiVR
{
    interface IActiveObserver
    {
        bool IsActive { get; }
        event Action OnActivate;
        event Action OnDeactivate;
    }
}
