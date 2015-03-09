using System;
using System.Runtime.InteropServices;

namespace RTDServer
{
    [Guid("EC0E6191-DB51-11D3-8F3E-00C04F3651B8")]
    public interface IRtdServer
    {
        int ServerStart(IRTDUpdateEvent callback);

        object ConnectData(int topicId,
            [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array strings,
            ref bool newValues);

        [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
        Array RefreshData(ref int topicCount);

        void DisconnectData(int topicId);

        int Heartbeat();

        void ServerTerminate();
    }

    [Guid("A43788C1-D91B-11D3-8F39-00C04F3651B8")]
    public interface IRTDUpdateEvent
    {
        void UpdateNotify();

        int HeartbeatInterval { get; set; }

        void Disconnect();
    }
}