using CommunityFinder.Services;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Views;

public partial class LanguagePage : ContentPage
{
    public LanguagePage()
    {
        InitializeComponent();
        SetTexts();
    }

    private void SetTexts()
    {
        // 页面标题
        this.Title = LangManager.Get("LanguagePageTitle");

        // 按钮文本
        EnglishButton.Text = LangManager.Get("English");
        ChineseButton.Text = LangManager.Get("Chinese");
        MalayButton.Text = LangManager.Get("Malay");

        // 当前语言显示
        UpdateCurrentLanguageLabel();
    }

    private void UpdateCurrentLanguageLabel()
    {
        string currentLang = LangManager.CurrentLang;
        switch (currentLang)
        {
            case "en":
                CurrentLangLabel.Text = LangManager.Get("CurrentLanguageEnglish");
                break;
            case "zh":
                CurrentLangLabel.Text = LangManager.Get("CurrentLanguageChinese");
                break;
            case "ms":
                CurrentLangLabel.Text = LangManager.Get("CurrentLanguageMalay");
                break;
            default:
                CurrentLangLabel.Text = LangManager.Get("CurrentLanguageUnknown");
                break;
        }
    }

    private void OnEnglishClicked(object sender, EventArgs e)
    {
        LangManager.SetLanguage("en");
        SetTexts();
    }

    private void OnChineseClicked(object sender, EventArgs e)
    {
        LangManager.SetLanguage("zh");
        SetTexts();
    }

    private void OnMalayClicked(object sender, EventArgs e)
    {
        LangManager.SetLanguage("ms");
        SetTexts();
    }
}
