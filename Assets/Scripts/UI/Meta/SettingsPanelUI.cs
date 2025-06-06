using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChaosCosmos.Core.Services;
using ChaosCosmos.Services.IAP;
using ChaosCosmos.Services.Consent;
using ChaosCosmos.Core.Constants;
using UnityEngine.Localization; // Per Locale
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Linq;
using System; // Per Action

namespace ChaosCosmos.UI.Meta
{
    public class SettingsPanelUI : MonoBehaviour
    {
        public MainMenuManager mainMenuManager;

        [Header("Settings UI Elements")]
        public Slider musicVolumeSlider;
        public TextMeshProUGUI musicVolumeValueText;
        public Slider sfxVolumeSlider;
        public TextMeshProUGUI sfxVolumeValueText;

        public TMP_Dropdown languageDropdown;

        public Button restorePurchasesButton;
        public Button privacyPolicyButton;
        public Button privacyPreferencesButton;
        // public Button resetProgressDebugButton; // Opzionale, per debug

        private IIAPService _iapService;
        private IConsentService _consentService;
        // private ISoundService _soundService; // Ipotetico

        private List<Locale> _availableLocales;
        private bool _isPopulatingDropdown = false; // Flag per evitare chiamate ricorsive

        void Start()
        {
            if (mainMenuManager == null)
            {
                mainMenuManager = FindObjectOfType<MainMenuManager>();
                if (mainMenuManager == null)
                {
                    Debug.LogError("SettingsPanelUI: MainMenuManager non trovato e non assegnato!");
                }
            }

            if (!TryGetService(out _iapService)) Debug.LogWarning("SettingsPanelUI: IIAPService non trovato. Ripristino acquisti potrebbe non funzionare.");
            if (!TryGetService(out _consentService)) Debug.LogWarning("SettingsPanelUI: IConsentService non trovato. Preferenze privacy potrebbero non funzionare.");
            // if (!TryGetService(out _soundService)) Debug.LogWarning("SettingsPanelUI: ISoundService non trovato. Controlli volume potrebbero non avere effetto audio immediato.");


            SetupUIListeners();
            // LoadSettingsAndPopulateUI() è chiamato da OnEnable
        }

        private bool TryGetService<T>(out T service) where T : class, IService
        {
            if (ServiceLocator.IsRegistered<T>())
            {
                service = ServiceLocator.Get<T>();
                return service != null;
            }
            service = null;
            Debug.LogWarning($"SettingsPanelUI: Servizio {typeof(T).Name} non registrato.");
            return false;
        }

        void OnEnable()
        {
            // Assicurati che LocalizationSettings sia pronto prima di popolare il dropdown
            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                LoadSettingsAndPopulateUI();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += HandleLocalizationInitialized;
            }
            // Registra listener per cambio lingua esterna per aggiornare il dropdown
            LocalizationSettings.SelectedLocaleChanged += OnExternalLocaleChanged;
        }

        void OnDisable()
        {
            LocalizationSettings.InitializationOperation.Completed -= HandleLocalizationInitialized;
            LocalizationSettings.SelectedLocaleChanged -= OnExternalLocaleChanged;
        }

