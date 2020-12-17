using UnityEngine;
using System;

namespace PikkartAR {

	public class MatrixUtils {

		private static Matrix4x4 landscapeProjectionMatrix = Matrix4x4.zero;
		private static Matrix4x4 portraitProjectionMatrix = Matrix4x4.zero;

		private static Matrix4x4 alternateLandscapeProjectionMatrix = Matrix4x4.zero;
		private static Matrix4x4 alternatePortraitProjectionMatrix = Matrix4x4.zero;

		public static Matrix4x4 GetAlternateLandscapeProjectionMatrix () {
            //Debug.Log("landscape hfov=" + Constants.CAM_LARGE_FOV + " vfov=" + Constants.CAM_SMALL_FOV);
            
            float[] mat_data = new float[16];
            PikkartARCore.GetProjectionMatrix(mat_data);
            /*alternateLandscapeProjectionMatrix.m00 = mat_data[0];
            alternateLandscapeProjectionMatrix.m01 = mat_data[1];
            alternateLandscapeProjectionMatrix.m02 = mat_data[2];
            alternateLandscapeProjectionMatrix.m03 = mat_data[3];

            alternateLandscapeProjectionMatrix.m10 = mat_data[4];
            alternateLandscapeProjectionMatrix.m11 = mat_data[5];
            alternateLandscapeProjectionMatrix.m12 = mat_data[6];
            alternateLandscapeProjectionMatrix.m13 = mat_data[7];

            alternateLandscapeProjectionMatrix.m20 = mat_data[8];
            alternateLandscapeProjectionMatrix.m21 = mat_data[9];
            alternateLandscapeProjectionMatrix.m22 = mat_data[10];
            alternateLandscapeProjectionMatrix.m23 = mat_data[11];

            alternateLandscapeProjectionMatrix.m30 = mat_data[12];
            alternateLandscapeProjectionMatrix.m31 = mat_data[13];
            alternateLandscapeProjectionMatrix.m32 = mat_data[14];
            alternateLandscapeProjectionMatrix.m33 = mat_data[15];*/

            //horizontal field of view = 2 atan(0.5 width / focallength)
            double v = 1.0 / (double)(mat_data[0]);
            double at = Math.Atan(v);
            double hfov = 2.0 * at;
            float camera_ar = Constants.CAMERA_REQUESTED_WIDTH / Constants.CAMERA_REQUESTED_HEIGHT;
            float t = ScreenUtilities.GetPortraitAspectRatio() * camera_ar * (float)Math.Tan(hfov / 2.0);
            float a = 2.0f * (float)Math.Atan((double)(t)) * 57.2958f;
            alternateLandscapeProjectionMatrix = Matrix4x4.Perspective(
                a,
                ScreenUtilities.GetLandscapeAspectRatio(),
                Constants.NEAR_CLIP_PLANE,
                Constants.FAR_CLIP_PLANE);

            return alternateLandscapeProjectionMatrix;
		}

		public static Matrix4x4 GetAlternatePortraitProjectionMatrix () {
            //Debug.Log("portrait hfov=" + Constants.CAM_LARGE_FOV + " vfov=" + Constants.CAM_SMALL_FOV);
           
            float[] mat_data = new float[16];
            PikkartARCore.GetProjectionMatrix(mat_data);
            /*alternateLandscapeProjectionMatrix.m00 = mat_data[0];
            alternateLandscapeProjectionMatrix.m01 = mat_data[1];
            alternateLandscapeProjectionMatrix.m02 = mat_data[2];
            alternateLandscapeProjectionMatrix.m03 = mat_data[3];

            alternateLandscapeProjectionMatrix.m10 = mat_data[4];
            alternateLandscapeProjectionMatrix.m11 = mat_data[5];
            alternateLandscapeProjectionMatrix.m12 = mat_data[6];
            alternateLandscapeProjectionMatrix.m13 = mat_data[7];

            alternateLandscapeProjectionMatrix.m20 = mat_data[8];
            alternateLandscapeProjectionMatrix.m21 = mat_data[9];
            alternateLandscapeProjectionMatrix.m22 = mat_data[10];
            alternateLandscapeProjectionMatrix.m23 = mat_data[11];

            alternateLandscapeProjectionMatrix.m30 = mat_data[12];
            alternateLandscapeProjectionMatrix.m31 = mat_data[13];
            alternateLandscapeProjectionMatrix.m32 = mat_data[14];
            alternateLandscapeProjectionMatrix.m33 = mat_data[15];*/
            double v = 1.0 / (double)(mat_data[0]);
            double at = Math.Atan(v);
            double hfov = 2.0 * at * 57.2958;
            alternatePortraitProjectionMatrix = Matrix4x4.Perspective(
                     (float)hfov,
                     ScreenUtilities.GetPortraitAspectRatio(),
                     Constants.NEAR_CLIP_PLANE,
                     Constants.FAR_CLIP_PLANE);

            return alternatePortraitProjectionMatrix;
		}

