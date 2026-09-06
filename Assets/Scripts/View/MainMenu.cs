using UnityEngine;
using UnityEngine.InputSystem;

namespace ToDonegeon.View
{
    public sealed class MainMenu : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }

        [SerializeField] private string _title = "ToDonegeon";

        private const float ButtonWidth = 360f;
        private const float ButtonHeight = 70f;
        private const float ButtonGap = 20f;
        private const float TitleHeight = 100f;
        private const int TitleFontSize = 52;
        private const int ButtonFontSize = 26;
        private const int AboutFontSize = 18;
        private const float AboutWidth = 580f;
        private const float AboutHeight = 380f;
        private const float OpenButtonWidth = 90f;
        private const float OpenButtonHeight = 28f;
        private const float OpenButtonMargin = 8f;

        [Header("Colors")]
        [SerializeField] private Color _backgroundColor = new Color(0.16f, 0.12f, 0.28f, 1f);
        [SerializeField] private Color _buttonColor = new Color(0.95f, 0.55f, 0.10f, 1f);
        [SerializeField] private Color _buttonHoverColor = new Color(1f, 0.66f, 0.20f, 1f);
        [SerializeField] private Color _buttonTextColor = Color.white;
        [SerializeField] private Color _titleColor = new Color(1f, 0.85f, 0.55f, 1f);
        [SerializeField] private Color _aboutColor = new Color(0.95f, 0.92f, 0.85f, 1f);

        private const string AboutText =
            "ToDonegeon is a turn-based productivity roguelike where you level up by completing you real-life tasks.\n\n" +
            "Descend through procedurally generated dungeons: find the stairs (a pink tile) to go deeper, and every " +
            "floor grows larger and more dangerous.\n\n" +
            "Your strength comes from real life - complete the tasks in your to-do list to earn XP, " +
            "level up and raise your attack. You also heal when you level up! \n\n" +
            "Additionly, you can dock the window to your screen edge to keep your task list on top while you work.\n\n" +
            "The application doesn't contain any sounds or real-time activities so that you can focus on your tasks and not get distracted";

        private GUIStyle _backgroundStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _aboutStyle;

        private Texture2D _backgroundTexture;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;
        private Color _builtBackground;
        private Color _builtButton;
        private Color _builtButtonHover;
        private Color _builtButtonText;
        private Color _builtTitle;
        private Color _builtAbout;
        private bool _stylesBuilt;

        private bool _showAbout;
        private bool _gameStarted;

        private void Awake()
        {
            IsOpen = true;
            _showAbout = false;
            _gameStarted = false;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            if (IsOpen)
            {
                if (_gameStarted) IsOpen = false; // resume
            }
            else
            {
                _showAbout = false;
                IsOpen = true;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (!IsOpen)
            {
                DrawOpenButton();
                return;
            }

            GUI.depth = -1000; // draw on top of any other panel
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none, _backgroundStyle);

            if (_showAbout) DrawAbout();
            else DrawMenu();
        }

        // small in-game button that reopens the menu once a game is running
        private void DrawOpenButton()
        {
            if (!_gameStarted || WindowDock.IsDocked) return;

            float x = (Screen.width - OpenButtonWidth) * 0.5f;
            var rect = new Rect(x, OpenButtonMargin, OpenButtonWidth, OpenButtonHeight);
            ScreenGuiRegistry.Register(rect); // so clicking it does not also move the player
            if (GUI.Button(rect, "Menu")) IsOpen = true;
        }

        private void DrawMenu()
        {
            GUI.Label(new Rect(0f, Screen.height * 0.2f, Screen.width, TitleHeight), _title, _titleStyle);

            float x = (Screen.width - ButtonWidth) * 0.5f;
            float y = Screen.height * 0.44f;

            string primary = _gameStarted ? "Resume" : "Start the game";
            if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), primary, _buttonStyle))
            {
                _gameStarted = true;
                IsOpen = false;
            }

            y += ButtonHeight + ButtonGap;
            if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), "Read about the game", _buttonStyle))
                _showAbout = true;

            y += ButtonHeight + ButtonGap;
            if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), "Exit", _buttonStyle))
                Application.Quit();
        }

        private void DrawAbout()
        {
            float x = (Screen.width - AboutWidth) * 0.5f;
            float y = (Screen.height - AboutHeight) * 0.5f;

            GUILayout.BeginArea(new Rect(x, y, AboutWidth, AboutHeight));
            GUILayout.Label(_title, _titleStyle);
            GUILayout.Space(ButtonGap);
            GUILayout.Label(AboutText, _aboutStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", _buttonStyle, GUILayout.Height(ButtonHeight)))
                _showAbout = false;
            GUILayout.EndArea();
        }

        // rebuilds the styles only when first needed or after a colour is changed in the inspector
        private void EnsureStyles()
        {
            if (_stylesBuilt
                && _backgroundStyle.normal.background != null
                && _builtBackground == _backgroundColor
                && _builtButton == _buttonColor
                && _builtButtonHover == _buttonHoverColor
                && _builtButtonText == _buttonTextColor
                && _builtTitle == _titleColor
                && _builtAbout == _aboutColor)
                return;

            RebuildStyles();
        }

        private void RebuildStyles()
        {
            ReplaceTexture(ref _backgroundTexture, _backgroundColor);
            ReplaceTexture(ref _buttonTexture, _buttonColor);
            ReplaceTexture(ref _buttonHoverTexture, _buttonHoverColor);

            _backgroundStyle = new GUIStyle { normal = { background = _backgroundTexture } };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = TitleFontSize
            };
            _titleStyle.normal.textColor = _titleColor;

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = ButtonFontSize
            };
            _buttonStyle.normal.background = _buttonTexture;
            _buttonStyle.hover.background = _buttonHoverTexture;
            _buttonStyle.active.background = _buttonHoverTexture;
            _buttonStyle.normal.textColor = _buttonTextColor;
            _buttonStyle.hover.textColor = _buttonTextColor;
            _buttonStyle.active.textColor = _buttonTextColor;

            _aboutStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = AboutFontSize,
                wordWrap = true
            };
            _aboutStyle.normal.textColor = _aboutColor;

            _builtBackground = _backgroundColor;
            _builtButton = _buttonColor;
            _builtButtonHover = _buttonHoverColor;
            _builtButtonText = _buttonTextColor;
            _builtTitle = _titleColor;
            _builtAbout = _aboutColor;
            _stylesBuilt = true;
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null) Destroy(_backgroundTexture);
            if (_buttonTexture != null) Destroy(_buttonTexture);
            if (_buttonHoverTexture != null) Destroy(_buttonHoverTexture);
        }

        private static void ReplaceTexture(ref Texture2D texture, Color color)
        {
            if (texture != null) Destroy(texture);
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
        }
    }
}
