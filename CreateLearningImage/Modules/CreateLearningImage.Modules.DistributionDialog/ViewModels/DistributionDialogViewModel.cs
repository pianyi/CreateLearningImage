using CreateLearningImage.Core;
using CreateLearningImage.Core.Datas;
using CreateLearningImage.Core.Events;
using CreateLearningImage.Core.Mvvm;
using CreateLearningImage.Services.Datas;
using CreateLearningImage.Services.ViewMains;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Events;
using Prism.Navigation;
using Prism.Services.Dialogs;
using PrismExpansion.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using Unity;

namespace CreateLearningImage.Modules.DistributionDialog.ViewModels
{
    /// <summary>
    /// 画像選別ダイアログ
    /// </summary>
    public class DistributionDialogViewModel : DisposableBindableViewModelBase, ICustomizeDialogAware, IDisposable, IDestructible
    {
        /// <summary>
        /// ログクラス
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Metro用ダイアログ制御クラス
        /// </summary>
        [Dependency]
        internal IDialogCoordinator DialogCoordinator { get; set; }

        /// <summary>
        /// イベント登録クラス
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// メイン画面操作サービス
        /// </summary>
        private readonly ViewMainControlService _mainControlService;

        /// <summary>
        /// 画面クローズアクション(フォームの閉じるボタン以外閉じる方法が無いので、利用しない)
        /// </summary>
        public event Action<IDialogResult> RequestClose;

        /// <summary>
        /// タイトルを取得・設定します
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// ウィンド表示位置(左)を指定
        /// </summary>
        public double Left { get; set; } = double.NaN;

        /// <summary>
        /// ウィンド表示位置(上)を指定
        /// </summary>
        public double Top { get; set; } = double.NaN;

        /// <summary>
        /// オーナーウィンドウの指定を拒否します
        /// </summary>
        /// <remarks>解析画面はオーナーを指定しないようにする</remarks>
        public bool UnsetOwner { get; set; } = true;

        /// <summary>
        /// 現在表示中のページ番号
        /// </summary>
        public ReactivePropertySlim<int> PageNumber { get; set; }

        /// <summary>
        /// 最大のページ番号
        /// </summary>
        public ReactivePropertySlim<int> MaxPageNumber { get; set; }

        /// <summary>
        /// 切り抜かれた画像データ
        /// </summary>
        public ReadOnlyReactiveCollection<ImageData> Faces { get; set; }

        /// <summary>
        /// 画面表示イメージ
        /// </summary>
        public ReactivePropertySlim<BitmapSource> Images { get; set; }

        /// <summary>
        /// Prevボタン動作
        /// </summary>
        public ReactiveCommand PrevCommand { get; set; }

        /// <summary>
        /// Nextボタン動作
        /// </summary>
        public ReactiveCommand NextCommand { get; set; }

        /// <summary>
        /// Deleteキー動作
        /// </summary>
        public ReactiveCommand DeleteCommand { get; set; }

        /// <summary>
        /// 上キー動作
        /// </summary>
        public ReactiveCommand UpCommand { get; set; }

        /// <summary>
        /// 下キー動作
        /// </summary>
        public ReactiveCommand DownCommand { get; set; }

        /// <summary>
        /// 左キー動作
        /// </summary>
        public ReactiveCommand LeftCommand { get; set; }

        /// <summary>
        /// →キー動作
        /// </summary>
        public ReactiveCommand RightCommand { get; set; }

        /// <summary>
        /// 数字キー動作
        /// </summary>
        public ReactiveCommand<string> NumberCommand { get; set; }

        /// <summary>
        /// 出力先情報リスト
        /// </summary>
        public ReactiveCollection<ComboBoxItem> OutputInfoList { get; set; }

        /// <summary>
        /// 出力先情報の選択中セットアップ情報
        /// </summary>
        public ReactiveProperty<string> SelectedOutputInfo { get; set; }
        /// <summary>
        /// 出力先情報の選択中のIndex番号
        /// </summary>
        public ReactiveProperty<int> SelectedIndex { get; set; }

        /// <summary>
        /// アイテムの追加コマンド
        /// </summary>
        public ReactiveCommand AppendDirectoryCommand { get; set; }

