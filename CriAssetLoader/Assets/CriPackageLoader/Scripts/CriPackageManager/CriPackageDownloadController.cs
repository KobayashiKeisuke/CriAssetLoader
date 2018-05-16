using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CriPackageManageSystem
{
	/// <summary>
	/// Cri のPackage(CPK) のDownload周りを制御するスクリプト
	/// </summary>
	public class CriPackageDownloadController :  System.IDisposable
	{
	    //--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

		// Download 完了コールバック定義
		public delegate void OnCompleteDownload( bool _isSucceeded);
		#endregion //) ===== CONSTS =====

	    //--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		// Installer
		private CriFsWebInstaller[] m_webInstallers = null;
		private CriFsWebInstaller[] WebInstallers{get{return m_webInstallers;}}

		#endregion //) ===== MEMBER_VARIABLES =====


	    //--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INITIALIZE =====

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="_concurrency">同時インストール可能数</param>
		public CriPackageDownloadController( int _concurrency )
		{
			int concurrency = Mathf.Max( _concurrency, 1);	//NonZero
			m_webInstallers = new CriFsWebInstaller[concurrency];
			for (int i = 0; i < concurrency; i++)
			{
				m_webInstallers[i] = new CriFsWebInstaller();
			}
		}
		#endregion //) ===== INITIALIZE =====

		//--------------------------------------------
		// Dispose
		//--------------------------------------------
		#region ===== DISPOSE =====

		public void Dispose()
		{
			for (int i = 0; i < WebInstallers.Length; i++)
			{
				WebInstallers[i].Dispose();
				WebInstallers[i] = null;
			}

			m_webInstallers = null;
		}
		#endregion //) ===== DISPOSE =====

	    //--------------------------------------------
		// Public API
		//--------------------------------------------
		#region ===== PUBLIC_API =====


		/// <summary>
		/// Download 実行
		/// </summary>
		/// <param name="_cpkName">CPK名</param>
		/// <param name="_versionHashName">CPKのHash値文字列</param>
		/// <param name="_onComplete">ダウンロード完了コールバック</param>
		/// <returns></returns>
		public IEnumerator DonwloadCPK( string _cpkName, string _versionHashName, OnCompleteDownload _onComplete )
		{
			string url = CriPackageUtility.GetPackegeURL( _cpkName );
			// 無効なURL
			if( string.IsNullOrEmpty(url))
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( false );
				}
				yield break;
			}
			// Cache上に存在している
			if( CriPackageCacheController.IsCached( _cpkName, _versionHashName ))
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( true );
				}
				yield break;
			}

			CriFsWebInstaller installer = GetEmptyInstaller();
			// 空くまで待機
			while( installer == null )
			{
				installer = GetEmptyInstaller();
				yield return null;
			}
			string outputPath = CriPackageCacheController.GetOutputPath( _cpkName, _versionHashName );
			// Directory 生成
			CriPackageCacheController.CreateAssetCacheDir( _cpkName );

			// Download 開始
			installer.Copy(url, outputPath);
			// 終了まで待機
			CriFsWebInstaller.StatusInfo info = installer.GetStatusInfo();
			while( info.status == CriFsWebInstaller.Status.Busy )
			{
				info = installer.GetStatusInfo();
				yield return null;
			}

			info = installer.GetStatusInfo();
			switch( info.status)
			{
				case CriFsWebInstaller.Status.Error:
				{
					//エラーによる失敗
					if( _onComplete != null )
					{
						_onComplete.Invoke( false );
					}

				}break;
				case CriFsWebInstaller.Status.Stop:
				{
					// 誰かに止められた
					if( _onComplete != null )
					{
						_onComplete.Invoke( false );
					}
				}break;
				case CriFsWebInstaller.Status.Complete:
				{
					// 成功
					if( _onComplete != null )
					{
						_onComplete.Invoke( true );
					}
				}break;

			}
		}

		#endregion //) ===== PUBLIC_API =====

		/// <summary>
		/// 空きInstallerを取得
		/// </summary>
		/// <returns></returns>
		private CriFsWebInstaller GetEmptyInstaller()
		{
			for (int i = 0; i < WebInstallers.Length; i++)
			{
				var statusInfo = WebInstallers[i].GetStatusInfo();
				if( statusInfo.status != CriFsWebInstaller.Status.Busy )
				{
					return WebInstallers[i];
				}
			}

			return null;
		}
	}
}
