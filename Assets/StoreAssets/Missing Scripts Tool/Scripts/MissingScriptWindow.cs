#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AisenTools.MissingScripts
{
    public class MissingScriptWindow : EditorWindow
    {
        private const string TITLE = "Missing Scripts Tool";
        private const string VERSION = "2.0.0 (Unity 6000.2+)";

        private Vector2 _scroll;
        private string _search = string.Empty;

        private bool _includeOpenScenes = true;
        private bool _includeAllScenes = false;
        private bool _includePrefabs = true;
        private bool _includeInactive = true;
        private bool _showOnlySelectedFolders = false;
        private bool _dryRun = false;
        private bool _makeBackups = true;

        private readonly List<Record> _results = new List<Record>();
        private readonly HashSet<string> _uniqueKeys = new HashSet<string>();
        private double _lastScanTime;

        private static Texture2D _headerTex, _badgeBlue, _badgeRed, _badgeGreen, _boxTex;
        private static GUIStyle _headerStyle, _pillStyle, _pillWarnStyle, _pillOkStyle, _toolbarStyle, _listRowStyle, _hintStyle;

        private const string BACKUP_ROOT = "Assets/Missing Scripts Tool/Backups";
        private const string LOG_DIR = "Assets/Missing Scripts Tool/Logs";

        [MenuItem("Tools/Missing Scripts Tool %#u")] // Ctrl/Cmd + Shift + U
        public static void ShowWindow()
        {
            var w = GetWindow<MissingScriptWindow>(TITLE);
            w.minSize = new Vector2(880, 560);
            w.Show();
        }

        private void OnEnable()
        {
            BuildStyles();
        }

        private void OnGUI()
        {
            if (_headerTex == null) BuildStyles();

            DrawHeader();
            GUILayout.Space(8);
            DrawToolbar();
            GUILayout.Space(6);
            DrawResults();
            GUILayout.Space(4);
            DrawFooter();
        }

        #region UI
        private void DrawHeader()
        {
            var r = GUILayoutUtility.GetRect(position.width, 84f);
            GUI.DrawTexture(r, _headerTex, ScaleMode.StretchToFill);

            GUILayout.BeginArea(r);
            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(14);
                GUILayout.Label(TITLE, _headerStyle, GUILayout.Height(40));
                GUILayout.FlexibleSpace();
                GUILayout.Label(VERSION, EditorStyles.miniLabel);
                GUILayout.Space(16);
            }
            GUILayout.EndArea();
        }

        private void DrawToolbar()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    _includeOpenScenes = GUILayout.Toggle(_includeOpenScenes, new GUIContent("Open Scenes"), _toolbarStyle, GUILayout.Height(26));
                    _includeAllScenes = GUILayout.Toggle(_includeAllScenes, new GUIContent("All Scenes in Project"), _toolbarStyle, GUILayout.Height(26));
                    _includePrefabs = GUILayout.Toggle(_includePrefabs, new GUIContent("Prefabs"), _toolbarStyle, GUILayout.Height(26));
                    _includeInactive = GUILayout.Toggle(_includeInactive, new GUIContent("Include Inactive"), _toolbarStyle, GUILayout.Height(26));
                    _showOnlySelectedFolders = GUILayout.Toggle(_showOnlySelectedFolders, new GUIContent("Only Selected Folders/Assets"), _toolbarStyle, GUILayout.Height(26));
                    GUILayout.FlexibleSpace();

                    GUILayout.Label("Search:", GUILayout.Width(52));
                    _search = GUILayout.TextField(_search, GUILayout.MaxWidth(220));
                }

                using (new GUILayout.HorizontalScope())
                {
                    _dryRun = GUILayout.Toggle(_dryRun, new GUIContent("Dry Run"), _toolbarStyle, GUILayout.Height(22));
                    _makeBackups = GUILayout.Toggle(_makeBackups, new GUIContent("Backups"), _toolbarStyle, GUILayout.Height(22));
                    GUILayout.Space(8);

                    if (GUILayout.Button("Scan", GUILayout.Height(28), GUILayout.Width(140)))
                        Scan();
                    if (GUILayout.Button("Clear", GUILayout.Height(28), GUILayout.Width(100)))
                        ClearResults();

                    GUILayout.Space(12);
                    using (new newEnabledScope(_results.Count > 0))
                    {
                        if (GUILayout.Button("Remove Missing in Prefabs", GUILayout.Height(28), GUILayout.Width(220)))
                            RemoveInPrefabs();
                        if (GUILayout.Button("Remove Missing in Open Scenes", GUILayout.Height(28), GUILayout.Width(260)))
                            RemoveInOpenScenes();
                    }
                }
            }
        }

        private void DrawResults()
        {
            var filtered = 0;
            foreach (var r in _results) if (PassesSearch(r)) filtered++;

            using (new GUILayout.HorizontalScope())
            {
                DrawPill($"Found {_results.Count}", _pillStyle);
                DrawPill($"Shown {filtered}", _pillOkStyle);
                DrawPill($"Prefabs {CountBy(Record.SourceKind.Prefab)}", _pillStyle);
                DrawPill($"Scenes {CountBy(Record.SourceKind.Scene)}", _pillStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy Log", GUILayout.Width(100), GUILayout.Height(22)))
                    EditorGUIUtility.systemCopyBuffer = BuildClipboard();
            }

            GUILayout.Space(4);
            using (var sv = new GUILayout.ScrollViewScope(_scroll))
            {
                _scroll = sv.scrollPosition;
                foreach (var rec in _results)
                {
                    if (!PassesSearch(rec)) continue;
                    DrawRow(rec);
                }
            }
        }

        private void DrawRow(Record r)
        {
            using (new GUILayout.HorizontalScope(_listRowStyle))
            {
                var badge = r.Kind == Record.SourceKind.Prefab ? _badgeBlue : _badgeRed;
                GUILayout.Label(badge, GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Space(6);
                GUILayout.Label(r.Kind.ToString(), GUILayout.Width(64));

                if (GUILayout.Button(new GUIContent(Path.GetFileName(r.AssetPath), r.AssetPath), EditorStyles.linkLabel, GUILayout.Width(260)))
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(r.AssetPath);
                    Selection.activeObject = obj; EditorGUIUtility.PingObject(obj);
                }

                GUILayout.Label(r.ObjectPath, GUILayout.ExpandWidth(true));

                using (new newEnabledScope(r.Kind == Record.SourceKind.Scene))
                {
                    if (GUILayout.Button("Open Scene & Select", GUILayout.Width(160)))
                        OpenSceneAndSelect(r);
                }
                using (new newEnabledScope(r.Kind == Record.SourceKind.Prefab))
                {
                    if (GUILayout.Button("Open Prefab", GUILayout.Width(120)))
                        AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(r.AssetPath));
                }
            }
        }

        private void DrawFooter()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Tip: Use ‘Only Selected Folders/Assets’ to limit scans. Dry Run simulates deletions without writing.", _hintStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Last scan: {(EditorApplication.timeSinceStartup - _lastScanTime < 0.1 ? "–" : $"{DateTime.Now:t}")}");
            }
        }

        private void DrawPill(string text, GUIStyle style)
        {
            GUILayout.Label(text, style, GUILayout.Height(20));
            GUILayout.Space(6);
        }
        #endregion

        #region Actions
        private void Scan()
        {
            ClearResults();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var folders = _showOnlySelectedFolders ? GetSelectedSearchRoots() : null; // null = whole project

                if (_includePrefabs)
                    ScanPrefabs(folders);

                if (_includeOpenScenes)
                    ScanOpenScenes();

                if (_includeAllScenes)
                    ScanAllScenesInProject(folders);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _lastScanTime = EditorApplication.timeSinceStartup;
            }
            sw.Stop();
            Repaint();
            Debug.Log($"[{TITLE}] Scan finished in {sw.ElapsedMilliseconds} ms. Found {_results.Count} objects with missing scripts.");
        }

        private static string ProjectRootAbs =>
            System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));

        private void RemoveInPrefabs()
        {
            var prefabPaths = new HashSet<string>();
            foreach (var r in _results) if (r.Kind == Record.SourceKind.Prefab) prefabPaths.Add(r.AssetPath);
            if (prefabPaths.Count == 0) { EditorUtility.DisplayDialog(TITLE, "No prefabs in results.", "OK"); return; }

            if (!Confirm("Remove missing components in listed prefabs?")) return;

            var idx = 0; var total = prefabPaths.Count;
            foreach (var path in prefabPaths)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Fix Prefabs", path, (++idx) / (float)total)) break;

                var root = PrefabUtility.LoadPrefabContents(path);
                var modified = RemoveMissingRecursive(root);
                if (modified && !_dryRun)
                {
                    if (_makeBackups) WriteBackup(path);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    AssetDatabase.SaveAssets();
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (!_dryRun)
            {
                AssetDatabase.Refresh();
                Scan();
            }
        }

        private void RemoveInOpenScenes()
        {
            if (EditorSceneManager.sceneCount == 0) { EditorUtility.DisplayDialog(TITLE, "No open scenes.", "OK"); return; }
            if (!Confirm("Remove missing components in OPEN scenes?")) return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (!sc.isLoaded) continue;
                if (EditorUtility.DisplayCancelableProgressBar("Fix Scenes", sc.path, (i + 1) / (float)SceneManager.sceneCount)) break;

                var changed = false;
                foreach (var root in sc.GetRootGameObjects())
                    changed |= RemoveMissingRecursive(root);

                if (changed && !_dryRun)
                    EditorSceneManager.MarkSceneDirty(sc);
            }

            if (!_dryRun)
            {
                EditorSceneManager.SaveOpenScenes();
                Scan();
            }
            EditorUtility.ClearProgressBar();
        }
        #endregion

        #region Scanning
        private void ScanOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (!sc.isLoaded) continue;
                if (EditorUtility.DisplayCancelableProgressBar("Scan Open Scenes", sc.path, (i + 1) / (float)SceneManager.sceneCount)) break;
                foreach (var root in sc.GetRootGameObjects())
                    CollectMissingRecursive(root, sc.path, Record.SourceKind.Scene);
            }
        }

        private void ScanAllScenesInProject(string[] roots)
        {
            var guids = roots == null ? AssetDatabase.FindAssets("t:Scene") : AssetDatabase.FindAssets("t:Scene", roots);
            var opened = CaptureOpenScenesState();

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (ShouldSkipPath(path)) continue;
                if (EditorUtility.DisplayCancelableProgressBar("Scan Project Scenes", path, (i + 1) / (float)guids.Length)) break;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    CollectMissingRecursive(root, path, Record.SourceKind.Scene);
            }

            RestoreOpenScenesState(opened);
        }

        private void ScanPrefabs(string[] roots)
        {
            var guids = roots == null ? AssetDatabase.FindAssets("t:Prefab") : AssetDatabase.FindAssets("t:Prefab", roots);
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (ShouldSkipPath(path)) continue;
                if (EditorUtility.DisplayCancelableProgressBar("Scan Prefabs", path, (i + 1) / (float)guids.Length)) break;

                try
                {
                    var root = PrefabUtility.LoadPrefabContents(path);
                    CollectMissingRecursive(root, path, Record.SourceKind.Prefab);
                    PrefabUtility.UnloadPrefabContents(root);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[{TITLE}] Prefab error {path}: {e.Message}");
                }
            }
        }

        private void CollectMissingRecursive(GameObject go, string assetPath, Record.SourceKind kind)
        {
            var comps = ListComponents(go);
            var hasMissing = false;
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    hasMissing = true; break;
                }
            }

            if (hasMissing)
            {
                var rec = new Record
                {
                    Kind = kind,
                    AssetPath = assetPath,
                    ObjectPath = GetTransformPath(go.transform)
                };
                var key = rec.Key;
                if (_uniqueKeys.Add(key)) _results.Add(rec);
            }

            foreach (Transform ch in go.transform)
            {
                if (!_includeInactive && !ch.gameObject.activeInHierarchy) continue;
                CollectMissingRecursive(ch.gameObject, assetPath, kind);
            }
        }

        private static int RemoveMissingRecursiveCount(GameObject go)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0) EditorUtility.SetDirty(go);

            foreach (Transform ch in go.transform)
                removed += RemoveMissingRecursiveCount(ch.gameObject);

            return removed;
        }

        private bool RemoveMissingRecursive(GameObject go)
        {
            return RemoveMissingRecursiveCount(go) > 0;
        }
        #endregion

        #region Utils
        private static Component[] ListComponents(GameObject go)
        {
            return go.GetComponents(typeof(Component));
        }

        private static string GetTransformPath(Transform t)
        {
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        private static bool ShouldSkipPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            if (path.StartsWith("Packages/")) return true;
            if (path.Contains("/PackageCache/")) return true;
            return false;
        }

        private bool PassesSearch(Record r)
        {
            if (string.IsNullOrEmpty(_search)) return true;
            var s = _search.IndexOf('"') >= 0 ? _search.Trim('"') : _search;
            var cmp = StringComparison.OrdinalIgnoreCase;
            return r.AssetPath.IndexOf(s, cmp) >= 0 || r.ObjectPath.IndexOf(s, cmp) >= 0 || r.Kind.ToString().IndexOf(s, cmp) >= 0;
        }

        private int CountBy(Record.SourceKind kind)
        {
            var c = 0; foreach (var r in _results) if (r.Kind == kind) c++; return c;
        }

        private void ClearResults()
        {
            _results.Clear();
            _uniqueKeys.Clear();
        }

        private static bool Confirm(string msg) => EditorUtility.DisplayDialog(TITLE, msg, "Yes", "No");

        private string BuildClipboard()
        {
            var sw = new System.Text.StringBuilder();
            sw.AppendLine($"{TITLE} — {DateTime.Now:yyyy-MM-dd HH:mm}");
            foreach (var r in _results)
                sw.AppendLine($"{r.Kind};{r.AssetPath};{r.ObjectPath}");
            return sw.ToString();
        }

        private static string[] GetSelectedSearchRoots()
        {
            var sel = Selection.assetGUIDs;
            if (sel == null || sel.Length == 0) return null;

            var list = new List<string>();
            foreach (var g in sel)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (File.Exists(path))
                {
                    var dir = Path.GetDirectoryName(path)?.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(dir) && !list.Contains(dir)) list.Add(dir);
                }
                else if (Directory.Exists(path))
                {
                    var dir = path.Replace('\\', '/');
                    if (!list.Contains(dir)) list.Add(dir);
                }
            }
            return list.Count == 0 ? null : list.ToArray();
        }

        private static OpenScenesState CaptureOpenScenesState()
        {
            var state = new OpenScenesState
            {
                Paths = new List<string>(),
                ActiveIndex = SceneManager.GetActiveScene().buildIndex
            };
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (sc.isLoaded) state.Paths.Add(sc.path);
            }
            return state;
        }

        private static void RestoreOpenScenesState(OpenScenesState state)
        {
            if (state == null || state.Paths == null || state.Paths.Count == 0) return;
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            for (int i = 0; i < state.Paths.Count; i++)
            {
                var mode = i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive;
                EditorSceneManager.OpenScene(state.Paths[i], mode);
            }
        }

        private static void OpenSceneAndSelect(Record r)
        {
            if (r.Kind != Record.SourceKind.Scene) return;
            var sc = EditorSceneManager.OpenScene(r.AssetPath, OpenSceneMode.Single);
            GameObject target = null;
            foreach (var root in sc.GetRootGameObjects())
            {
                if (GetTransformPath(root.transform) == r.ObjectPath) { target = root; break; }
                var child = root.transform.Find(r.ObjectPath.Substring(r.ObjectPath.IndexOf('/') + 1));
                if (child != null) { target = child.gameObject; break; }
            }
            if (target != null) { Selection.activeGameObject = target; EditorGUIUtility.PingObject(target); }
        }

        private static string ToProjectAbsolute(string assetsRelative)
        {
            var p = System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectRootAbs, assetsRelative));
            return p.Replace('\\', '/');
        }

        private static string ToAssetsRelative(string projectAbsolute)
        {
            var pr = ProjectRootAbs.Replace('\\', '/');
            var abs = System.IO.Path.GetFullPath(projectAbsolute).Replace('\\', '/');
            if (!abs.StartsWith(pr)) throw new Exception("Path outside project: " + abs);
            return "Assets" + abs.Substring(pr.Length);
        }

        private static string StripAssetsPrefix(string path)
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("Assets/")) return path.Substring("Assets/".Length);
            if (path == "Assets") return string.Empty;
            return path;
        }

        private static void AppendLog(string line)
        {
            try
            {
                System.IO.Directory.CreateDirectory(LOG_DIR);
                var file = System.IO.Path.Combine(LOG_DIR, $"missing-scripts-{DateTime.Now:yyyyMMdd}.log").Replace('\\', '/');
                System.IO.File.AppendAllText(file, $"{DateTime.Now:HH:mm:ss} {line}\n");
                AssetDatabase.ImportAsset(file);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{TITLE}] Log write failed: {e.Message}");
            }
        }

        private static void WriteBackup(string assetPath)
        {
            try
            {
                var relDir = System.IO.Path.GetDirectoryName(assetPath) ?? "Assets";
                var relUnderAssets = StripAssetsPrefix(relDir);

                var backupRelDir = System.IO.Path.Combine(BACKUP_ROOT, relUnderAssets).Replace('\\', '/');
                System.IO.Directory.CreateDirectory(backupRelDir);

                var fileName = System.IO.Path.GetFileName(assetPath);

                var srcAbs = ToProjectAbsolute(assetPath);
                var dstAbs = ToProjectAbsolute(System.IO.Path.Combine(backupRelDir, fileName).Replace('\\', '/'));

                System.IO.File.Copy(srcAbs, dstAbs, true);

                var dstAssets = ToAssetsRelative(dstAbs);
                AssetDatabase.ImportAsset(dstAssets);

                AppendLog($"BACKUP: {assetPath} -> {dstAssets}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{TITLE}] Backup failed for {assetPath}: {e.Message}");
                AppendLog($"BACKUP FAIL: {assetPath} :: {e.Message}");
            }
        }

        private void BuildStyles()
        {
            _headerTex = MakeGradientTex(new Color(0.09f, 0.12f, 0.18f), new Color(0.06f, 0.08f, 0.12f), 8, 84);
            _boxTex = MakeTex(new Color(0.11f, 0.15f, 0.2f));
            _badgeBlue = MakeCircleTex(new Color(0.4f, 0.7f, 1.0f));
            _badgeRed = MakeCircleTex(new Color(1.0f, 0.45f, 0.45f));
            _badgeGreen = MakeCircleTex(new Color(0.45f, 1.0f, 0.6f));

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.85f, 0.93f, 1f) }
            };

            _toolbarStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 22
            };

            _pillStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 2, 2),
                normal = { textColor = new Color(0.85f, 0.93f, 1f), background = _boxTex }
            };
            _pillWarnStyle = new GUIStyle(_pillStyle); _pillWarnStyle.normal.textColor = new Color(1f, .8f, .6f);
            _pillOkStyle = new GUIStyle(_pillStyle); _pillOkStyle.normal.textColor = new Color(.8f, 1f, .8f);

            _listRowStyle = new GUIStyle("RL Background");
            _listRowStyle.padding = new RectOffset(8, 8, 6, 6);
            _listRowStyle.margin = new RectOffset(4, 4, 2, 2);

            _hintStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c); t.Apply();
            return t;
        }
        private static Texture2D MakeGradientTex(Color top, Color bottom, int w, int h)
        {
            var t = new Texture2D(w, h) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < h; y++)
            {
                var k = (float)y / (h - 1);
                var c = Color.Lerp(bottom, top, k);
                for (int x = 0; x < w; x++) t.SetPixel(x, y, c);
            }
            t.Apply();
            return t;
        }
        private static Texture2D MakeCircleTex(Color c)
        {
            const int s = 20; var t = new Texture2D(s, s) { hideFlags = HideFlags.HideAndDontSave };
            var r = (s - 1) * 0.5f; var r2 = r * r; var cx = r; var cy = r;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    var dx = x - cx; var dy = y - cy;
                    var a = dx * dx + dy * dy <= r2 ? 1f : 0f;
                    var col = Color.Lerp(Color.clear, c, a);
                    t.SetPixel(x, y, col);
                }
            t.Apply();
            return t;
        }
        #endregion

        #region Types
        [Serializable]
        private class OpenScenesState
        {
            public List<string> Paths;
            public int ActiveIndex;
        }

        [Serializable]
        private class Record
        {
            public enum SourceKind { Prefab, Scene }
            public SourceKind Kind;
            public string AssetPath;
            public string ObjectPath;

            public string Key => $"{Kind}|{AssetPath}|{ObjectPath}";
        }

        private readonly struct newEnabledScope : IDisposable
        {
            private readonly bool _prev;
            public newEnabledScope(bool enabled)
            {
                _prev = GUI.enabled; GUI.enabled = enabled;
            }
            public void Dispose() { GUI.enabled = _prev; }
        }
        #endregion
    }
}

[InitializeOnLoad]
public class ScriptIconHandler
{
    static ScriptIconHandler()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        if (assetPath.EndsWith("MissingScriptWindow.cs"))
        {
            if (Event.current.type == EventType.MouseDown && selectionRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                AisenTools.MissingScripts.MissingScriptWindow.ShowWindow();
            }
        }
    }
}
#endif