using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.OS;

namespace Terraria.Testing;

public class WindowsPerformanceDiagnostics
{
	public struct Data
	{
		public double? ProcessorThrottlePercent;

		public double? MainThreadCPUPercent;

		public double ExpectedCPUPercent;

		public long? ContentionQueueLength;

		public int CurrentProcessor;

		public bool PinnedToProcessor;

		public bool RecommendUnpinning;
	}

	private struct PDH_FMT_COUNTERVALUE
	{
		public int CStatus;

		public double doubleValue;
	}

	private static readonly uint PDH_FMT_DOUBLE = 512u;

	private static Thread _monitorThread;

	private static object _lock = new object();

	private static Data _data;

	private static readonly float ContentionPerfDropThreshold = 0.8f;

	private static readonly int ConsecutiveContentionChecksBeforeUnpin = 20;

	private static int _unpinHintCount = 0;

	private static IntPtr queryHandle = IntPtr.Zero;

	private static IntPtr processorPerformanceHandle = IntPtr.Zero;

	private static IntPtr processorQueueLengthHandle = IntPtr.Zero;

	private static IntPtr threadProcessorTimeHandle = IntPtr.Zero;

	private static IntPtr threadProcessIdHandle = IntPtr.Zero;

	private static readonly string ProcessName = Process.GetCurrentProcess().ProcessName;

	private static readonly int PID = Process.GetCurrentProcess().Id;

	private static int ProcessCopyNumber = 0;

	private static int MonitoringCoreNumber = 0;

	public static bool Supported => Platform.IsWindows;

	[DllImport("Kernel32.dll")]
	private static extern int GetCurrentProcessorNumber();

	[DllImport("Pdh.dll", SetLastError = true)]
	private static extern int PdhOpenQuery(IntPtr dataSource, IntPtr userData, out IntPtr query);

