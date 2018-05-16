using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CriPackageManageSystem
{
	/// <summary>
	/// CRI 謹製ファイルマジックプロを用いてパッキングしたファイル管理システム
	/// </summary>
	/// <typeparam name="CriPackageManager"></typeparam>
	public class CriPackageManager : Singleton<CriPackageManager> , System.IDisposable
	{
		//--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

		public const int DOWNLOAD_CONCURRENCY = 2;
		#endregion //) ===== CONSTS =====

		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		CriPackageLoadController m_loadCtrl = null;
		#endregion //) ===== MEMBER_VARIABLES =====
		
		//--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INIT =====

		/// <summary>
		/// 初期化処理
		/// 継承先で何らかの初期化処理が必要な場合はこちらへ
		/// ※Awake 以外からの呼び出し禁止
		/// </summary>
		protected override void Init()
		{
			// Initialize CriFsWebInstaller module
			CriFsWebInstaller.InitializeModule(CriFsWebInstaller.defaultModuleConfig);

			m_loadCtrl = new CriPackageLoadController( this, DOWNLOAD_CONCURRENCY);

			StartCoroutine( m_loadCtrl.LoadManifest("manifest.cpk", "testVer") );
		}
		#endregion //) ===== INIT =====


		//--------------------------------------------
		// Dispose
		//--------------------------------------------
		#region ===== DISPOSE =====

		void OnDestroy()
		{
			Dispose();
		}

		public void Dispose()
		{
			m_loadCtrl.Dispose();
			
			// Finalize CriFsWebInstaller module
			CriFsWebInstaller.FinalizeModule();
		}
		#endregion //) ===== DISPOSE =====


		//--------------------------------------------
		// Update
		//--------------------------------------------
		#region ===== UPDATE =====

		void Update()
		{
			// DL中はこれを常に実行させる
			CriFsWebInstaller.ExecuteMain();
		}

		#endregion //) ===== UPDATE =====

		//--------------------------------------------
		// Public API
		//--------------------------------------------
		#region ===== PUBLIC_API =====

		/// <summary>
		/// CriPackage のLoad窓口
		/// </summary>
		/// <param name="_cpkName"></param>
		/// <param name="_fileName"></param>
		public void LoadFile(string _cpkName, string _fileName )
		{
			if( m_loadCtrl == null )
			{
				Debug.LogWarning("LoadController doesnt initilized:"+_cpkName);
				return;
			}

			StartCoroutine( m_loadCtrl.LoadFile(_cpkName, _fileName, null));

		}

		/// <summary>
		/// CPKのロード
		/// </summary>
		/// <param name="_cpkName">.cpk のパス</param>
		/// <param name="_onComplete">コールバック</param>
		public void LoadCPK( string _cpkName, CriPackageLoadController.OnCompleteBind _onComplete)
		{
			if( m_loadCtrl == null )
			{
				Debug.LogWarning("LoadController doesnt initilized:"+_cpkName);
				return;
			}

			StartCoroutine( m_loadCtrl.LoadCPK(_cpkName, _onComplete));
		}
		#endregion //) ===== PUBLIC_API =====

	}	
}