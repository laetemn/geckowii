using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

using D2XXDirect;

namespace FTDIUSBGecko
{
    public interface IGeckoDevice
    {
        FT_STATUS OpenBySerialNumber(String serial);

        FT_STATUS GetNumberOfDevices(ref UInt32 numberOfDevices);

        FT_STATUS SetTimeouts(UInt32 readTimeout, UInt32 writeTimeout);

        FT_STATUS SetLatencyTimer(Byte ucTimer);

        FT_STATUS InTransferSize(UInt32 transfer);

        FT_STATUS Close();

        FT_STATUS ResetDevice();

        FT_STATUS Purge(UInt32 eventCh);

        FT_STATUS Read(Byte[] buffer, UInt32 nobytes, ref UInt32 bytes_read);

        FT_STATUS Write(Byte[] buffer, Int32 nobytes, ref UInt32 bytes_written);
    }
}
