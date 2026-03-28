using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.DefaultInputActions;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Console
{
    public class Console_Main : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private TextAsset consoleXmlFile; // Добавляем ссылку на XML файл

        private InputActionMap inputActionMap;
        private InputAction inputAction;
        public Texture2D overlayImage;
        public float raz;
        private bool showOverlay = false;



        private void Awake()
        {


            inputActionMap = inputActionAsset.FindActionMap("button_console");
            inputAction = inputActionMap.FindAction("button_1");

            inputAction.started += OnButton;
            overlayImage = MakeTextureTransparent(overlayImage, raz);
        }

        private void OnEnable()
        {
            inputAction.Enable();
        }

        private void OnDisable()
        {
            inputAction.Disable();
        }

        private void OnDestroy()
        {
            inputAction.started -= OnButton;
        }

        private void OnButton(InputAction.CallbackContext context)
        {
            showOverlay = !showOverlay;
        }

        private string currentInput = "";
        private List<string> outputLines = new List<string>();
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private Vector2 scrollPosition;

        private void OnGUI()
        {
            if (!showOverlay) return;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayImage);

            Rect outputRect = new Rect(10, 10, Screen.width - 20, Screen.height * 0.9f - 20);
            GUILayout.BeginArea(outputRect);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.box);

            foreach (string line in outputLines)
            {
                GUILayout.Label(line);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            Rect inputRect = new Rect(10, Screen.height * 0.9f, Screen.width - 20, Screen.height * 0.1f - 10);
            GUILayout.BeginArea(inputRect, GUI.skin.box);

            GUILayout.Label("Ввод:");

            GUI.SetNextControlName("InputField");
            currentInput = GUILayout.TextField(currentInput);

            if (GUILayout.Button("Отправить (Enter)") || Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                SubmitInput();
            }

            if (Event.current.isKey)
            {
                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    NavigateHistory(-1);
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    NavigateHistory(1);
                    Event.current.Use();
                }
            }

            GUILayout.EndArea();

            if (GUI.GetNameOfFocusedControl() == "")
            {
                GUI.FocusControl("InputField");
            }
        }

        private void SubmitInput()
        {
            if (!string.IsNullOrEmpty(currentInput))
            {
                outputLines.Add("> " + currentInput);
                commandHistory.Add(currentInput);
                historyIndex = commandHistory.Count;
                ProcessCommand(currentInput);
                currentInput = "";
                scrollPosition.y = float.MaxValue;
            }
        }

        private void NavigateHistory(int direction)
        {
            if (commandHistory.Count == 0) return;

            historyIndex += direction;

            if (historyIndex < 0)
            {
                historyIndex = -1;
                currentInput = "";
            }
            else if (historyIndex >= commandHistory.Count)
            {
                historyIndex = commandHistory.Count;
                currentInput = "";
            }
            else
            {
                currentInput = commandHistory[historyIndex];
            }
        }
        public Texture2D MakeTextureTransparent(Texture2D sourceTexture, float alphaMultiplier)
        {
            Texture2D newTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

            Color32[] pixels = sourceTexture.GetPixels32();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                pixel.a = (byte)(pixel.a * alphaMultiplier);
                pixels[i] = pixel;
            }

            newTexture.SetPixels32(pixels);
            newTexture.Apply();

            return newTexture;
        }









        MethodFinder method = new MethodFinder();
        private void ProcessCommand(string command)
        {
            method.ParseTypedValues(command);
            //outputLines.Add(command);
        }
    }
}