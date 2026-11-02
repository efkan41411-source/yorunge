using UnityEngine;

public sealed class PaketciInputController : MonoBehaviour
{
    [SerializeField] private UnityEngine.InputSystem.InputActionAsset controls;
    [SerializeField] private PaketciUIController ui;
    private UnityEngine.InputSystem.InputActionAsset runtime;
    private UnityEngine.InputSystem.InputAction left, right, throwAction, back;
    private void Awake()
    {
        runtime=Instantiate(controls);
        left=runtime.FindAction("Gameplay/Left",true);right=runtime.FindAction("Gameplay/Right",true);throwAction=runtime.FindAction("Gameplay/Throw",true);back=runtime.FindAction("Gameplay/Back",true);
        left.performed+=Left;right.performed+=Right;throwAction.performed+=Throw;back.performed+=Back;
    }
    private void OnEnable(){if(runtime!=null)runtime.Enable();}
    private void OnDisable(){if(runtime!=null)runtime.Disable();}
    private void OnDestroy(){if(runtime==null)return;left.performed-=Left;right.performed-=Right;throwAction.performed-=Throw;back.performed-=Back;Destroy(runtime);}
    private void Left(UnityEngine.InputSystem.InputAction.CallbackContext context)=>ui.Move(-1);
    private void Right(UnityEngine.InputSystem.InputAction.CallbackContext context)=>ui.Move(1);
    private void Throw(UnityEngine.InputSystem.InputAction.CallbackContext context)=>ui.Throw();
    private void Back(UnityEngine.InputSystem.InputAction.CallbackContext context)=>ui.Back();
}
