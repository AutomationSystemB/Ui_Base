using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace HamburgerMenuApp
{
    interface _iFormUpdate
    {
        void WriteAlarms(string text);
        void WriteUserInfo(string text);
        void WriteComunicationInfo(string text);
        void WriteActStep(string text);
        void WriteStepHistory(string text);

        void SetComunicationImage(Image img);
        void SetUserInfoImage(Image img);

        void DebugInfo(bool value);
        void StartButton(bool value);
        void StopButton(bool value);
        void HomePosButton(bool value);
        
    }
}