        /// <summary>
        /// 出力フォルダ
        /// </summary>
        private string _outputBasePath = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="mainControlService"></param>
        public DistributionDialogViewModel(IEventAggregator eventAggregator,
                                           ViewMainControlService mainControlService)
        {
            _eventAggregator = eventAggregator;
            _mainControlService = mainControlService;

            PageNumber = new ReactivePropertySlim<int>(0).AddTo(Disposables);
            MaxPageNumber = new ReactivePropertySlim<int>(0).AddTo(Disposables);

            Faces = mainControlService.Faces.ToReadOnlyReactiveCollection().AddTo(Disposables);
            Faces.ObserveAddChanged().Subscribe(AddFaceData);
            Faces.ObserveRemoveChanged().Subscribe(RemoveFaceData);

            Images = new ReactivePropertySlim<BitmapSource>().AddTo(Disposables);

            PrevCommand = new ReactiveCommand().WithSubscribe(PrevClick).AddTo(Disposables);
            NextCommand = new ReactiveCommand().WithSubscribe(NextClick).AddTo(Disposables);
            DeleteCommand = new ReactiveCommand().WithSubscribe(KeyDelete).AddTo(Disposables);
            UpCommand = new ReactiveCommand().WithSubscribe(KeyUp).AddTo(Disposables);
            DownCommand = new ReactiveCommand().WithSubscribe(KeyDown).AddTo(Disposables);
            LeftCommand = new ReactiveCommand().WithSubscribe(KeyLeft).AddTo(Disposables);
            RightCommand = new ReactiveCommand().WithSubscribe(KeyRight).AddTo(Disposables);
            NumberCommand = new ReactiveCommand<string>().WithSubscribe(KeyNumber).AddTo(Disposables);

            AppendDirectoryCommand = new ReactiveCommand().WithSubscribe(DoAppendDirectory).AddTo(Disposables);

            OutputInfoList = new ReactiveCollection<ComboBoxItem>();
            SelectedOutputInfo = new ReactiveProperty<string>(Constants.DirectoryNameOthers);
            SelectedIndex = new ReactiveProperty<int>().AddTo(Disposables);

            _eventAggregator.GetEvent<InitializeEvent>()
                            .Subscribe(Initialize)
                            .AddTo(Disposables);
        }

        /// <summary>
        /// 画面の初期化を行います。
        /// </summary>
        private void Initialize()
        {
            Images.Value = null;
            PageNumber.Value = 0;
            MaxPageNumber.Value = 0;
        }

        /// <summary>
        /// 顔画像が追加された時の動作
        /// </summary>
        /// <param name="imageData">追加された画像情報</param>
        private void AddFaceData(ImageData imageData)
        {
            MaxPageNumber.Value = Faces.Count;
            if (Images.Value == null)
            {
                // 1個目を表示する(メソッド内で+1するので0を指定)
                PageNumber.Value = 0;
                NextClick();
            }
        }

        /// <summary>
        /// 顔画像が削除された時の動作
        /// </summary>
        /// <param name="imageData">削除された画像情報</param>
        private void RemoveFaceData(ImageData imageData)
        {
            MaxPageNumber.Value = Faces.Count;

            if (MaxPageNumber.Value < PageNumber.Value)
            {
                // 最大値を超えた場合は1にする
                PageNumber.Value = 1;
            }

            SetNextSelectedIndex();
        }

        /// <summary>
        /// 前へボタン押下
        /// </summary>
        private void PrevClick()
        {
            // 現在選択中のフォルダを設定します
            SetNowSelectedIndex();

            // 前の画像へ移動
            PageNumber.Value--;
            if (PageNumber.Value < 1)
            {
                // 1より小さくなったら、最大値にする
                PageNumber.Value = MaxPageNumber.Value;
            }

            // 次に表示する画像と対応するフォルダを選択状態にします。
            SetNextSelectedIndex();
        }

        /// <summary>
        /// 次へボタン押下
        /// </summary>
        private void NextClick()
        {
            // 現在選択中のフォルダを設定します
            SetNowSelectedIndex();

            // 次の画像へ移動
            PageNumber.Value++;
            if (MaxPageNumber.Value < PageNumber.Value)
            {
                // 最大値を超えた場合は1にする
                PageNumber.Value = 1;
            }

            // 次に表示する画像と対応するフォルダを選択状態にします。
            SetNextSelectedIndex();
        }

        /// <summary>
        /// 削除キー動作
        /// </summary>
        private void KeyDelete()
        {
            _mainControlService.DeleteAt(PageNumber.Value);
        }

        /// <summary>
        /// 上キー押下
        /// </summary>
        private void KeyUp()
        {
            // 上のコンボボックスを選択する
            SelectedIndex.Value--;
            if (SelectedIndex.Value < 0)
            {
                SelectedIndex.Value = 0;
            }
        }

        /// <summary>
        /// 下キー押下
        /// </summary>
        private void KeyDown()
        {
            // 下のコンボボックスを選択する
            SelectedIndex.Value++;
            if (OutputInfoList.Count <= SelectedIndex.Value)
            {
                SelectedIndex.Value = OutputInfoList.Count - 1;
            }
        }

        /// <summary>
        /// 0～9 の値に合わせてコンボボックスの値を変更します。
        /// </summary>
        /// <param name="num"></param>
        private void KeyNumber(string num)
        {
            if (int.TryParse(num, out int result))
            {
                SelectedIndex.Value = result;
            }
        }

        /// <summary>
        /// 左キー押下
        /// </summary>
        private void KeyLeft()
        {
            PrevClick();
        }

        /// <summary>
        /// 右キー押下
        /// </summary>
        private void KeyRight()
        {
            NextClick();
        }

