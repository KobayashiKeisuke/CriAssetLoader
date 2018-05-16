using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace CriPackageManageSystem
{
	/// <summary>
	/// UnityのAssetBundleManifestの代わりのなるCPKManifest( ただのcsv)管理用クラス
	/// </summary>
	public class CriPackageManifestController : IDisposable
	{
	    //--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====


		public const char CSV_DELIMITER = ',';

		public static readonly string SCHEME_FILENAME = "cpkname";
		public static readonly string SCHEME_VERSION = "version";

		#endregion //) ===== CONSTS =====

		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

        //Version List
        private List<string[]> m_cpkManifest = new List<string[]>(); // CSVの中身を入れるリスト

		#endregion //) ===== MEMBER_VARIABLES =====

		//--------------------------------------------
		// INIT
		//--------------------------------------------
		#region ===== INITIALIZE =====

		public CriPackageManifestController()
		{
			if( m_cpkManifest == null )
			{
				m_cpkManifest = new List<string[]>();
			}
		}
		#endregion //) ===== INITIALIZE =====

		//--------------------------------------------
		// Dispose
		//--------------------------------------------
		#region ===== DISPOSE =====

		public void Dispose()
		{
			m_cpkManifest.Clear();
		}
		#endregion //) ===== DISPOSE =====

		/// <summary>
		/// Load してきたバイナリデータからCSV情報を取得
		/// </summary>
		/// <param name="_isSucceeded"></param>
		/// <param name="_data"></param>
        public void ReadManifestCsv( bool _isSucceeded, byte[] _data)
        {
            if( _data == null || _data.Length < 1 || !_isSucceeded)
            {
                return;
            }
			//念のためリセット
			m_cpkManifest.Clear();
            /*	The request.bytes stores the loaded data.		*/
            using (var st = new StreamReader(new MemoryStream(_data), System.Text.Encoding.UTF8)) 
            {
                string csvData = st.ReadToEnd();
                StringReader reader = new StringReader(csvData);
                while(reader.Peek() > -1)
                {
                    m_cpkManifest.Add( reader.ReadLine().Split(CSV_DELIMITER));
                }
			}
        }

		/// <summary>
		/// cpkName からVersionHash値を取得
		/// </summary>
		/// <param name="_cpkName"></param>
		/// <returns></returns>
        public string GetVersionName( string _cpkName )
        {
            if( m_cpkManifest == null || m_cpkManifest.Count < 1)
            {
                return string.Empty;
            }
            //GetScheme
            int versionNameIndex = -1;
            int cpkNameIndex = -1;
            for (int i = 0; i < m_cpkManifest[0].Length; i++)
            {
                if( m_cpkManifest[0][i] == SCHEME_VERSION )
                {
                    versionNameIndex = i;
                }
                else if( m_cpkManifest[0][i] == SCHEME_FILENAME )
                {
                    cpkNameIndex = i;
                }
            }
            // Scheme が変わっている可能性があるためエラー
            if( versionNameIndex < 0 || cpkNameIndex < 0)
            {
                return string.Empty;
            }

            for (int i = 0, length=m_cpkManifest.Count; i < length; i++)
            {
                if( m_cpkManifest[i][cpkNameIndex] == _cpkName )
                {
					Debug.LogWarning( _cpkName+ "->"+m_cpkManifest[i][versionNameIndex]);
                    return m_cpkManifest[i][versionNameIndex] ;
                }
            }
            // 未登録のため不明
            return string.Empty;
		}
	}
}