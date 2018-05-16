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

		/// <summary>
		/// 指定バージョン以外の
		/// </summary>
		/// <param name="_packageName"></param>
		/// <param name="_versionHashName"></param>
		/// <returns></returns>
		public static bool DeleteTargetOtherVersionPackage(string _packageName, string _versionHashName )
		{
			string dirPath = GetOutputDirPath( _packageName);

			if( string.IsNullOrEmpty(dirPath)  || string.IsNullOrEmpty( _packageName) || string.IsNullOrEmpty( _versionHashName))
			{
				return false;
			}
			// Dir が無ければ削除も同然
			if( !Directory.Exists( dirPath ))
			{
				return true;
			}

			// 指定Dirにファイルがなければ、実質削除完了と等価
			string[] filePaths = Directory.GetFiles( dirPath );
			if( filePaths == null || filePaths.Length < 1)
			{
				return true;
			}

			// 指定Hash以外のファイルを全削除
			for (int i = 0; i < filePaths.Length; i++)
			{
				if( Path.GetFileName( filePaths[i]) == _versionHashName)
				{
					continue;
				}
				File.Delete( filePaths[i]);
			}
			return true;	
		}

		#endregion //) ===== STATIC_API =====
	}
}