		private static Matrix4x4 rotationMatrixPortrait = Matrix4x4.zero;
		private static Matrix4x4 rotationMatrixPortraitUpsideDown = Matrix4x4.zero;
		private static Matrix4x4 rotationLandscapeLeft = Matrix4x4.zero;
		private static Matrix4x4 rotationLandscapeRight = Matrix4x4.zero;

		private static Matrix4x4 GetRotationMatrixPortrait () {
			if (rotationMatrixPortrait == Matrix4x4.zero) {
				rotationMatrixPortrait = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, 270), Vector3.one);
			}
			return rotationMatrixPortrait;
		}

		private static Matrix4x4 GetRotationMatrixPortraitUpsideDown () {
			if (rotationMatrixPortraitUpsideDown == Matrix4x4.zero) {
				rotationMatrixPortraitUpsideDown = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, 90), Vector3.one);
			}
			return rotationMatrixPortraitUpsideDown;
		}

		private static Matrix4x4 GetRotationLandscapeLeft () {
			if (rotationLandscapeLeft == Matrix4x4.zero) {
				rotationLandscapeLeft = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, 0), Vector3.one);
			}
			return rotationLandscapeLeft;
		}

		private static Matrix4x4 GetRotationLandscapeRight () {
			if (rotationLandscapeRight == Matrix4x4.zero) {
				rotationLandscapeRight = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, 180), Vector3.one);
			}
			return rotationLandscapeRight;
		}

		public static Matrix4x4 GetRotationMatrixForOrientation () {
			switch (DeviceOrientationChange.GetDeviceOrientation ()) {
			case DeviceOrientation.Portrait:
				return GetRotationMatrixPortrait ();
			case DeviceOrientation.PortraitUpsideDown:
				return GetRotationMatrixPortraitUpsideDown ();
			case DeviceOrientation.LandscapeRight:
				return GetRotationLandscapeRight ();
			default:
				return GetRotationLandscapeLeft ();
			}
		}

		public static Matrix4x4 GetMatrixFromArray (float[] array) {
			if (array.Length != Constants.MATRIX_4X4_ARRAY_SIZE) {
				throw new ArgumentException ("GetMatrixFromArray Input array length must be " + Constants.MATRIX_4X4_ARRAY_SIZE);
			}

			Matrix4x4 outputMatrix = new Matrix4x4 ();
			for (int y=0;y<4;y++) {
				for(int x=0; x<4; x++){
					outputMatrix[y,x] = array[x+y*4];
				}
			}
			return outputMatrix;
		}

		public static Matrix4x4 GetViewMatrixFromArray (float[] array) {
			Matrix4x4 outputMatrix = Matrix4x4.identity;
			for (int y = 0; y < 3; y++) {
				for (int x = 0; x < 4; x++) {
					outputMatrix[y,x] = array[x+y*4];
				}
			}
			return outputMatrix;
		}

		public static Matrix4x4 SwitchFirstTwoColumns (Matrix4x4 projectionMatrix) {

			Matrix4x4 supportMatrix = projectionMatrix;
				
			float m00 = supportMatrix.m00;
			float m11 = supportMatrix.m11;
			float m01 = supportMatrix.m01;
			float m10 = supportMatrix.m10;
				
			supportMatrix.m00 = m01;
			supportMatrix.m10 = m11;
			supportMatrix.m01 = m00;
			supportMatrix.m11 = m10;
				
			return supportMatrix;
		}

		public static Matrix4x4 Switch0011 (Matrix4x4 projectionMatrix) {

			Matrix4x4 supportMatrix = projectionMatrix;
				
			float m00 = supportMatrix.m00;
			float m11 = supportMatrix.m11;
				
			supportMatrix.m00 = m00;
			supportMatrix.m11 = m11;
				
			return supportMatrix;
		}

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }

        public static void SetTransformFromMatrixAverage(Transform transform, ref Matrix4x4 matrix)
        {
            Vector3 newP = (ExtractTranslationFromMatrix(ref matrix) + transform.localPosition)*0.5f;
            Quaternion newO = Quaternion.Lerp(ExtractRotationFromMatrix(ref matrix), transform.localRotation, 0.5f);
            transform.localPosition = newP;
            transform.localRotation = newO;
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }


        // EXTRAS!

        /// <summary>
        /// Identity quaternion.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
        /// </remarks>
        public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
        /// <summary>
        /// Identity matrix.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
        /// </remarks>
        public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>
        /// The translation transform matrix.
        /// </returns>
        public static Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = IdentityMatrix;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }
    }
}
