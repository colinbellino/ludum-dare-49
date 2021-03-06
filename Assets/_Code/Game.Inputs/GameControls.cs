//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.1.1
//     from Assets/Inputs/GameControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Game.Inputs
{
    public partial class @GameControls : IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @GameControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameControls"",
    ""maps"": [
        {
            ""name"": ""Global"",
            ""id"": ""4697eda8-c6f8-489f-8788-f4213e440af3"",
            ""actions"": [
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""44280f62-7e08-44fb-8ad7-f6b524301f72"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""a3e8f8db-fef2-4336-a587-d99b6dc3e268"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Confirm"",
                    ""type"": ""Button"",
                    ""id"": ""4fa6220e-4015-42d4-990c-c3249c2f01ea"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a7d3969f-5af0-4c29-9c13-705c2c46144c"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c9d3d3f-1d9e-4751-b3a3-f01fc91b558c"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d40badd4-b709-4cd7-8d48-eacbfe027691"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""babb51fe-c801-4151-b741-69670c17cd0c"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3f87249a-6884-4b2e-8d17-d49f9f7d62e6"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Confirm"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3adc6850-fa38-407f-bc34-156fecef6ff3"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Confirm"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Gameplay"",
            ""id"": ""c19e3e3c-441e-46e6-a672-6a07327dc34a"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""669a3815-aef5-4946-be49-29496bd995fc"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Confirm"",
                    ""type"": ""Button"",
                    ""id"": ""59e2b5e2-a2c3-4160-9526-b74d6651644b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Reset"",
                    ""type"": ""Button"",
                    ""id"": ""f5faf788-ecb0-4cb1-a7d2-5421869f46f8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""f736fa93-6a8b-420c-9b25-c998477d80ff"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8a78db08-c7d3-4978-9c52-46ab9120f979"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""82561f99-ceef-4b30-9b7e-add6b4abff71"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""959cbe57-ac4a-427e-bb85-b86f08afcb9d"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""7ed01ae7-7eff-46fe-afe2-ea63021cde00"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""92888cea-20d7-4864-888f-aaf3117b59df"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""7204d693-4e0c-4afc-82eb-2bdc811d9699"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""bafb5471-dd56-465a-94ba-d955e4d3849c"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""924680f1-5c81-4e85-9639-182d3f074bd6"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b770c35f-063d-4115-966d-97ca0df4b9b7"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a39df26b-6651-4c3f-88be-c8e511601704"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e7f7b802-cf08-49f6-8a44-bc6cb299f425"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ffdd3a0d-6e79-4f95-87cc-594d5383470f"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Confirm"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f1581f41-0e70-44de-8588-c537f0e78dba"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Confirm"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dc5751be-1447-4cd4-94b1-21972a5591e3"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Reset"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""563bcf99-d7c7-402f-88fc-90fc7bef0d4d"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Reset"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Global
            m_Global = asset.FindActionMap("Global", throwIfNotFound: true);
            m_Global_Pause = m_Global.FindAction("Pause", throwIfNotFound: true);
            m_Global_Cancel = m_Global.FindAction("Cancel", throwIfNotFound: true);
            m_Global_Confirm = m_Global.FindAction("Confirm", throwIfNotFound: true);
            // Gameplay
            m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
            m_Gameplay_Move = m_Gameplay.FindAction("Move", throwIfNotFound: true);
            m_Gameplay_Confirm = m_Gameplay.FindAction("Confirm", throwIfNotFound: true);
            m_Gameplay_Reset = m_Gameplay.FindAction("Reset", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }
        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }
        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Global
        private readonly InputActionMap m_Global;
        private IGlobalActions m_GlobalActionsCallbackInterface;
        private readonly InputAction m_Global_Pause;
        private readonly InputAction m_Global_Cancel;
        private readonly InputAction m_Global_Confirm;
        public struct GlobalActions
        {
            private @GameControls m_Wrapper;
            public GlobalActions(@GameControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Pause => m_Wrapper.m_Global_Pause;
            public InputAction @Cancel => m_Wrapper.m_Global_Cancel;
            public InputAction @Confirm => m_Wrapper.m_Global_Confirm;
            public InputActionMap Get() { return m_Wrapper.m_Global; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GlobalActions set) { return set.Get(); }
            public void SetCallbacks(IGlobalActions instance)
            {
                if (m_Wrapper.m_GlobalActionsCallbackInterface != null)
                {
                    @Pause.started -= m_Wrapper.m_GlobalActionsCallbackInterface.OnPause;
                    @Pause.performed -= m_Wrapper.m_GlobalActionsCallbackInterface.OnPause;
                    @Pause.canceled -= m_Wrapper.m_GlobalActionsCallbackInterface.OnPause;
                    @Cancel.started -= m_Wrapper.m_GlobalActionsCallbackInterface.OnCancel;
                    @Cancel.performed -= m_Wrapper.m_GlobalActionsCallbackInterface.OnCancel;
                    @Cancel.canceled -= m_Wrapper.m_GlobalActionsCallbackInterface.OnCancel;
                    @Confirm.started -= m_Wrapper.m_GlobalActionsCallbackInterface.OnConfirm;
                    @Confirm.performed -= m_Wrapper.m_GlobalActionsCallbackInterface.OnConfirm;
                    @Confirm.canceled -= m_Wrapper.m_GlobalActionsCallbackInterface.OnConfirm;
                }
                m_Wrapper.m_GlobalActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Pause.started += instance.OnPause;
                    @Pause.performed += instance.OnPause;
                    @Pause.canceled += instance.OnPause;
                    @Cancel.started += instance.OnCancel;
                    @Cancel.performed += instance.OnCancel;
                    @Cancel.canceled += instance.OnCancel;
                    @Confirm.started += instance.OnConfirm;
                    @Confirm.performed += instance.OnConfirm;
                    @Confirm.canceled += instance.OnConfirm;
                }
            }
        }
        public GlobalActions @Global => new GlobalActions(this);

        // Gameplay
        private readonly InputActionMap m_Gameplay;
        private IGameplayActions m_GameplayActionsCallbackInterface;
        private readonly InputAction m_Gameplay_Move;
        private readonly InputAction m_Gameplay_Confirm;
        private readonly InputAction m_Gameplay_Reset;
        public struct GameplayActions
        {
            private @GameControls m_Wrapper;
            public GameplayActions(@GameControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Move => m_Wrapper.m_Gameplay_Move;
            public InputAction @Confirm => m_Wrapper.m_Gameplay_Confirm;
            public InputAction @Reset => m_Wrapper.m_Gameplay_Reset;
            public InputActionMap Get() { return m_Wrapper.m_Gameplay; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
            public void SetCallbacks(IGameplayActions instance)
            {
                if (m_Wrapper.m_GameplayActionsCallbackInterface != null)
                {
                    @Move.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMove;
                    @Move.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMove;
                    @Move.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMove;
                    @Confirm.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnConfirm;
                    @Confirm.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnConfirm;
                    @Confirm.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnConfirm;
                    @Reset.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnReset;
                    @Reset.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnReset;
                    @Reset.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnReset;
                }
                m_Wrapper.m_GameplayActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Move.started += instance.OnMove;
                    @Move.performed += instance.OnMove;
                    @Move.canceled += instance.OnMove;
                    @Confirm.started += instance.OnConfirm;
                    @Confirm.performed += instance.OnConfirm;
                    @Confirm.canceled += instance.OnConfirm;
                    @Reset.started += instance.OnReset;
                    @Reset.performed += instance.OnReset;
                    @Reset.canceled += instance.OnReset;
                }
            }
        }
        public GameplayActions @Gameplay => new GameplayActions(this);
        public interface IGlobalActions
        {
            void OnPause(InputAction.CallbackContext context);
            void OnCancel(InputAction.CallbackContext context);
            void OnConfirm(InputAction.CallbackContext context);
        }
        public interface IGameplayActions
        {
            void OnMove(InputAction.CallbackContext context);
            void OnConfirm(InputAction.CallbackContext context);
            void OnReset(InputAction.CallbackContext context);
        }
    }
}
