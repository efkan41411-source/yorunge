using UnityEngine;

public sealed class TutorialController : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[] choices;
    [SerializeField] private UnityEngine.UI.Button next;
    [SerializeField] private TMPro.TMP_Text body;
    private GameplayStyle style;
    private int step = -1;
    private bool solved;
    private void Awake(){for(int i=0;i<choices.Length;i++){int index=i;choices[i].onClick.AddListener(()=>Choose(index));}}
    public void Configure(GameplayStyle gameplayStyle,int tutorialStep)
    {
        if(style!=gameplayStyle||step!=tutorialStep){style=gameplayStyle;step=tutorialStep;solved=false;}
        next.interactable=step>0||solved;
        for(int i=0;i<choices.Length;i++)
        {
            choices[i].gameObject.SetActive(step==0);
            choices[i].GetComponentInChildren<TMPro.TMP_Text>().text=style==GameplayStyle.Courier?(i==0?"SOL":i==1?"ORTA":"SAĞ"):(i==0?"KIRMIZI":i==1?"MAVİ":"SARI");
        }
        if(step==0&&!solved)body.text=style==GameplayStyle.Courier?"Deneyelim: Hedef ORTA şeritte.\nHedefe geçmek için doğru düğmeye dokun.":"Deneyelim: Sipariş MAVİ paket istiyor.\nAşağıdan uygun paketi seç.";
    }
    private void Choose(int index)
    {
        solved=index==1;next.interactable=solved;
        body.text=solved?"Doğru seçim!\nGerçek oyuna bir adım daha yaklaştın.":"Bir kez daha dene.\n"+(style==GameplayStyle.Courier?"Hedef ORTA şeritte.":"Sipariş MAVİ paket istiyor.");
    }
}
