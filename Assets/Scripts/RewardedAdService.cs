using System;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

[DefaultExecutionOrder(-1100)]
public sealed class RewardedAdService : MonoBehaviour
{
    private const string AndroidTestAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    private const string IosTestAdUnitId = "ca-app-pub-3940256099942544/1712485313";

    public static RewardedAdService Instance { get; private set; }

    private RewardedAd rewardedAd;
    private Action rewardCallback;
    private Action completionWithoutReward;
    private bool isLoading;
    private bool isShowing;
    private bool sdkReady;
    private bool rewardEarnedInCurrentAd;
    private bool consentUpdated;

    public bool IsReady
    {
        get { return sdkReady && rewardedAd != null && rewardedAd.CanShowAd() && !isShowing; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject serviceObject = new GameObject("Rewarded Ad Service");
        DontDestroyOnLoad(serviceObject);
        serviceObject.AddComponent<RewardedAdService>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID || UNITY_IOS
        ConsentInformation.Update(new ConsentRequestParameters(), OnConsentInfoUpdated);
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        DestroyRewardedAd();
    }

    public bool Show(Action onRewardEarned, Action onUnavailable, Action onClosedWithoutReward)
    {
        if (!IsReady)
        {
            onUnavailable?.Invoke();
            return false;
        }

        isShowing = true;
        rewardCallback = onRewardEarned;
        completionWithoutReward = onClosedWithoutReward;
        rewardEarnedInCurrentAd = false;
        rewardedAd.Show(reward =>
        {
            if (reward.Amount > 0)
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    rewardEarnedInCurrentAd = true;
                    Action callback = rewardCallback;
                    rewardCallback = null;
                    completionWithoutReward = null;
                    callback?.Invoke();
                });
            }
        });
        return true;
    }

    public bool PrivacyOptionsRequired
    {
        get { return consentUpdated && ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required; }
    }

    public void ShowPrivacyOptions(Action onClosed)
    {
        if (!PrivacyOptionsRequired)
        {
            onClosed?.Invoke();
            return;
        }

        ConsentForm.ShowPrivacyOptionsForm(error =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (error != null)
                {
                    Debug.LogError("Privacy options form could not be shown: " + error.Message);
                }
                onClosed?.Invoke();
            });
        });
    }

    private void OnConsentInfoUpdated(FormError error)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            consentUpdated = true;
            if (error != null)
            {
                Debug.LogWarning("Consent information could not be updated: " + error.Message);
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    if (formError != null)
                    {
                        Debug.LogError("Consent form could not be loaded or shown: " + formError.Message);
                    }

                    if (ConsentInformation.CanRequestAds())
                    {
                        MobileAds.Initialize(OnSdkInitialized);
                    }
                });
            });
        });
    }

    private void OnSdkInitialized(InitializationStatus status)
    {
        if (status == null)
        {
            Debug.LogError("Google Mobile Ads SDK initialization failed.");
            return;
        }

        sdkReady = true;
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        if (isLoading || rewardedAd != null)
        {
            return;
        }

#if UNITY_ANDROID
        string adUnitId = AndroidTestAdUnitId;
#elif UNITY_IOS
        string adUnitId = IosTestAdUnitId;
#else
        return;
#endif

        isLoading = true;
        RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            isLoading = false;
            if (error != null)
            {
                Debug.LogWarning("Rewarded ad could not be loaded: " + error.GetMessage());
                return;
            }

            if (ad == null)
            {
                Debug.LogError("Rewarded ad load completed without an ad or an error.");
                return;
            }

            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentClosed += HandleAdClosed;
            rewardedAd.OnAdFullScreenContentFailed += HandleAdFailedToShow;
        });
    }

    private void HandleAdClosed()
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            isShowing = false;
            if (!rewardEarnedInCurrentAd)
            {
                rewardCallback = null;
                Action callback = completionWithoutReward;
                completionWithoutReward = null;
                callback?.Invoke();
            }
            DestroyRewardedAd();
            LoadRewardedAd();
        });
    }

    private void HandleAdFailedToShow(AdError error)
    {
        Debug.LogWarning("Rewarded ad failed to show: " + error.GetMessage());
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            isShowing = false;
            rewardCallback = null;
            Action callback = completionWithoutReward;
            completionWithoutReward = null;
            callback?.Invoke();
            DestroyRewardedAd();
            LoadRewardedAd();
        });
    }

    private void DestroyRewardedAd()
    {
        if (rewardedAd == null)
        {
            return;
        }

        rewardedAd.Destroy();
        rewardedAd = null;
    }
}
