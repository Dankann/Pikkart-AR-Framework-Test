using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace PikkartAR {

	public class ColorUtils {

        private static byte[] bytes = null;

        public static byte[] Color32ArrayToByteArray(Color32[] colors)
		{
			if (colors == null || colors.Length == 0)
				return null;
                
/*#if UNITY_EDITOR_OSX
			int l = Constants.PROCESS_WIDTH * Constants.PROCESS_HEIGHT * 4;
            if (bytes == null || bytes.Length != l) bytes = new byte[l];
			for (int r = 0; r < Constants.PROCESS_HEIGHT; ++r) 
			{
				for (int c = 0; c < Constants.PROCESS_WIDTH; ++c)
				{
					int idx1 = (r * Constants.PROCESS_WIDTH + c) * 4;
					int idx2 = ((int)((r/2.0f)*3.0f) * Constants.OS_CAM_RES_WIDTH + 160 + (int)((c/2.0f)*3.0f));
					bytes[idx1] = colors[idx2].r;
					bytes[idx1 + 1] = colors[idx2].g;
					bytes[idx1 + 2] = colors[idx2].b;
					bytes[idx1 + 3] = 255;
				}
			}
#else*/
            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
			int length = lengthOfColor32 * colors.Length;
            if (bytes == null || bytes.Length != length) bytes = new byte[length];
			
			GCHandle handle = default(GCHandle);
			try
			{
				handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
				IntPtr ptr = handle.AddrOfPinnedObject();
				Marshal.Copy(ptr, bytes, 0, length);
			}
			finally
			{
				if (handle != default(GCHandle))
					handle.Free();
			}
//#endif
			
			return bytes;
		}

        public static Color32[] GetColorArray(byte[] rgbData)
		{
			Color32[] colorArray = new Color32[rgbData.Length/4];
			for(var i = 0; i < rgbData.Length; i+=4)
			{
				Color32 color = new Color32(rgbData[i + 0], rgbData[i + 1], rgbData[i + 2],rgbData[i + 3]);
				colorArray[i/4] = color;
			}
			
			return colorArray;
		}
	}
}
