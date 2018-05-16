using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CriPackageManageSystem
{
    /// <summary>
    /// Package(CPK) Load 管理コントローラー
    /// </summary>
    public class CriPackageLoadController : System.IDisposable
    {
    
    	//--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

        /// <summary>
        /// Binder 管理用データクラス
        /// </summary>
        public class BinderData
        {
            // Bind ID
            private uint m_bindId= 0 ;
            public  uint BindId{get{return m_bindId;}}

            // Binder
            private CriFsBinder m_binder = null;
            public CriFsBinder Binder{get{return m_binder;}}

            // Binder 参照カウント
            private int m_refCount = 0 ;
            public  int RefCount{get{return m_refCount;}}

            // コンストラクタ
            public BinderData( uint _bindId, CriFsBinder _binder)
            {
                m_bindId = _bindId;
                m_binder = _binder;
                m_refCount =0;
            }

            public void SetReference(){m_refCount++;}

            public void SetRelease()
            {
                m_refCount--;
                if( m_refCount < 1)
                {
                    CriFsBinder.Unbind( BindId );
                    Binder.Dispose();
                    m_binder = null;
                }
            }
        }


		// Bind 完了コールバック定義
		public delegate void OnCompleteBind( bool _isSucceeded, BinderData _bindData );
        // Load 完了時コールバック定義
		public delegate void OnCompleteLoad( bool _isSucceeded, byte[] _data );
		#endregion //) ===== CONSTS =====

	    //--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====
        CriPackageManager m_manager = null;

        //Download Controller
        CriPackageDownloadController m_dlCtrl;

        // Bind List
        Dictionary< string, BinderData > m_bindDict = new Dictionary<string, BinderData>(); // KeyValuePair< cpkName, Data >
        Dictionary<string, BinderData> BindDict{get{return m_bindDict;}}

		#endregion //) ===== MEMBER_VARIABLES =====


	    //--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INITIALIZE =====

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_concurrency"></param>
        public CriPackageLoadController(CriPackageManager _manager,  int _concurrency)
        {
            m_manager = _manager;
            m_dlCtrl = new CriPackageDownloadController( _concurrency );
        }
		#endregion //) ===== INITIALIZE =====

		//--------------------------------------------
		// Dispose
		//--------------------------------------------
		#region ===== DISPOSE =====

		public void Dispose()
		{
            foreach (KeyValuePair<string, BinderData> pair  in BindDict)
            {
                //強制リリース
                while( pair.Value.RefCount > 0)
                {
                    pair.Value.SetRelease();
                }
            }

			BindDict.Clear();

            m_dlCtrl.Dispose();
		}
		#endregion //) ===== DISPOSE =====

	    //--------------------------------------------
		// Public API
		//--------------------------------------------
		#region ===== PUBLIC_API =====

        /// <summary>
        /// CPK からファイルのロードを行う
        /// </summary>
        /// <param name="_cpkName"></param>
        /// <param name="_fileName">CPKのRootからの相対パス</param>
        /// <param name="_onCompLoad">コールバック</param>
        /// <returns></returns>
        public IEnumerator LoadFile( string _cpkName, string _fileName, OnCompleteLoad _onCompLoad )
        {
            /* 
             * ------------------------------
             * フロー
             * 1. Cache に無ければDownload -> Cache に保存
             * 2. Cache にあるCPKファイルをBind
             * 3. Bind したbinder から目的のファイルを取り出す
             * ------------------------------
             */

            // Download ~ Bind まで
            bool isSucceededLoadCpk = false;
            BinderData bindedData = null;
            yield return LoadCPK( _cpkName, ( bool _isSucceeded, BinderData _data)=>{
                isSucceededLoadCpk = _isSucceeded;
                bindedData = _data;
            });

            // Load 失敗したのでここで打ち止め
            if( !isSucceededLoadCpk )
            {
                if( _onCompLoad != null )
                {
                    _onCompLoad.Invoke( false, null );
                }
                yield break;
            }

            // File のLoad 部分
            yield return LoadFile( _fileName, bindedData, _onCompLoad);
        }

        /// <summary>
        /// CPK のDownload~Bind までの処理
        /// LoadFileの途中処理としての利用や、CueSheet取得用などに用いると思われ
        /// </summary>
        /// <param name="_cpkName"></param>
        /// <param name="_onComplete"></param>
        /// <returns></returns>
        public IEnumerator LoadCPK( string _cpkName, OnCompleteBind _onComplete)
        {
             /* 
             * ------------------------------
             * フロー
             * 1. Cache に無ければDownload -> Cache に保存
             * 2. Cache にあるCPKファイルをBind
             * ------------------------------
             */

            // Cache上に無い
			if( !CriPackageUtility.IsCached( _cpkName ))
			{
                // Download すべきか、サーバー上のファイルを直接バインドすべきかは要検討
                yield return　m_dlCtrl.DonwloadCPK( _cpkName, null);
			}

            // Dictionary未登録であればCache 上にあるCPKファイルをバインド
            if( !BindDict.ContainsKey(_cpkName))
            {
                yield return BindCPK( _cpkName);
            }

            BinderData data = null;
            bool ret = BindDict.TryGetValue( _cpkName, out data);

            if( _onComplete != null )
            {
                // Load 成功したので参照しているはず
                if( ret && data != null )
                {
                    data.SetReference();
                }
                _onComplete.Invoke( ret, data);
            }
        }

        public bool SetRelease( string _cpkName )
        {
            BinderData data = null;
            bool ret = BindDict.TryGetValue( _cpkName, out data);
            if( !ret || data == null)
            {
                return false;
            }

            data.SetRelease();
            return true;
        }
		#endregion //) ===== PUBLIC_API =====



        /// <summary>
        /// 指定ファイルのバインド
        /// </summary>
        /// <param name="_cpkName"></param>
        /// <param name="bindId"></param>
        /// <returns></returns>
        private IEnumerator BindCPK( string _cpkName )
        {
            uint bindId = 0;
            if( m_manager == null )
            {
                Debug.LogWarning("Cant Exec Coroutine");
                yield break;
            }
            string path = CriPackageUtility.GetOutputPath( _cpkName );
            CriFsBinder binder = new CriFsBinder();

            CriFsBindRequest request = CriFsUtility.BindCpk(binder, path);
            bindId = request.bindId;

            // 待機
	    	yield return request.WaitForDone(m_manager);

            if( string.IsNullOrEmpty( request.error ) )
            {
                // Succeeded
                // List に登録
                BindDict.Add( _cpkName, new BinderData( bindId, binder));
            }
            else
            {
                //Error
                Debug.LogWarning("Failed to bind CPK. (path=" + path + ")");
                Debug.LogWarning("Error:"+request.error);
            }
        }

        /// <summary>
        /// File のLoadリクエスト
        /// </summary>
        /// <param name="_fileName">ファイル名(CPKのRootからの相対パス)</param>
        /// <param name="_data">対象のBindData</param>
        /// <param name="_onComplete">コールバック</param>
        /// <returns></returns>
        private IEnumerator LoadFile( string _fileName, BinderData _data, OnCompleteLoad _onComplete )
        {
            // Validation
            if( string.IsNullOrEmpty( _fileName) ||  _data == null )
            {
                if( _onComplete != null )
                {
                    _onComplete.Invoke( false, null);
                }
                yield break;
            }

            CriFsLoadFileRequest req = CriFsUtility.LoadFile( _data.Binder, _fileName );

            // 待機
	    	yield return req.WaitForDone(m_manager);

            bool isSucceeded = !string.IsNullOrEmpty( req.error);
            if( !isSucceeded)
            {
                Debug.LogWarning( "Failed to load :"+_fileName);
            }

            if( _onComplete != null )
            {
                _onComplete.Invoke( isSucceeded, req.bytes );
            }
        }
    }
}

