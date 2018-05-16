using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace CriPackageManageSystem
{
	/// <summary>
	/// Cache 制御スクリプト
	/// </summary>
	public class CriPackageCacheController 
	{
		 //--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

		// 拡張子分離用
		const char SPLIT_EXTENTION_CHAR = '.';

		#endregion //) ===== CONSTS =====


		//--------------------------------------------
		// static API
		//--------------------------------------------
		#region ===== STATIC_API =====


		/// <summary>
		/// Packageの出力先Dir
		/// </summary>
		/// <param name="_packageName"></param>
		/// <returns></returns>
		public static string GetOutputDirPath( string _packageName )
		{
			return Path.Combine( CriPackageUtility.AssetCachePath, _packageName.Split(SPLIT_EXTENTION_CHAR)[0] );
		}

		/// <summary>
		/// CacheRoot/CPK_BASENAME/HASH.cpk 
		/// という形に変形させる
		/// </summary>
		/// <param name="_packageName"></param>
		/// <param name="_versionHashName"></param>
		/// <returns></returns>
		public static string GetOutputPath( string _packageName, string _versionHashName )
		{
			string extention = Path.GetExtension(_packageName);
			return string.Format( "{0}/{1}{2}", GetOutputDirPath(_packageName), _versionHashName, extention);
		}

		/// <summary>
		/// 必要に応じてキャッシュ用ディレクトリを作成する
		/// </summary>
		/// <param name="_packageName"></param>
		public static void CreateAssetCacheDir( string _packageName )
		{
			string path = GetOutputDirPath( _packageName);
			if( !System.IO.Directory.Exists( path))
			{
				System.IO.Directory.CreateDirectory( path );
			}
		}

		
		/// <summary>
		/// CacheDirに存在するかチェック
		/// TODO: AssetBundle みたいにVersionIDやHash値チェックしたい
		/// </summary>
		/// <param name="_packageName"></param>
		/// <returns></returns>
		public static bool IsCached( string _packageName, string _versionHashName )
		{
			string path = GetOutputPath( _packageName, _versionHashName);
			return File.Exists( path );
		}

		#endregion //) ===== STATIC_API =====
	}
}