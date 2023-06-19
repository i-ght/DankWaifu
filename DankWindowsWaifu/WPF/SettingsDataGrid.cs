using DankWaifu.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DankWindowsWaifu.WPF
{
    public class SettingsDataGrid
    {
        private readonly Window _owner;
        private readonly Grid _content;

        private readonly Dictionary<string, SettingPrimitive> _primitives;
        private readonly Dictionary<string, SettingCollection> _collections;
        private readonly ObservableCollection<SettingObj> _collection;

        public DataGrid DataGrid { get; private set; }

        public SettingsDataGrid(Window owner, Grid content, ObservableCollection<SettingObj> collection)
        {
            _primitives = new Dictionary<string, SettingPrimitive>();
            _collections = new Dictionary<string, SettingCollection>();

            _owner = owner;
            _content = content;
            _collection = collection;

            AddValuesToDicts();
        }

        /// <summary>
        /// Save all primitive type settings
        /// </summary>
        public void SavePrimitives()
        {
            foreach (var item in _primitives)
                DankWaifu.Sys.Settings.Save(item.Key, item.Value.ValueObj);
        }

        /// <summary>
        /// Gets the Keyword list object with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetKeywordsList(string key)
        {
            if (!_collections.ContainsKey(key))
                return new Dictionary<string, List<string>>();

            var settingCollection = _collections[key];
            if (!(settingCollection is SettingKeywords))
                return new Dictionary<string, List<string>>();

            var settingKeywords = (SettingKeywords)_collections[key];
            return settingKeywords.Value;
        }

        /// <summary>
        /// Gets the Queue object with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Queue<string> GetQueue(string key)
        {
            if (!_collections.ContainsKey(key))
                return new Queue<string>();

            var settingCollection = _collections[key];
            if (!(settingCollection is SettingQueue))
                return new Queue<string>();

            var settingQueue = (SettingQueue)settingCollection;
            return settingQueue.Value;
        }

        /// <summary>
        /// Gets the Queue object with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ConcurrentQueue<string> GetConcurrentQueue(string key)
        {
            if (!_collections.ContainsKey(key))
                return new ConcurrentQueue<string>();

            var settingCollection = _collections[key];
            if (!(settingCollection is SettingConcurrentQueue))
                return new ConcurrentQueue<string>();

            var settingQueue = (SettingConcurrentQueue)settingCollection;
            return settingQueue.Value;
        }

        public FileStream GetFileStream(string key)
        {
            if (!_collections.ContainsKey(key))
                return null;

            var settingCollection = _collections[key];
            if (!(settingCollection is SettingFileStream))
                return null;

            var settingFileStream = (SettingFileStream)_collections[key];
            return settingFileStream.Value;
        }

        /// <summary>
        /// Gets the Keyword list object with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //public Dictionary<string, List<string>> GetKeywordsList(string key)
        //{
        //    if (!_collections.ContainsKey(key))
        //        return new Dictionary<string, List<string>>();

        //    var settingCollection = _collections[key];
        //    if (!(settingCollection is SettingKeywords))
        //        return new Dictionary<string, List<string>>();

        //    var settingKeywords = (SettingKeywords)_collections[key];
        //    return settingKeywords.Value;
        //}

        /// <summary>
        /// Gets the list object with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> GetList(string key)
        {
            if (!_collections.ContainsKey(key))
                return new List<string>();

            var settingCollection = _collections[key];
            if (!(settingCollection is SettingList))
                return new List<string>();

            var settingQueue = (SettingList)settingCollection;
            return settingQueue.Value;
        }

        /// <summary>
        /// Organizes collections vs primitives
        /// </summary>
        private void AddValuesToDicts()
        {
            foreach (var item in _collection)
            {
                if (item is SettingCollection)
                {
                    var settingCollection = (SettingCollection)item;
                    _collections.Add(settingCollection.Key, settingCollection);
                    continue;
                }

                if (!(item is SettingPrimitive))
                    continue;

                var settingPrimitive = (SettingPrimitive)item;
                _primitives.Add(item.Key, settingPrimitive);
            }
        }

        /// <summary>
        /// Adds a data grid with all the specified settings to the UI
        /// </summary>
        public void CreateUi()
        {
            DataGrid = new DataGrid
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                AutoGenerateColumns = false
            };

            var keyColumn = new DataGridTextColumn
            {
                Header = "Key",
                Width = DataGridLength.SizeToCells,
                Binding = new Binding(nameof(SettingObj.KeyLabel)),
                IsReadOnly = true,
                MinWidth = 50
            };

            var typeColumn = new DataGridTextColumn
            {
                Header = "Type",
                Width = DataGridLength.SizeToCells,
                Binding = new Binding(nameof(SettingObj.TypeLabel)),
                IsReadOnly = true,
                MinWidth = 50
            };

            var valueColumn = new DataGridTextColumn
            {
                Header = "Value",
                Width = new DataGridLength(0.8, DataGridLengthUnitType.Star),
                Binding = new Binding(nameof(SettingCollection.ValueLabel))
            };

            DataGrid.Columns.Add(keyColumn);
            DataGrid.Columns.Add(typeColumn);
            DataGrid.Columns.Add(valueColumn);

            DataGrid.ItemsSource = _collection;
            DataGrid.BeginningEdit += OnDataGridOnBeginningEdit;
            DataGrid.MouseDoubleClick += OnDoubleClick;

            _content.Children.Add(DataGrid);
        }

        /// <summary>
        /// Clears a settings collection if double right clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var index = dataGrid.SelectedIndex;
            if (index < 0)
                return;

            var item = _collection[index];

            if (!(item is SettingCollection))
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Right:
                    if (item is SettingQueue)
                    {
                        var settingQueue = (SettingQueue)item;
                        settingQueue.Value.Clear();
                        settingQueue.FileName = string.Empty;
                        settingQueue.ValueLabel = string.Empty;
                        return;
                    }

                    if (item is SettingConcurrentQueue)
                    {
                        var settingQueue = (SettingConcurrentQueue)item;
                        settingQueue.Value.Clear();
                        settingQueue.FileName = string.Empty;
                        settingQueue.ValueLabel = string.Empty;
                        return;
                    }

                    if (item is SettingFileStream)
                    {
                        var settingFileStream = (SettingFileStream)item;
                        settingFileStream.Value.Close();
                        settingFileStream.FileName = string.Empty;
                        settingFileStream.ValueLabel = string.Empty;
                        return;
                    }

                    if (item is SettingKeywords)
                    {
                        var settingKeywords = (SettingKeywords)item;
                        settingKeywords.Value.Clear();
                        settingKeywords.FileName = string.Empty;
                        settingKeywords.ValueLabel = string.Empty;
                        return;
                    }

                    if (!(item is SettingList))
                        return;

                    var settingList = (SettingList)item;
                    settingList.Value.Clear();
                    settingList.FileName = string.Empty;
                    settingList.ValueLabel = string.Empty;
                    return;
            }
        }

        /// <summary>
        /// If the selected index is a primitive, allow the value to be edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dataGridBeginningEditEventArgs"></param>
        private void OnDataGridOnBeginningEdit(object sender, DataGridBeginningEditEventArgs dataGridBeginningEditEventArgs)
        {
            var index = dataGridBeginningEditEventArgs.Row.GetIndex();
            var item = _collection[index];
            if (!(item is SettingCollection))
                return;

            dataGridBeginningEditEventArgs.Cancel = true;
            HandleEditCollection(item);
        }

        /// <summary>
        /// Show an open file dialog to select a setting collections source
        /// </summary>
        /// <param name="item"></param>
        private void HandleEditCollection(SettingObj item)
        {
            var settingCollection = item as SettingCollection;
            if (settingCollection == null)
                return;

            if (item is SettingQueue)
            {
                var collection = (Queue<string>)item.ValueObj;
                var fileName = WPFHelpers.SelectFile(_owner, item.KeyLabel);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                collection.LoadFromFile(fileName);
                settingCollection.FileName = fileName;
                settingCollection.ValueLabel = $"[{collection.Count:N0}] | {fileName}";
                return;
            }

            if (item is SettingConcurrentQueue)
            {
                var collection = (ConcurrentQueue<string>)item.ValueObj;
                var fileName = WPFHelpers.SelectFile(_owner, item.KeyLabel);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                collection.LoadFromFile(fileName);
                settingCollection.FileName = fileName;
                settingCollection.ValueLabel = $"[{collection.Count:N0}] | {fileName}";
                return;
            }

            if (item is SettingFileStream)
            {
                var settingStream = (SettingFileStream)item;
                var fileName = WPFHelpers.SelectFile(_owner, settingCollection.KeyLabel);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                settingStream.Load(fileName);
                return;
            }

            if (item is SettingKeywords)
            {
                var settingKeywords = (SettingKeywords)item;
                var fileName = WPFHelpers.SelectFile(_owner, item.KeyLabel);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                if (!settingKeywords.LoadKeywordsFromFile(fileName))
                    return;

                settingKeywords.FileName = fileName;
                settingKeywords.ValueLabel = $"[{settingKeywords.Value.Count:N0}]| {fileName}";
                return;
            }

            if (!(item is SettingList))
                return;

            {
                var collection = (List<string>)item.ValueObj;
                var fileName = WPFHelpers.SelectFile(_owner, item.KeyLabel);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                collection.LoadFromFile(fileName);
                settingCollection.FileName = fileName;
                settingCollection.ValueLabel = $"[{collection.Count:N0}] | {fileName}";

                var args = new CollectionLoadedRaisedEventArgs(
                    settingCollection.Key,
                    settingCollection.FileName,
                    collection.Count,
                    collection
                );
                CollectionLoadedRaised?.Invoke(this, args);
            }
        }

        public event EventHandler<CollectionLoadedRaisedEventArgs> CollectionLoadedRaised;
    }
}