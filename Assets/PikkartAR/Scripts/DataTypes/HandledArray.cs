using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PikkartAR {

	public class HandledArray<T> {

        public const int MAX_BUFFER_SIZE = 2073600; //1228800; //640*480*4


        private T[] array;
		private GCHandle handler;
		
		public HandledArray (int arrayLength = MAX_BUFFER_SIZE) {
			array = new T[arrayLength];
			handler = GCHandle.Alloc(array, GCHandleType.Pinned);
		}
		
		public T[] GetArray () {
			return array;
		}
		
		public void SetArray (T[] arrayToSet) {
			array = arrayToSet;
		}
		
		public void Free () {
			handler.Free ();
		}
	}
}
