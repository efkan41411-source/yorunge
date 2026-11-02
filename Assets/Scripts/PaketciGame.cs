using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class PaketciGame : MonoBehaviour
{
    [SerializeField] private PaketciUIController ui;
    private AudioSource audioSource;
    private AudioClip tap, success, error;
    private void Awake()
    {
        Screen.orientation=ScreenOrientation.Portrait;Screen.sleepTimeout=SleepTimeout.SystemSetting;
        audioSource=GetComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;
        tap=Tone("Paketci_Tap",720,0.045f);success=Tone("Paketci_Success",980,0.13f);error=Tone("Paketci_Error",260,0.11f);
        ui.Feedback+=Play;
    }
    private void OnDestroy(){if(ui!=null)ui.Feedback-=Play;if(tap!=null)Destroy(tap);if(success!=null)Destroy(success);if(error!=null)Destroy(error);}
    private void Play(string message,bool good)
    {
        var save=ui.Session.Save;if(save.Sound){audioSource.volume=save.Volume;audioSource.PlayOneShot(message=="tap"?tap:good?success:error);}
        if(message!="tap"&&save.Haptics&&Application.isMobilePlatform)Handheld.Vibrate();
    }
    private static AudioClip Tone(string name,float frequency,float duration)
    {
        const int rate=22050;int count=Mathf.CeilToInt(rate*duration);float[] samples=new float[count];for(int i=0;i<count;i++)samples[i]=Mathf.Sin(2*Mathf.PI*frequency*i/rate)*(1-(float)i/count)*0.18f;
        var clip=AudioClip.Create(name,count,1,rate,false);clip.SetData(samples,0);return clip;
    }
}
