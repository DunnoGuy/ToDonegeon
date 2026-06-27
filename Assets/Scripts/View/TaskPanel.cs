using UnityEngine;
using ToDonegeon.Tasks;
using ToDonegeon.Gameplay;

namespace ToDonegeon.View
{
    // Prototype task tracker
    public sealed class TaskPanel : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;

        private const float PanelWidth = 300f;
        private const float PanelMargin = 10f;
        private const float RowSpacing = 6f;
        private const float BarHeight = 16f;
        private const float CheckboxWidth = 22f;
        private const float RewardWidth = 44f;
        private const float RemoveWidth = 24f;
        private const float ReorderWidth = 22f;

        private static readonly string[] DifficultyNames = System.Enum.GetNames(typeof(TaskDifficulty));

        private readonly TaskList _tasks = new TaskList();
        private Vector2 _scroll;
        private string _newTitle = string.Empty;
        private int _newDifficulty = (int)TaskDifficulty.Medium;
        private string _lastReward;
        private GUIStyle _barLabel;

        private void Awake()
        {
            if (_player == null) _player = FindAnyObjectByType<PlayerController>();
            SeedExamples();
        }

        private void SeedExamples()
        {
            _tasks.Add("Read a chapter", TaskDifficulty.Easy);
            _tasks.Add("30-minute workout", TaskDifficulty.Medium);
            _tasks.Add("Finish the assignment", TaskDifficulty.Hard);
        }

        private void OnGUI()
        {
            float x = Screen.width - PanelWidth - PanelMargin;
            var area = new Rect(x, PanelMargin, PanelWidth, Screen.height - 2f * PanelMargin);
            ScreenGuiRegistry.Register(area);
            GUILayout.BeginArea(area, GUI.skin.box);

            GUILayout.Label("TASKS");
            DrawProgress();
            GUILayout.Space(RowSpacing);

            _scroll = GUILayout.BeginScrollView(_scroll);
            DrawTasks();
            GUILayout.EndScrollView();

            GUILayout.Space(RowSpacing);
            DrawAddRow();

            GUILayout.EndArea();
        }

        private void DrawProgress()
        {
            PlayerCharacter character = _player != null ? _player.Character : null;
            if (character == null)
            {
                GUILayout.Label("(no player linked)");
                return;
            }

            Experience xp = character.Experience;
            GUILayout.Label($"Level {xp.Level}    XP {xp.Total}");
            DrawXpBar(xp);
            if (!string.IsNullOrEmpty(_lastReward)) GUILayout.Label(_lastReward);
        }

        private void DrawXpBar(Experience xp)
        {
            Rect bar = GUILayoutUtility.GetRect(1f, BarHeight, GUILayout.ExpandWidth(true));
            float fraction = xp.XpForNextLevel > 0 ? (float)xp.XpIntoLevel / xp.XpForNextLevel : 0f;

            Color previous = GUI.color;
            GUI.color = Color.gray;
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * fraction, bar.height), Texture2D.whiteTexture);
            GUI.color = previous;

            if (_barLabel == null) _barLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(bar, $"{xp.XpIntoLevel} / {xp.XpForNextLevel}", _barLabel);
        }

        private void DrawTasks()
        {
            System.Guid? toRemove = null;
            System.Guid? toMove = null;
            int moveIndex = 0;
            var items = _tasks.Items;

            for (int i = 0; i < items.Count; i++)
            {
                TaskItem task = items[i];
                GUILayout.BeginHorizontal();

                GUI.enabled = i > 0;
                if (GUILayout.Button("▲", GUILayout.Width(ReorderWidth))) { toMove = task.Id; moveIndex = i - 1; }
                GUI.enabled = i < items.Count - 1;
                if (GUILayout.Button("▼", GUILayout.Width(ReorderWidth))) { toMove = task.Id; moveIndex = i + 1; }
                GUI.enabled = true;

                if (task.IsDone)
                {
                    GUI.enabled = false;
                    GUILayout.Toggle(true, string.Empty, GUILayout.Width(CheckboxWidth));
                    GUI.enabled = true;
                    GUILayout.Label($"{task.Title}  (done)");
                }
                else
                {
                    if (GUILayout.Toggle(false, string.Empty, GUILayout.Width(CheckboxWidth)))
                        CompleteTask(task);

                    string edited = GUILayout.TextField(task.Title);
                    if (edited != task.Title) task.Edit(edited, task.Description);
                    GUILayout.Label($"+{task.XpReward}", GUILayout.Width(RewardWidth));
                }

                if (GUILayout.Button("x", GUILayout.Width(RemoveWidth))) toRemove = task.Id;
                GUILayout.EndHorizontal();
            }

            // Apply the pending edit after the loop, so the list is not mutated while iterating.
            if (toMove.HasValue) _tasks.Move(toMove.Value, moveIndex);
            if (toRemove.HasValue) _tasks.Remove(toRemove.Value);
        }

        private void CompleteTask(TaskItem task)
        {
            int reward = _tasks.Complete(task.Id);
            if (reward <= 0) return;

            PlayerCharacter character = _player != null ? _player.Character : null;
            if (character == null)
            {
                _lastReward = $"+{reward} XP (no player linked)";
                return;
            }

            int levels = character.GainExperience(reward);
            _lastReward = levels > 0
                ? $"LEVEL UP!  +{reward} XP  ->  Lv {character.Experience.Level}  (ATK {character.AttackPower}, healed)"
                : $"+{reward} XP";
        }

        private void DrawAddRow()
        {
            GUILayout.Label("New task");
            _newTitle = GUILayout.TextField(_newTitle);
            _newDifficulty = GUILayout.SelectionGrid(_newDifficulty, DifficultyNames, DifficultyNames.Length);

            if (GUILayout.Button("Add") && !string.IsNullOrWhiteSpace(_newTitle))
            {
                _tasks.Add(_newTitle, (TaskDifficulty)_newDifficulty);
                _newTitle = string.Empty;
            }
        }
    }
}