        /// <summary>
        /// 現在選択中のフォルダ名を画像と紐づけます
        /// </summary>
        private void SetNowSelectedIndex()
        {
            if (0 < PageNumber.Value)
            {
                // データの保存先フォルダを変更
                ImageData nowData = Faces[PageNumber.Value - 1];

                var oldFolderName = nowData.FolderName;
                nowData.FolderName = SelectedOutputInfo.Value;
                _mainControlService.SaveImage(nowData, oldFolderName);
            }
        }

        /// <summary>
        /// 次に表示するフォルダ名を画面に表示します
        /// </summary>
        private void SetNextSelectedIndex()
        {
            int index = PageNumber.Value - 1;

            if (index < Faces.Count)
            {
                ImageData nextData = Faces[index];
                if (string.IsNullOrEmpty(nextData.FolderName))
                {
                    SelectedOutputInfo.Value = Constants.DirectoryNameOthers;
                }
                else
                {
                    SelectedOutputInfo.Value = nextData.FolderName;
                }

                Images.Value = nextData.Image;
            }
            else
            {
                Images.Value = null;
            }
        }

        /// <summary>
        /// フォルダ名入力ダイアログを表示
        /// </summary>
        private void DoAppendDirectory()
        {
            // 入力ダイアログを表示
            var inputName = DialogCoordinator.ShowModalInputExternal(this,
                                                                     "フォルダ追加",
                                                                     "追加するフォルダ名称を入力してください。");

            if (!string.IsNullOrWhiteSpace(inputName))
            {
                var path = Path.Combine(_outputBasePath, inputName);

                if (!Directory.Exists(path))
                {
                    // 出力フォルダの作成
                    Directory.CreateDirectory(path);

                    // コンボボックスを追加する
                    OutputInfoList.Add(new ComboBoxItem(inputName, inputName));

                    // ソート
                    var tmp = OutputInfoList.OrderBy(e => e).ToList();

                    // Othersを先頭へ
                    ComboBoxItem others = tmp.Where(e => e.DisplayMemberValue == Constants.DirectoryNameOthers).FirstOrDefault();
                    if (others != null)
                    {
                        if (tmp.Remove(others))
                        {
                            tmp.Insert(0, others);
                        }
                    }

                    // 入れ替え
                    OutputInfoList.Clear();
                    OutputInfoList.AddRange(tmp);
                }

                var item = OutputInfoList.Where(x => x.GetSelectedValue<string>().ToLower() == inputName.ToLower())
                                         .First();
                SelectedOutputInfo.Value = item.GetSelectedValue<string>();
            }
        }

        /// <summary>
        /// ダイアログを閉じても良いかの判断
        /// </summary>
        /// <returns>true:閉じて良い、false:閉じれない</returns>
        public bool CanCloseDialog()
        {
            bool result = true;

            foreach (ImageData data in Faces)
            {
                if (string.IsNullOrEmpty(data.FolderName))
                {
                    var confirm = DialogCoordinator.ShowModalMessageExternal(this, "確認", "未確認のデータが有ります。データを破棄されますがよろしいですか？",
                                                                             MessageDialogStyle.AffirmativeAndNegative);
                    if (confirm != MessageDialogResult.Affirmative)
                    {
                        result = false;
                    }
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// ダイアログが閉じた時の動作
        /// </summary>
        public void OnDialogClosed()
        {
            // ダイアログを閉じる
            Dispose();
        }

        /// <summary>
        /// ダイアログが表示された時の動作
        /// </summary>
        /// <param name="parameters"></param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null)
            {
                // 引数を画面に反映する
                if (parameters.TryGetValue(DialogParams.Title, out string title))
                {
                    Title = title;
                }
                if (parameters.TryGetValue(DialogParams.Top, out double top))
                {
                    Top = top;
                }
                if (parameters.TryGetValue(DialogParams.Left, out double left))
                {
                    Left = left;
                }
                if (parameters.TryGetValue(DialogParams.UnsetOwner, out bool unSetOwner))
                {
                    UnsetOwner = unSetOwner;
                }
                if (parameters.TryGetValue(DialogParams.FolderPath, out string path))
                {
                    _outputBasePath = path;

                    var othersPath = Path.Combine(_outputBasePath, Constants.DirectoryNameOthers);
                    if (!Directory.Exists(othersPath))
                    {
                        // その他用フォルダが存在しない場合は作る
                        Directory.CreateDirectory(othersPath);
                    }
                }
            }

            if (Directory.Exists(_outputBasePath))
            {
                // Otherは必ず1個目
                OutputInfoList.Add(new ComboBoxItem(Constants.DirectoryNameOthers, Constants.DirectoryNameOthers));

                // コンボボックスデータを作成
                foreach (var path in Directory.GetDirectories(_outputBasePath, "*", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(path);
                    if (name != Constants.DirectoryNameOthers)
                    {
                        // Others以外を順番に
                        OutputInfoList.AddOnScheduler(new ComboBoxItem(name, name));
                    }
                }
            }
        }

        /// <summary>
        /// 画面破棄の親名称を取得します
        /// </summary>
        /// <returns>nameofの親クラス名称</returns>
        protected override string ParentCloseDialogControl()
        {
            return $"{nameof(DistributionDialogViewModel)}";
        }
    }
}