        private void HandleLocalizationInitialized(AsyncOperationHandle<LocalizationSettings> op)
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                LoadSettingsAndPopulateUI();
            }
            else
            {
                Debug.LogError("SettingsPanelUI: Fallimento inizializzazione LocalizationSettings.");
            }
        }

        private void OnExternalLocaleChanged(Locale newLocale)
        {
            // Se la lingua viene cambiata da un altro sistema, aggiorna il dropdown
            if (languageDropdown != null && LocalizationSettings.InitializationOperation.IsDone)
            {
                int newLocaleIndex = _availableLocales.IndexOf(newLocale);
                if (newLocaleIndex != -1)
                {
                    _isPopulatingDropdown = true; // Evita che OnLanguageDropdownChanged venga triggerato
                    languageDropdown.SetValueWithoutNotify(newLocaleIndex);
                    _isPopulatingDropdown = false;
                }
            }
        }


        private void SetupUIListeners()
        {
            musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(OnSfxVolumeChanged);
            languageDropdown?.onValueChanged.AddListener(OnLanguageDropdownChanged);

            restorePurchasesButton?.onClick.AddListener(OnRestorePurchasesClicked);
            privacyPolicyButton?.onClick.AddListener(OnPrivacyPolicyClicked);
            privacyPreferencesButton?.onClick.AddListener(OnPrivacyPreferencesClicked);
        }

        private void LoadSettingsAndPopulateUI()
        {
            if (!gameObject.activeInHierarchy) return; // Non fare nulla se il pannello non è attivo

            // Volume
            float musicVol = PlayerPrefs.GetFloat(PlayerPrefsKeys.SETTINGS_MUSIC_VOLUME, 0.75f);
            if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(musicVol);
            if (musicVolumeValueText != null) musicVolumeValueText.text = $"{Mathf.RoundToInt(musicVol * 100)}%";
            // _soundService?.SetMusicVolume(musicVol); // Applica subito il volume caricato

            float sfxVol = PlayerPrefs.GetFloat(PlayerPrefsKeys.SETTINGS_SFX_VOLUME, 0.75f);
            if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
            if (sfxVolumeValueText != null) sfxVolumeValueText.text = $"{Mathf.RoundToInt(sfxVol * 100)}%";
            // _soundService?.SetSfxVolume(sfxVol); // Applica subito il volume caricato

            // Lingua
            PopulateLanguageDropdown();

            // Stato pulsante Restore Purchases
            if (restorePurchasesButton != null)
            {
                restorePurchasesButton.interactable = (_iapService != null && _iapService.IsInitialized);
            }
             if (privacyPreferencesButton != null)
            {
                privacyPreferencesButton.interactable = (_consentService != null && _consentService.IsInitialized);
            }
        }

        private void PopulateLanguageDropdown()
        {
            if (languageDropdown == null) return;
            if (!LocalizationSettings.InitializationOperation.IsDone || LocalizationSettings.AvailableLocales == null)
            {
                 Debug.LogWarning("SettingsPanelUI: Localization non pronta per popolare dropdown lingue.");
                 languageDropdown.ClearOptions();
                 languageDropdown.AddOptions(new List<string> { "Loading..." });
                 languageDropdown.interactable = false;
                 return;
            }

            _isPopulatingDropdown = true;
            languageDropdown.interactable = true;
            languageDropdown.ClearOptions();

            _availableLocales = LocalizationSettings.AvailableLocales.Locales; // Cache per l'indice
            List<string> localeNames = new List<string>();
            int currentSelectedLocaleIndex = -1;

            for (int i = 0; i < _availableLocales.Count; i++)
            {
                localeNames.Add(_availableLocales[i].LocaleName);
                if (_availableLocales[i] == LocalizationSettings.SelectedLocale)
                {
                    currentSelectedLocaleIndex = i;
                }
            }
            languageDropdown.AddOptions(localeNames);

            if (currentSelectedLocaleIndex != -1)
            {
                languageDropdown.SetValueWithoutNotify(currentSelectedLocaleIndex);
            }
            _isPopulatingDropdown = false;
        }

        public void OnMusicVolumeChanged(float value)
        {
            // TODO_SFX: UI_Slider_Scrub (mentre cambia, opzionale e non troppo frequente)
            PlayerPrefs.SetFloat(PlayerPrefsKeys.SETTINGS_MUSIC_VOLUME, value);
            if (musicVolumeValueText != null) musicVolumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            // _soundService?.SetMusicVolume(value);
            Debug.Log($"Settings: Volume Musica impostato a {value}");
        }

        public void OnSfxVolumeChanged(float value)
        {
            // TODO_SFX: UI_Slider_Scrub (e magari un SFX di test al rilascio)
            PlayerPrefs.SetFloat(PlayerPrefsKeys.SETTINGS_SFX_VOLUME, value);
            if (sfxVolumeValueText != null) sfxVolumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            // _soundService?.SetSfxVolume(value);
            Debug.Log($"Settings: Volume SFX impostato a {value}");
        }

        public void OnLanguageDropdownChanged(int index)
        {
            // TODO_SFX: UI_Dropdown_Select
            if (_isPopulatingDropdown || _availableLocales == null || index < 0 || index >= _availableLocales.Count)
            {
                return;
            }

            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                Locale newLocale = _availableLocales[index];
                LocalizationSettings.SelectedLocale = newLocale;
                Debug.Log($"Settings: Lingua cambiata a: {newLocale.LocaleName}");
            }
        }

        public void OnRestorePurchasesClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            Debug.Log("Settings: Pulsante 'Ripristina Acquisti' cliccato.");
            if (_iapService != null && _iapService.IsInitialized)
            {
                _iapService.RestorePurchases((success, message) => {
                    if (success) Debug.Log($"Settings: Ripristino acquisti completato. Messaggio: {message}");
                    else Debug.LogError($"Settings: Fallimento ripristino acquisti. Messaggio: {message}");
                });
            } else Debug.LogError("Settings: IAPService non disponibile per ripristino acquisti.");
        }

        public void OnPrivacyPolicyClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            string privacyPolicyURL = "http://www.example.com/chaoscosmos/privacy";
            Debug.Log($"Settings: Pulsante 'Privacy Policy' cliccato. Apro URL: {privacyPolicyURL}");
            Application.OpenURL(privacyPolicyURL);
        }

        public void OnPrivacyPreferencesClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard
            Debug.Log("Settings: Pulsante 'Preferenze Privacy' cliccato.");
            if (_consentService != null && _consentService.IsInitialized)
            {
                _consentService.ResetConsentStatus();
                Debug.Log("Settings: Stato consenso resettato. I popup verranno mostrati al prossimo avvio (simulato).");
            } else Debug.LogError("Settings: IConsentService non disponibile.");
        }

        public void OnBackButtonClicked()
        {
            // TODO_SFX: UI_Button_Click_Standard (o un suono di "back/cancel")
            PlayerPrefs.Save();
            Debug.Log("Settings: Impostazioni salvate in PlayerPrefs.");

            if (mainMenuManager != null)
            {
                mainMenuManager.ShowMainButtonsPanel();
            }
            else
            {
                Debug.LogWarning("SettingsPanelUI: MainMenuManager non referenziato, disattivo solo il pannello.");
                gameObject.SetActive(false);
            }
        }
    }
}