	[DllImport("Pdh.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int PdhAddCounter(IntPtr query, string counterPath, IntPtr userData, out IntPtr counter);

	[DllImport("Pdh.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int PdhRemoveCounter(IntPtr counter);

	[DllImport("Pdh.dll", SetLastError = true)]
	private static extern int PdhCollectQueryData(IntPtr query);

	[DllImport("Pdh.dll", SetLastError = true)]
	private static extern int PdhGetFormattedCounterValue(IntPtr counter, uint format, out uint type, out PDH_FMT_COUNTERVALUE value);

	[DllImport("Pdh.dll", SetLastError = true)]
	private static extern int PdhCloseQuery(IntPtr query);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentThread();

	[DllImport("kernel32.dll")]
	private static extern bool SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

	[DllImport("kernel32.dll")]
	private static extern uint SetThreadIdealProcessor(IntPtr hThread, uint dwIdealProcessor);

	public static Data GetData()
	{
		lock (_lock)
		{
			if (_monitorThread == null)
			{
				_data.PinnedToProcessor = true;
				_data.CurrentProcessor = GetCurrentProcessorNumber();
				_monitorThread = new Thread(MonitorPerformanceCounters)
				{
					IsBackground = true,
					Name = "Perf Counter Monitoring"
				};
				_monitorThread.Start();
			}
			else
			{
				int currentProcessorNumber = GetCurrentProcessorNumber();
				if (_data.CurrentProcessor != currentProcessorNumber)
				{
					_data.PinnedToProcessor = false;
				}
				_data.CurrentProcessor = currentProcessorNumber;
			}
			_data.ExpectedCPUPercent = DetailedFPS.GetCPUUtilization(60) * 100f;
			return _data;
		}
	}

	private static bool ShouldRecommendUnpinning()
	{
		if (Environment.ProcessorCount < 4)
		{
			return false;
		}
		if (_data.PinnedToProcessor && _data.CurrentProcessor == 0 && _data.ContentionQueueLength > 0 && _data.MainThreadCPUPercent < _data.ExpectedCPUPercent * (double)ContentionPerfDropThreshold)
		{
			_unpinHintCount++;
		}
		else
		{
			_unpinHintCount = 0;
		}
		return _unpinHintCount >= ConsecutiveContentionChecksBeforeUnpin;
	}

	public static void UnpinFromCore0()
	{
		int allProcMask = (1 << Environment.ProcessorCount) - 1;
		Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(allProcMask ^ 1);
		Task.Factory.StartNew(delegate
		{
			Thread.Sleep(100);
			Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)allProcMask;
		});
	}

	private static string GetMainThreadCounterIdentifier()
	{
		return string.Format("\\Thread({0}/0{1})", ProcessName, (ProcessCopyNumber == 0) ? "" : ("#" + ProcessCopyNumber));
	}

	private static bool AddCounter(string name, ref IntPtr handle)
	{
		if (handle != IntPtr.Zero)
		{
			PdhRemoveCounter(handle);
		}
		return PdhAddCounter(queryHandle, name, IntPtr.Zero, out handle) != 0;
	}

	private static bool ReadCounter(IntPtr handle, out double value)
	{
		if (PdhGetFormattedCounterValue(handle, PDH_FMT_DOUBLE, out var _, out var value2) == 0)
		{
			value = value2.doubleValue;
			return true;
		}
		value = 0.0;
		return false;
	}

	private static void ReadCounter(IntPtr handle, out double? value)
	{
		if (handle != IntPtr.Zero && ReadCounter(handle, out double value2))
		{
			value = value2;
		}
		else
		{
			value = null;
		}
	}

	private static void MonitorPerformanceCounters()
	{
		if (PdhOpenQuery(IntPtr.Zero, IntPtr.Zero, out queryHandle) != 0)
		{
			return;
		}
		AddCounter("\\System\\Processor Queue Length", ref processorQueueLengthHandle);
		RecreateCoreCounters();
		RecreateThreadCounters();
		while (true)
		{
			int num = (_data.PinnedToProcessor ? _data.CurrentProcessor : (-1));
			if (num != MonitoringCoreNumber)
			{
				MonitoringCoreNumber = num;
				RecreateCoreCounters();
			}
			Thread.Sleep(250);
			PdhCollectQueryData(queryHandle);
			ReadCounter(processorPerformanceHandle, out double? value);
			ReadCounter(processorQueueLengthHandle, out double? value2);
			if (!ReadCounter(threadProcessIdHandle, out double value3))
			{
				ProcessCopyNumber = 0;
				RecreateThreadCounters();
				continue;
			}
			if (value3 != (double)PID)
			{
				ProcessCopyNumber++;
				RecreateThreadCounters();
				continue;
			}
			ReadCounter(threadProcessorTimeHandle, out double? value4);
			lock (_lock)
			{
				_data.ProcessorThrottlePercent = value;
				_data.ContentionQueueLength = (value2.HasValue ? new long?((long)value2.Value) : ((long?)null));
				LowPassUpdate(ref _data.MainThreadCPUPercent, value4, 0.25f);
				_data.RecommendUnpinning = ShouldRecommendUnpinning();
			}
		}
	}

	private static void LowPassUpdate(ref double? filtered, double? newValue, float rate)
	{
		if (!filtered.HasValue || !newValue.HasValue)
		{
			filtered = newValue;
		}
		else
		{
			filtered = filtered * (double)(1f - rate) + newValue * (double)rate;
		}
	}

	private static void RecreateCoreCounters()
	{
		string text = ((MonitoringCoreNumber < 0) ? "_Total" : MonitoringCoreNumber.ToString());
		AddCounter("\\Processor Information(0," + text + ")\\% Processor Performance", ref processorPerformanceHandle);
	}

	private static void RecreateThreadCounters()
	{
		AddCounter(GetMainThreadCounterIdentifier() + "\\% Processor Time", ref threadProcessorTimeHandle);
		AddCounter(GetMainThreadCounterIdentifier() + "\\ID Process", ref threadProcessIdHandle);
	}
}
