using System.Runtime.InteropServices;

namespace Odyssey.Networking
{
	public static class Protocol
	{
		public static T Deserialise<T>(ReadOnlySpan<byte> bytes)
		{
			var len = bytes.Length;
			var buffer = Marshal.AllocHGlobal(len);
			Marshal.Copy(bytes.ToArray(), 0, buffer, len); // copy to the alloc'd buffer

			var tObj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return tObj;
		}

		public static ReadOnlySpan<byte> Serialise<T>(T tObj)
		{
			unsafe
			{
				var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(tObj));
				Marshal.StructureToPtr(tObj, ptr, false);
				//Marshal.Copy(ptr, arr, 0, len);
				//Marshal.FreeHGlobal(ptr);

				return new ReadOnlySpan<byte>(ptr.ToPointer(), Marshal.SizeOf(tObj));
			}
		}

		//public static T Deserialise<T>(ReadOnlySpan<byte> bytes) where T : struct => MemoryMarshal.Cast<byte, T>(bytes)[0];//var len = bytes.Length;//var buffer = Marshal.AllocHGlobal(len);//Marshal.Copy(bytes.ToArray(), 0, buffer, len); // copy to the alloc'd buffer"////var tObj = (T)Marshal.PtrToStructure(buffer, typeof(T));//var tObj = MemoryMarshal.Cast<byte, T>(bytes);//Marshal.FreeHGlobal(buffer);//return tObj;

		//public static ReadOnlySpan<byte> Serialise<T>(T tObj) where T : struct
		//{
		//	var json = JsonSerializer.Serialize(tObj);
		//	//var bf = new BinaryFormatter();
		//	//var binary = bf.Serialize()


		//	unsafe
		//	{
		//		var size = Marshal.SizeOf(tObj);
		//		var ptr = Marshal.AllocHGlobal(size);
		//		Marshal.StructureToPtr(tObj, ptr, false);
		//		//Marshal.Copy(ptr, arr, 0, len);
		//		//Marshal.FreeHGlobal(ptr);

		//		return new ReadOnlySpan<byte>(ptr.ToPointer(), size);
		//	}
		//}
	}
}
