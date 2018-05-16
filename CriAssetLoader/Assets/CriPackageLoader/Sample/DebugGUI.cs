using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using CriPackageManageSystem;
public class DebugGUI : MonoBehaviour {


	public CriAtomSource source;

	public void LoadCpk()
	{
		CriPackageManager.I.LoadCPK( "Common/common.cpk", (bool isSuceeded, CriPackageLoadController.BinderData data) =>{
			//hoge
			CriAtom.AddCueSheet( "Common/BGM", "Common/BGM.acb", "Common/BGM.awb", data.Binder);

			CriAtom.AddCueSheet("Common/SE","Common/SE.acb","", data.Binder);
		});
	}

	public void PlayBGM()
	{
		source.Stop();

		source.cueSheet = "Common/BGM";
		source.cueName = "bgm0";

		source.Play();
	}

	public void PlaySE()
	{
		source.Stop();
		
		source.cueSheet = "Common/SE";
		source.cueName = "se01";

		source.Play();
	}
	public void PlaySE2()
	{
		source.Stop();
		
		source.cueSheet = "Common/SE";
		source.cueName = "se02";

		source.Play();
	}
}
