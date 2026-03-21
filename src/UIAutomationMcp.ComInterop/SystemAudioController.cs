using System.Runtime.InteropServices;

namespace UIAutomationMcp.ComInterop;

internal static class SystemAudioController
{
    public static UiAutomationAudioResult GetState()
    {
        using var endpointVolume = CreateEndpointVolume();
        var muted = endpointVolume.Interface.GetMute();
        return CreateResult("status", muted, muted ? "System audio is muted." : "System audio is unmuted.");
    }

    public static UiAutomationAudioResult SetMute(bool muted)
    {
        using var endpointVolume = CreateEndpointVolume();
        SetMuteChecked(endpointVolume.Interface, muted, Guid.Empty);
        var current = endpointVolume.Interface.GetMute();
        return CreateResult(muted ? "mute" : "unmute", current, current ? "System audio is muted." : "System audio is unmuted.");
    }

    public static UiAutomationAudioResult ToggleMute()
    {
        using var endpointVolume = CreateEndpointVolume();
        var target = !endpointVolume.Interface.GetMute();
        SetMuteChecked(endpointVolume.Interface, target, Guid.Empty);
        var current = endpointVolume.Interface.GetMute();
        return CreateResult("toggle-mute", current, current ? "System audio is muted." : "System audio is unmuted.");
    }

    private static UiAutomationAudioResult CreateResult(string action, bool muted, string message) => new()
    {
        Action = action,
        Muted = muted,
        Message = message
    };

    private static EndpointVolumeHandle CreateEndpointVolume()
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? endpoint = null;
        IAudioEndpointVolume? volume = null;

        try
        {
            enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
            endpoint = enumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Multimedia);
            volume = endpoint.Activate<IAudioEndpointVolume>(ClsCtx.InprocServer);
            return new EndpointVolumeHandle(volume, endpoint, enumerator);
        }
        catch
        {
            ReleaseComObject(volume);
            ReleaseComObject(endpoint);
            ReleaseComObject(enumerator);
            throw;
        }
    }

    private static void ReleaseComObject(object? instance)
    {
        if (instance is not null && Marshal.IsComObject(instance))
        {
            Marshal.FinalReleaseComObject(instance);
        }
    }

    private sealed class EndpointVolumeHandle(IAudioEndpointVolume volume, IMMDevice endpoint, IMMDeviceEnumerator enumerator) : IDisposable
    {
        public IAudioEndpointVolume Interface { get; } = volume;

        public void Dispose()
        {
            ReleaseComObject(Interface);
            ReleaseComObject(endpoint);
            ReleaseComObject(enumerator);
        }
    }

    private enum EDataFlow
    {
        Render,
        Capture,
        All
    }

    private enum ERole
    {
        Console,
        Multimedia,
        Communications
    }

    [Flags]
    private enum ClsCtx : uint
    {
        InprocServer = 0x1
    }

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private sealed class MMDeviceEnumeratorComObject;

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int NotImpl1();

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, ClsCtx clsCtx, nint activationParams, [MarshalAs(UnmanagedType.Interface)] out object instance);
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(nint notify);

        int UnregisterControlChangeNotify(nint notify);

        int GetChannelCount(out uint channelCount);

        int SetMasterVolumeLevel(float levelDb, Guid eventContext);

        int SetMasterVolumeLevelScalar(float level, Guid eventContext);

        int GetMasterVolumeLevel(out float levelDb);

        int GetMasterVolumeLevelScalar(out float level);

        int SetChannelVolumeLevel(uint channelNumber, float levelDb, Guid eventContext);

        int SetChannelVolumeLevelScalar(uint channelNumber, float level, Guid eventContext);

        int GetChannelVolumeLevel(uint channelNumber, out float levelDb);

        int GetChannelVolumeLevelScalar(uint channelNumber, out float level);

        [PreserveSig]
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool muted, Guid eventContext);

        [PreserveSig]
        int GetMute(out bool muted);

        int GetVolumeStepInfo(out uint step, out uint stepCount);

        int VolumeStepUp(Guid eventContext);

        int VolumeStepDown(Guid eventContext);

        int QueryHardwareSupport(out uint hardwareSupportMask);

        int GetVolumeRange(out float volumeMindB, out float volumeMaxdB, out float volumeIncrementdB);
    }

    private static IMMDevice GetDefaultAudioEndpoint(this IMMDeviceEnumerator enumerator, EDataFlow dataFlow, ERole role)
    {
        Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(dataFlow, role, out var endpoint));
        return endpoint;
    }

    private static T Activate<T>(this IMMDevice endpoint, ClsCtx clsCtx)
        where T : class
    {
        var iid = typeof(T).GUID;
        Marshal.ThrowExceptionForHR(endpoint.Activate(ref iid, clsCtx, nint.Zero, out var instance));
        return (T)instance;
    }

    private static bool GetMute(this IAudioEndpointVolume endpointVolume)
    {
        Marshal.ThrowExceptionForHR(endpointVolume.GetMute(out var muted));
        return muted;
    }

    private static void SetMuteChecked(IAudioEndpointVolume endpointVolume, bool muted, Guid eventContext)
    {
        Marshal.ThrowExceptionForHR(endpointVolume.SetMute(muted, eventContext));
    }
}
