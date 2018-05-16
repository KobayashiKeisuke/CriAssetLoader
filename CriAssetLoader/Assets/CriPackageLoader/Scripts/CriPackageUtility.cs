using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace CriPackageManageSystem
{
	/// <summary>
	/// Utilityクラス
	/// </summary>
	public static class CriPackageUtility
	{
		//--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====
		// HOST
		public static readonly string HOST = "https://AWSとかのサーバー/";

		// 念のためにAssetBundleと別になるようにTempから一階層掘る
		public static readonly string ASSET_ROOT_DIR = "cri_assets";

		#endregion //) ===== CONSTS =====

		/// <summary>
		/// AssetBundle の保存先のRootDirPath
		/// </summary>
		/// <returns></returns>
		public static string AssetCachePath { get { return Path.Combine( Application.platform == RuntimePlatform.IPhonePlayer ? Application.temporaryCachePath : Application.persistentDataPath, ASSET_ROOT_DIR); }}

		/// <summary>
		/// Download URL を取得
		/// </summary>
		/// <param name="_packageName"></param>
		/// <returns></returns>
		public static string GetPackegeURL( string _packageName )
		{
			if( string.IsNullOrEmpty(_packageName))
			{
				return string.Empty;
			}
			return string.Format("{0}{1}",HOST,_packageName);
		}
	}

}
