using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MSCLoader.LoadingScreen
{
    public class LoadingScreenParent : MonoBehaviour
    {
        [HideInInspector]
        public List<LoadingScreenItem> loadingScreenItems = new List<LoadingScreenItem>();

        public void SetProgress(float progress)
        {
            if (loadingScreenItems.Count == 0) Init();

            for (int i = 0; i < loadingScreenItems.Count; i++) loadingScreenItems[i].OnProgress(progress);
        }

        void Init()
        {
            loadingScreenItems.AddRange(this.gameObject.GetComponentsInChildren<LoadingScreenItem>(true));
        }

        private void OnLevelWasLoaded(int level)
        {
            DontDestroyOnLoad(this.gameObject);
            gameObject.SetActive(false);
        }
    }

    public partial class LoadingScreenItem : MonoBehaviour
    {
        void OnDisable() { OnReset(); }
        public virtual void OnProgress(float progress) { }
        public virtual void OnReset() { }
    }

    public class LoadingBar : LoadingScreenItem
    {
        public Image loadingBar;

        public override void OnProgress(float progress)
        {
            loadingBar.fillAmount = progress;
        }

        public override void OnReset()
        {
            loadingBar.fillAmount = 0f;
        }
    }

    public class LoadingSpinner : LoadingScreenItem
    {
        public Vector3 rotAxis = new Vector3(1f, 0f, 0f);
        public float rotateAmount = 1200f;
        public RectTransform spinnyObject;

        public override void OnProgress(float progress)
        {
            spinnyObject.localEulerAngles = rotAxis * (progress * rotateAmount);
        }

        public override void OnReset()
        {
            spinnyObject.localEulerAngles = rotAxis * 0f;
        }
    }

    public class LoadingText : LoadingScreenItem
    {
        public Text[] textObjects;
        public string baseText = "Now Loading Year 1995...";
        public bool addPercentageUnderText = false;

        public override void OnProgress(float progress)
        {
            for (int i = 0; i < textObjects.Length; i++) textObjects[i].text = $"{baseText} {(addPercentageUnderText ? "\n" : string.Empty)} {Mathf.FloorToInt(progress * 100)}%";
        }
        public override void OnReset()
        {
            for (int i = 0; i < textObjects.Length; i++) textObjects[i].text = $"{baseText} {(addPercentageUnderText ? "\n" : string.Empty)} {0}%";
        }
    }
}
