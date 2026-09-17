using JaguarCipher.Resources.Pages;
using CommunityToolkit.Maui.Storage;
using System.Text;

namespace JaguarCipher;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

public partial class MainPage : ContentPage
{
    private bool _passwordVisible = false;
    private bool _passwordEnable = true;
    private string? _selectedFilePath;
    private string? _processedFileText;
    private string? _processedFileName;
    private HelpPage? _helpPage;
    private int _currentIndexOperationLogic;
    private IList<string> _encryptionMethodPickerItems;

    public MainPage()
    {
        InitializeComponent();
        _encryptionMethodPickerItems = EncryptionMethodPicker.Items;
#if WINDOWS
        Loaded += MainPage_Loaded;
#endif
    }

#if WINDOWS
    private void MainPage_Loaded(object? sender, EventArgs e)
    {
        if (Handler?.PlatformView is FrameworkElement view)
        {
            view.AddHandler(
                UIElement.KeyDownEvent,
                new KeyEventHandler(MainPage_KeyDown),
                true);
        }
    }

    private async void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.F1)
        {
            e.Handled = true;

            await ShowHelpInfo();
        }
    }
#endif

    #region ====РЕЖИМ ШИФРОВАНИЯ / РАСШИФРОВАНИЯ // АЛГОРИТМ======

    private void OperationMode_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == false) return;

        UpdateOperationMode();
    }

    private void UpdateOperationMode()
    {
        InputFileName.Text = $"Здесь будет имя выбранного файла";
        _selectedFilePath = string.Empty;

        if (EncryptRadio.IsChecked == true)
        {
            ProcessButton.Text = "ЗАШИФРОВАТЬ";
            StatusLabel.Text = "Готово к шифрованию";
        }
        else
        {
            ProcessButton.Text = "РАСШИФРОВАТЬ";
            StatusLabel.Text = "Готово к расшифрованию";
        }
    }

    private void OperationLogic_CheckedChanged(object sender, EventArgs e)
    {
        if (EncryptionMethodPicker == null) return;

        int selectIndex = EncryptionMethodPicker.SelectedIndex;
        _currentIndexOperationLogic = selectIndex;

        switch (EncryptionMethodPicker.SelectedIndex)
        {
            case 0: // Цезарь
                ModeLabel.Text = "Метод: моноалфавитная замена";
                KeyLabel.Text = "Ключ: числовой сдвиг";
                AlphabetLabel.Text = "Алфавит: 33 русские буквы";
                SecurityLabel.Text = "Особенность: фиксированный сдвиг";
                _passwordEnable = true;
                PasswordEntry.Keyboard = Keyboard.Numeric;
                break;

            case 1: // Виженер
                ModeLabel.Text = "Метод: полиалфавитная замена";
                KeyLabel.Text = "Ключ: ключевое слово";
                AlphabetLabel.Text = "Алфавит: 33 русские буквы";
                SecurityLabel.Text = "Особенность: сдвиг зависит от ключа";
                _passwordEnable = true;
                PasswordEntry.Keyboard = Keyboard.Text;
                break;

            case 2: // Атбаш
                ModeLabel.Text = "Метод: моноалфавитная замена";
                KeyLabel.Text = "Ключ: не требуется";
                AlphabetLabel.Text = "Алфавит: 33 русские буквы";
                SecurityLabel.Text = "Особенность: обратный алфавит";
                _passwordEnable = false;
                break;

            case 3: // Бофор
                ModeLabel.Text = "Метод: полиалфавитная замена";
                KeyLabel.Text = "Ключ: ключевое слово";
                AlphabetLabel.Text = "Алфавит: 33 русские буквы";
                SecurityLabel.Text = "Особенность: обратный сдвиг";
                _passwordEnable = true;
                PasswordEntry.Keyboard = Keyboard.Text;
                break;

            case 4: // Тритемий
                ModeLabel.Text = "Метод: прогрессивный сдвиг";
                KeyLabel.Text = "Ключ: начальный сдвиг";
                AlphabetLabel.Text = "Алфавит: 33 русские буквы";
                SecurityLabel.Text = "Особенность: сдвиг меняется";
                _passwordEnable = true;
                PasswordEntry.Keyboard = Keyboard.Numeric;
                break;

            case 5: // Вернам
                ModeLabel.Text = "Метод: гаммирование";
                KeyLabel.Text = "Ключ: ключевая гамма";
                AlphabetLabel.Text = "Алфавит: 33 русские буквы";
                SecurityLabel.Text = "Особенность: операция по модулю";
                _passwordEnable = true;
                PasswordEntry.Keyboard = Keyboard.Text;
                break;
        }
        PasswordLayout.IsVisible = _passwordEnable;
    }

    private void PasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (PasswordEntry.Keyboard != Keyboard.Numeric) return;

        if (sender is Entry entry && e.NewTextValue != null)
        {
            var filtered = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

            if (entry.Text != filtered)
            {
                entry.Text = filtered;
            }
        }
    }

    #endregion

    #region ====ТЕКСТ / ФАЙЛ======

    private void DataMode_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == false) return;

        UpdateDataMode();
    }

    private void UpdateDataMode()
    {
        bool isTextMode = TextRadio.IsChecked;

        InputEditor.IsVisible = isTextMode;
        InputFileName.IsVisible = !isTextMode;
        SelectFileButton.IsVisible = !isTextMode;
        InputButtonPaste.IsVisible = isTextMode;
        InputButtonCopy.IsVisible = isTextMode;
        InputButtonReset.IsVisible = isTextMode;

        if (isTextMode == true)
        {
            StatusLabel.Text = EncryptRadio.IsChecked
                ? "Готово к шифрованию текста"
                : "Готово к расшифрованию текста";
        }
        else
        {
            StatusLabel.Text = EncryptRadio.IsChecked
                ? "Выберите файл для шифрования"
                : "Выберите файл для расшифрования";
        }
    }

    #endregion

    #region ====ПАРОЛЬ======

    private void PasswordVisibilityButton_Clicked(object sender, EventArgs e)
    {
        _passwordVisible = !_passwordVisible;

        PasswordEntry.IsPassword = !_passwordVisible;

        PasswordVisibilityButton.Text =
            _passwordVisible ? "◉" : "○";
    }

    #endregion

    #region ====ВСТАВКА/КОПИРОВАНИЕ/ОЧИСТКА ИСХОДНЫХ ДАННЫХ======

    private async void PasteInputButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            string? clipboardData = await Clipboard.Default.GetTextAsync();

            if (string.IsNullOrEmpty(clipboardData) == false)
            {
                InputEditor.Text = clipboardData;

                StatusLabel.Text = "Текст вставлен из буфера обмена";
            }
            else
            {
                StatusLabel.Text = "Буфер обмена пуст";
            }
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось вставить текст: {ex.Message}");
        }
    }

    private async void CopyInputButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(InputEditor.Text) == true)
            {
                StatusLabel.Text = "Нет данных для копирования";
                return;
            }

            await Clipboard.Default.SetTextAsync(
                InputEditor.Text);

            StatusLabel.Text = "Исходные данные скопированы";
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось скопировать текст: {ex.Message}");
        }
    }

    private void ClearInputButton_Clicked(object sender, EventArgs e)
    {
        string value = InputEditor.Text;
        if (string.IsNullOrEmpty(value) == true) return;

        InputEditor.Text = string.Empty;

        _selectedFilePath = null;

        StatusLabel.Text = "Исходные данные очищены";
    }

    #endregion

    #region ====КОПИРОВАНРИЕ/ОЧИСТКА РЕЗУЛЬАТАТА======

    private async void CopyResultButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(ResultEditor.Text) == true)
            {
                StatusLabel.Text = "Нет результата для копирования";
                return;
            }

            await Clipboard.Default.SetTextAsync(
                ResultEditor.Text);

            StatusLabel.Text = "Результат скопирован";
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось скопировать результат: {ex.Message}");
        }
    }

    private void ClearResultButton_Clicked(object sender, EventArgs e)
    {
        string value = ResultEditor.Text;
        if (string.IsNullOrEmpty(value) == true) return;

        ResultEditor.Text = string.Empty;
        _selectedFilePath = string.Empty;

        StatusLabel.Text = "Результат очищен";
    }

    private async void SaveFile_Clicked(
    object sender,
    EventArgs e)
    {
        await SaveProcessedFileAsync();
    }

    private async Task SaveProcessedFileAsync()
    {
        if (string.IsNullOrEmpty(_processedFileText) == true)
        {
            ShowError("Сначала выполните шифрование или расшифрование файла.");

            return;
        }

        try
        {
            string fileName =
                _processedFileName ?? "result.txt";

            using MemoryStream stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        _processedFileText));

            FileSaverResult result =
                await FileSaver.Default.SaveAsync(
                    fileName,
                    stream);

            if (result.IsSuccessful)
            {
                await DisplayAlert(
                    "Готово",
                    $"Файл сохранён:\n{result.FilePath}",
                    "ОК");

                StatusLabel.Text =
                    "Файл успешно сохранён";
            }
            else
            {
                await DisplayAlert(
                    "Ошибка",
                    "Не удалось сохранить файл.",
                    "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Ошибка сохранения",
                ex.Message,
                "ОК");
        }
    }

    #endregion

    #region ====ВЫБОР ФАЙЛА======

    private async void SelectFileButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = EncryptRadio.IsChecked == true
                    ? "Выберите файл для шифрования"
                    : "Выберите файл для расшифрования",

                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        [DevicePlatform.WinUI] = new[]
                        {
                        ".txt",
                        ".csv",
                        ".log",
                        ".json",
                        ".xml"
                        }
                    })
            });

            if (result == null)
            {
                StatusLabel.Text = "Выбор файла отменён";
                return;
            }

            _selectedFilePath = result.FullPath;

            StatusLabel.Text =
                $"Выбран файл: {result.FileName}";

            InputFileName.Text =
                $"Выбран файл: {result.FileName}";
        }
        catch (Exception ex)
        {
            ShowError(
                $"Не удалось выбрать файл: {ex.Message}");
        }
    }

    #endregion

    #region ====ОБРАБОТКА ТЕКСТА/ФАЙЛА======
    private async void ProcessButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PasswordEntry.Text) == true && _passwordEnable)
            {
                ShowError("Введите пароль.");

                PasswordEntry.Focus();
                return;
            }
            else if (EncryptionMethodPicker.SelectedItem == null)
            {
                ShowError("Выберите алгоритм шифрования.");

                await SettingsScrollView.ScrollToAsync(EncryptionMethodPicker, ScrollToPosition.Center, true);
                EncryptionMethodPicker.SelectedIndex = 0;
                return;
            }

            if (TextRadio.IsChecked == true)
            {
                await ProcessTextAsync();
            }
            else
            {
                await ProcessFileAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError(
                $"Произошла ошибка: {ex.Message}");
        }
    }

    private async Task ProcessTextAsync()
    {
        string inputText = InputEditor.Text;
        string inputPassword = PasswordEntry.Text;
        if (string.IsNullOrEmpty(inputText) == true)
        {
            await DisplayAlert(
                "Ошибка",
                "Введите исходный текст.",
                "ОК");

            InputEditor.Focus();
            return;
        }

        SetBusyState(true);

        try
        {
            StatusLabel.Text =
                EncryptRadio.IsChecked == true
                    ? "Выполняется шифрование..."
                    : "Выполняется расшифрование...";

            await Task.Delay(300);

            string result = "";
            int indexSelectMode = EncryptionMethodPicker.SelectedIndex;

            bool encrypt = EncryptRadio.IsChecked == true;

            switch (indexSelectMode)
            {
                case 0: // Цезарь
                    if (int.TryParse(inputPassword, out int shift) == false)
                    {
                        await DisplayAlert(
                        "Ошибка",
                        "Для шифра Цезаря пароль должен быть числом.",
                        "ОК");

                        PasswordEntry.Focus();
                        return;
                    }

                    result = encrypt
                        ? RussianCipher.CaesarEncrypt(inputText, shift)
                        : RussianCipher.CaesarDecrypt(inputText, shift);

                    break;


                case 1: // Виженер
                    result = encrypt
                        ? RussianCipher.VigenereEncrypt(inputText, inputPassword)
                        : RussianCipher.VigenereDecrypt(inputText, inputPassword);

                    break;


                case 2: // Атбаш
                    result = RussianCipher.AtbashEncrypt(inputText);

                    break;

                case 3:
                    result = encrypt
                        ? RussianCipher.BeaufortEncrypt(inputText, inputPassword)
                        : RussianCipher.BeaufortDecrypt(inputText, inputPassword);
                    break;

                case 4:
                    if (!int.TryParse(inputPassword, out int shiftt))
                        throw new Exception("Для Тритемия нужен числовой ключ.");

                    result = encrypt
                        ? RussianCipher.TrithemiusEncrypt(inputText, shiftt)
                        : RussianCipher.TrithemiusDecrypt(inputText, shiftt);
                    break;

                case 5:
                    result = encrypt
                        ? RussianCipher.VernamEncrypt(inputText, inputPassword)
                        : RussianCipher.VernamDecrypt(inputText, inputPassword);
                    break;

                default:
                    await DisplayAlert(
                    "Ошибка",
                    "Выберите алгоритм шифрования.",
                    "ОК");

                    await SettingsScrollView.ScrollToAsync(EncryptionMethodPicker, ScrollToPosition.Center, true);
                    EncryptionMethodPicker.SelectedIndex = 0;
                    return;
            }

            ResultEditor.Text = result;

            StatusLabel.Text =
                EncryptRadio.IsChecked == true
                    ? "Шифрование текста завершено"
                    : "Расшифрование текста завершено";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private async Task ProcessFileAsync()
    {
        if (string.IsNullOrEmpty(_selectedFilePath) == true)
        {
            await DisplayAlert(
                "Ошибка",
                "Сначала выберите файл.",
                "ОК");

            return;
        }

        if (File.Exists(_selectedFilePath) == false)
        {
            await DisplayAlert(
                "Ошибка",
                "Выбранный файл не существует.",
                "ОК");

            return;
        }

        SetBusyState(true);

        try
        {
            bool encrypt =
                EncryptRadio.IsChecked == true;

            StatusLabel.Text = encrypt
                ? "Выполняется шифрование файла..."
                : "Выполняется расшифрование файла...";

            await Task.Delay(300);

            // Читаем файл как текст
            string inputText = await File.ReadAllTextAsync(_selectedFilePath);

            if (string.IsNullOrEmpty(inputText) == true)
            {
                await DisplayAlert(
                    "Ошибка",
                    "Файл пустой.",
                    "ОК");

                return;
            }

            string inputPassword =
                PasswordEntry.Text ?? "";

            int indexSelectMode =
                EncryptionMethodPicker.SelectedIndex;

            string result = "";

            switch (indexSelectMode)
            {
                case 0: // Цезарь
                    {
                        if (int.TryParse(
                            inputPassword,
                            out int shiftt) == false)
                        {
                            await DisplayAlert(
                                "Ошибка",
                                "Для шифра Цезаря ключ должен быть числом.",
                                "ОК");

                            PasswordEntry.Focus();
                            return;
                        }

                        result = encrypt
                            ? RussianCipher.CaesarEncrypt(
                                inputText,
                                shiftt)
                            : RussianCipher.CaesarDecrypt(
                                inputText,
                                shiftt);

                        break;
                    }


                case 1: // Виженер
                    {
                        result = encrypt
                            ? RussianCipher.VigenereEncrypt(
                                inputText,
                                inputPassword)
                            : RussianCipher.VigenereDecrypt(
                                inputText,
                                inputPassword);

                        break;
                    }


                case 2: // Атбаш
                    {
                        result = RussianCipher.AtbashEncrypt(
                            inputText);

                        break;
                    }

                case 3: // Бофор
                    {
                        result = encrypt
                            ? RussianCipher.BeaufortEncrypt(
                                inputText,
                                inputPassword)
                            : RussianCipher.BeaufortDecrypt(
                                inputText,
                                inputPassword);

                        break;
                    }


                case 4: // Тритемий
                    {
                        if (!int.TryParse(
                            inputPassword,
                            out int shift))
                        {
                            await DisplayAlert(
                                "Ошибка",
                                "Для Тритемия ключ должен быть числом.",
                                "ОК");

                            PasswordEntry.Focus();
                            return;
                        }

                        result = encrypt
                            ? RussianCipher.TrithemiusEncrypt(
                                inputText,
                                shift)
                            : RussianCipher.TrithemiusDecrypt(
                                inputText,
                                shift);

                        break;
                    }

                case 5: // Вернам
                    {
                        result = encrypt
                            ? RussianCipher.VernamEncrypt(
                                inputText,
                                inputPassword)
                            : RussianCipher.VernamDecrypt(
                                inputText,
                                inputPassword);

                        break;
                    }

                default:
                    {
                        await DisplayAlert(
                            "Ошибка",
                            "Выберите алгоритм шифрования.",
                            "ОК");

                        return;
                    }
            }

            // Сохраняем результат в памяти,
            // чтобы потом использовать кнопкой "Сохранить"
            _processedFileText = result;

            string originalName =
                Path.GetFileNameWithoutExtension(
                    _selectedFilePath);

            _processedFileName = encrypt
                ? originalName + ".encrypted.txt"
                : originalName + ".decrypted.txt";

            // Показываем результат
            ResultEditor.Text = result;

            StatusLabel.Text = encrypt
                ? "Шифрование файла завершено"
                : "Расшифрование файла завершено";
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Ошибка",
                ex.Message,
                "ОК");
        }
        finally
        {
            SetBusyState(false);
        }
    }

    #endregion

    #region ====ОБНОВЛЕНИЕ СОСТОЯНИЯ ИНТЕРФЕЙСА======

    private void SetBusyState(bool isBusy)
    {
        ProcessButton.IsEnabled = !isBusy;
        PasswordEntry.IsEnabled = !isBusy;
        InputEditor.IsEnabled = !isBusy;

        if (isBusy == true)
        {
            ProcessButton.Text = "ВЫПОЛНЕНИЕ...";
        }
        else
        {
            ProcessButton.Text =
                EncryptRadio.IsChecked == true
                    ? "ЗАШИФРОВАТЬ"
                    : "РАСШИФРОВАТЬ";
        }
    }

    #endregion

    #region ====ОШИБКА======

    private async void ShowError(string message)
    {
        StatusLabel.Text = "Ошибка";

        await DisplayAlert(
            "Ошибка",
            message,
            "ОК");
    }

    #endregion

    #region ====СПРАВКА F1======

    private async void HelpButton_Clicked(object sender, EventArgs e)
    {
        await ShowHelpInfo();
    }

    private async Task ShowHelpInfo()
    {
        if (_helpPage != null) return;

        HelpPage helpPage = new HelpPage();
        await Navigation.PushModalAsync(helpPage);

        _helpPage = helpPage;
        _helpPage!.Closed += ZeroHelpInfo;
    }

    private void ZeroHelpInfo(object? sender, EventArgs e)
    {
        _helpPage!.Closed -= ZeroHelpInfo;
        _helpPage = null;
    }

    #endregion
}