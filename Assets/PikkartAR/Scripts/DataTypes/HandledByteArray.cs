using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace PikkartAR {
		
	public class HandledByteArray {

        public const int MAX_BUFFER_SIZE = 2073600; //1228800; //640*480*4
		
		private byte[] array;
		private GCHandle handler;
		
		private static Color32[] data;
		private GCHandle dataHandle;

		//private int dataLength;
        		
		public HandledByteArray (int arrayLength = MAX_BUFFER_SIZE) {
			array = new byte[arrayLength];
			handler = GCHandle.Alloc(array, GCHandleType.Pinned);
			data = new Color32[Constants.CAMERA_REQUESTED_WIDTH * Constants.CAMERA_REQUESTED_HEIGHT];
			dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			//int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
			//dataLength = lengthOfColor32 * data.Length;	
		}
		
		public byte[] GetArray () {
			return array;
		}
		
		public void SetArray (byte[] arrayToSet) {
			array = arrayToSet;
		}
		
		public void Free () {
			handler.Free ();
			dataHandle.Free ();
		}

		public IntPtr GetAddress () {
			return handler.AddrOfPinnedObject ();
		}
	}
}
