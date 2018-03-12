using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class PagingScrollViewController : 
	ViewController, IBeginDragHandler, IEndDragHandler
{
	[SerializeField] private float animationDuration = 0.3f;
	[SerializeField] private float key1InTangent = 0.0f;
	[SerializeField] private float key1OutTangent = 0.1f;
	[SerializeField] private float key2InTangent = 0.0f;
	[SerializeField] private float key2OutTangent = 0.0f;

#region 添加控制页面控制的处理
	[SerializeField] private PageControl pageControl;	// 相关页面控制
	private ScrollRect cachedScrollRect;
	public ScrollRect CachedScrollRect
	{
		get {
			if(cachedScrollRect == null)
				{ cachedScrollRect = GetComponent<ScrollRect>(); }
			return cachedScrollRect;
		}
	}

	private bool isAnimating = false;		
	private Vector2 destPosition;			
	private Vector2 initialPosition;		
	private AnimationCurve animationCurve;	
	private int prevPageIndex = 0;		

	public void OnBeginDrag(PointerEventData eventData){isAnimating = false;	}

	public void OnEndDrag(PointerEventData eventData)
	{
		GridLayoutGroup grid = CachedScrollRect.content.GetComponent<GridLayoutGroup>();

		CachedScrollRect.StopMovement();

		float pageWidth = -(grid.cellSize.x + grid.spacing.x);

		int pageIndex = 
			Mathf.RoundToInt((CachedScrollRect.content.anchoredPosition.x) / pageWidth);

		if(pageIndex == prevPageIndex && Mathf.Abs(eventData.delta.x) >= 4)
		{
			CachedScrollRect.content.anchoredPosition += 
				new Vector2(eventData.delta.x, 0.0f);
			pageIndex += (int)Mathf.Sign(-eventData.delta.x);
		}

		if(pageIndex < 0)
		{
			pageIndex = 0;
		}
		else if(pageIndex > grid.transform.childCount-1)
		{
			pageIndex = grid.transform.childCount-1;
		}

		prevPageIndex = pageIndex;  

		float destX = pageIndex * pageWidth;
		destPosition = new Vector2(destX, CachedScrollRect.content.anchoredPosition.y);
		initialPosition = CachedScrollRect.content.anchoredPosition;

		Keyframe keyFrame1 = new Keyframe(Time.time, 0.0f, key1InTangent, key1OutTangent);
		Keyframe keyFrame2 = new Keyframe(Time.time + animationDuration, 1.0f, key2InTangent, key2OutTangent);
		animationCurve = new AnimationCurve(keyFrame1, keyFrame2);

		isAnimating = true;

		pageControl.SetCurrentPage(pageIndex);
	}
#endregion 添加控制页面控制的处理

#region 自动滚动动画的实施
	
	void LateUpdate()
	{
		if(isAnimating)
		{
			if(Time.time >= animationCurve.keys[animationCurve.length-1].time)
			{
				CachedScrollRect.content.anchoredPosition = destPosition;
				isAnimating = false;
				return;
			}

			Vector2 newPosition = initialPosition + 
				(destPosition - initialPosition) * animationCurve.Evaluate(Time.time);
			CachedScrollRect.content.anchoredPosition = newPosition;
		}
	}
#endregion

#region 调整位置
	private Rect currentViewRect;

	void Start()
	{
		

		pageControl.SetNumberOfPages(3);	
		pageControl.SetCurrentPage(0);
		UpdateView();
	}

	void Update()
	{
		if(CachedRectTransform.rect.width != currentViewRect.width || 
		   CachedRectTransform.rect.height != currentViewRect.height)
			UpdateView();
	}

	private void UpdateView()
	{
		currentViewRect = CachedRectTransform.rect;

		GridLayoutGroup grid = CachedScrollRect.content.GetComponent<GridLayoutGroup>();
		int paddingH = Mathf.RoundToInt((currentViewRect.width - grid.cellSize.x) / 2.0f);
		int paddingV = Mathf.RoundToInt((currentViewRect.height - grid.cellSize.y) / 2.0f);
		grid.padding = new RectOffset(paddingH, paddingH, paddingV, paddingV);
	}
#endregion
}
