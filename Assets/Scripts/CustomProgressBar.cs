using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CustomProgressBar : MonoBehaviour
{

	#region --------------------	Public Properties

	public float percent { get { return _percent; } set { _percent = Mathf.Clamp01(value); } }

	#endregion

	#region --------------------	Private Fields

	private Image _image = null;
	[SerializeField] private Image _fillImage;
	[SerializeField] [Range(0f, 1f)] private float _percent;
	[SerializeField] private bool _flipHorizontal = false;

	#endregion

	#region --------------------	Private Methods

	private void Awake() => _image = GetComponent<Image>();

	private void Update()
	{
		if (GameManager.state != GameManager.GameState.Gameplay) return;
		if (_fillImage == null) return;
		_fillImage.enabled = _image.enabled;
		_fillImage.rectTransform.SetParent(_image.rectTransform);
		_fillImage.rectTransform.anchoredPosition = Vector2.zero + ((_flipHorizontal) ? Vector2.right : Vector2.zero);
		_fillImage.rectTransform.anchorMin = Vector2.zero + ((_flipHorizontal) ? Vector2.right : Vector2.zero);
		_fillImage.rectTransform.anchorMax = Vector2.up + ((_flipHorizontal) ? Vector2.right : Vector2.zero);
		_fillImage.rectTransform.pivot = Vector2.up * 0.5f + ((_flipHorizontal) ? Vector2.right : Vector2.zero);
		_fillImage.rectTransform.sizeDelta = new Vector2(_percent * _image.rectTransform.sizeDelta.x, 0f);
	}

	#endregion

}