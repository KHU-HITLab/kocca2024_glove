using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using DI = System.Diagnostics;
using System.IO;
using UnityEngine;

public class HapticGlove : IHapticGlove
{
    private int listenPort;

    private Mutex mtx = new Mutex();
    private volatile bool running = true;

    private float[] values = new float[5];
    private float[] commands = new float[5];

    public HapticGlove()
    {
        // acquire available ephemeral port for udp communication
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listenPort = ((IPEndPoint)socket.LocalEndPoint).Port;
        }

        // Run middleware executable
        RunMiddlewareExecutable(listenPort);

        // Connect with middleware
        new Thread(() =>
        {
            byte[] buf_tx = new byte[5];
            byte[] buf_rx = new byte[5];

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

            Thread.Sleep(1000);

            try
            {
                while (running)
                {
                    mtx.WaitOne();
                    for (int i = 0; i < 3; i++)
                    {
                        buf_tx[i] = (byte)(commands[i] * 255);
                    }
                    mtx.ReleaseMutex();

                    EndPoint server_ep = new IPEndPoint(IPAddress.Loopback, listenPort);
                    socket.SendTo(buf_tx, server_ep);

                    try
                    {
                        socket.Receive(buf_rx);
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode == SocketError.TimedOut)
                        {
                            continue;
                        }
                        else
                        {
                            //throw e;
                            if (isTracking)
                            {
                                isTracking = false;
                                OnDeviceTrackingChanged?.Invoke(false);

                            }
                        }
                    }

                    if (!isTracking)
                    {
                        isTracking = true;
                        OnDeviceTrackingChanged?.Invoke(true);
                    }

                    mtx.WaitOne();
                    for (int i = 0; i < 3; i++)
                    {
                        values[i] = (float)Math.Round(buf_rx[i] / 255f, 3);
                    }
                    mtx.ReleaseMutex();

                    Thread.Sleep(10);
                }
            }
            finally
            {
                if (socket != null)
                {
                    socket.Close();
                }
            }
        }).Start();
    }

    ~HapticGlove()
    {
        Dispose();
    }

    public void Dispose()
    {
        running = false;
        StopMiddlewareExecutable();
    }

    private DI.Process proc = null;
    private void RunMiddlewareExecutable(int listenPort)
    {
        string executablePath = Path.Combine(Application.streamingAssetsPath, "glove_middleware.exe");

        DI.ProcessStartInfo startInfo = new DI.ProcessStartInfo
        {
            FileName = executablePath, // 실행 파일의 경로
            Arguments = listenPort.ToString(), // 실행 파일에 전달할 인수
            WorkingDirectory = Application.streamingAssetsPath, // 실행 파일의 작업 디렉터리
            UseShellExecute = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
        };

        proc = DI.Process.Start(startInfo);


    }

    private void StopMiddlewareExecutable()
    {
        foreach (var p in DI.Process.GetProcessesByName("glove_middleware"))
        {
            p.Kill();
        }
    }

    public void ExecuteForceHaptic(ForceHapticData data)
    {
        // 오른손만 지원
        if (data.isLeft)
        {
            throw new NotImplementedException();
        }

        mtx.WaitOne();
        commands[data.fingerType] = data.bendValue;
        mtx.ReleaseMutex();
    }

    public void StopAllForceHaptic()
    {
        mtx.WaitOne();
        for (int i = 0; i < 5; i++)
        {
            commands[i] = 0f;
        }
        mtx.ReleaseMutex();
    }

    private bool isTracking = false;
    private Action<bool> m_OnValueChanged;
    public Action<bool> OnDeviceTrackingChanged
    {
        get
        {
            return m_OnValueChanged;
        }
        set
        {
            m_OnValueChanged = value;
        }
    }

    public HandData GetHandData(bool isLeft)
    {
        // 오른손만 지원
        if (isLeft)
        {
            throw new NotImplementedException();
        }

        var data = new HandData();
        data.isLeft = false;
        data.isTracked = isTracking;

        data.fingerBendValue = new float[5];

        mtx.WaitOne();
        for (int i = 0; i < 5; i++)
        {
            data.fingerBendValue[i] = values[i];
        }
        mtx.ReleaseMutex();

        return data;
    }

    public void ExecuteVibrationHaptic(VibrationHapticData data)
    {
        throw new NotImplementedException();
    }

    public void StopAllVibrationHaptic()
    {
        throw new NotImplementedException();
    }
}
