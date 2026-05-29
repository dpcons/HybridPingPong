using Microsoft.AspNetCore.Http;

namespace HybridPingPong.Web.Localization;

public class LanguageService
{
    public const string CookieName = "lang";
    public const string DefaultLanguage = "it";

    private string _language = DefaultLanguage;

    public LanguageService(IHttpContextAccessor httpContextAccessor)
    {
        var cookie = httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
        if (!string.IsNullOrWhiteSpace(cookie) && Translations.IsSupported(cookie))
        {
            _language = cookie;
        }
    }

    public event Action? LanguageChanged;

    public string Language
    {
        get => _language;
        set
        {
            if (!Translations.IsSupported(value) || _language == value) return;
            _language = value;
            LanguageChanged?.Invoke();
        }
    }

    public string T(string key) => Translations.Get(_language, key);

    public string this[string key] => T(key);
}
