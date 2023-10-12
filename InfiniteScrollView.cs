/// <summary>
/// 无限滑动列表
/// 使用说明
///     ――此功能脚本是对ScrollRect的扩展，所以必须添加UGUI提供的基础Scroll View
///     ――Content上必须添加GridLayoutGroup组件，通过GridLayoutGroup组件设计布局，(我在代码中对startCorner、startAxis、childAlignment和constraintCount进行了限制，不需要对其设置)
///     ――不能添加Content Size Fitter组件
///     ――先调用SetTotalCount方法设置总的数据数量再调用Init方法进行初始化
///     ――根据需求修改SetShow方法体
///     ――只适用于单向滑动的情况，不能满足竖直和水平同时滑动的需求，因为大多数无限滑动列表的使用场景都是单向的
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 无限滑动列表
/// </summary>
public class InfiniteScrollView : MonoBehaviour
{
  private ScrollRect scrollRect;//滑动框组件
  private RectTransform content;//滑动框的Content
  private GridLayoutGroup layout;//布局组件

  [Header("滑动类型")]
  public ScrollType scrollType;
  [Header("固定的Item数量")]
  public int fixedCount;
  [Header("Item的预制体")]
  public GameObject itemPrefab;

  private int totalCount;//总的数据数量
  private List<RectTransform> dataList = new List<RectTransform>();//数据实体列表
  private int headIndex;//头下标
  private int tailIndex;//尾下标
  private Vector2 firstItemAnchoredPos;//第一个Item的锚点坐标

  #region Init

  /// <summary>
  /// 实例化Item
  /// </summary>
  private void InitItem()
  {
      for (int i = 0; i < fixedCount; i++)
      {
          GameObject tempItem = Instantiate(itemPrefab, content);
          dataList.Add(tempItem.GetComponent<RectTransform>());
          SetShow(tempItem.GetComponent<RectTransform>(), i);
      }
  }

  /// <summary>
  /// 设置Content大小
  /// </summary>
  private void SetContentSize()
  {
      content.sizeDelta = new Vector2
          (
              layout.padding.left + layout.padding.right + totalCount * (layout.cellSize.x + layout.spacing.x) - layout.spacing.x - content.rect.width,
              layout.padding.top + layout.padding.bottom + totalCount * (layout.cellSize.y + layout.spacing.y) - layout.spacing.y
          );
  }

  /// <summary>
  /// 设置布局
  /// </summary>
  private void SetLayout()
  {
      layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
      layout.startAxis = GridLayoutGroup.Axis.Horizontal;
      layout.childAlignment = TextAnchor.UpperLeft;
      layout.constraintCount = 1;
      if (scrollType == ScrollType.Horizontal)
      {
          scrollRect.horizontal = true;
          scrollRect.vertical = false;
          layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
      }
      else if (scrollType == ScrollType.Vertical)
      {
          scrollRect.horizontal = false;
          scrollRect.vertical = true;
          layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
      }
  }

  /// <summary>
  /// 得到第一个数据的锚点位置
  /// </summary>
  private void GetFirstItemAnchoredPos()
  {
      firstItemAnchoredPos = new Vector2
          (
              layout.padding.left + layout.cellSize.x / 2,
              -layout.padding.top - layout.cellSize.y / 2
          );
  }

  #endregion

  #region Main

  /// <summary>
  /// 滑动中
  /// </summary>
  private void OnScroll(Vector2 v)
  {
      if (dataList.Count == 0)
      {
          Debug.LogWarning("先调用SetTotalCount方法设置数据总数量再调用Init方法进行初始化");
          return;
      }

      if (scrollType == ScrollType.Vertical)
      {
          //向上滑
          while (content.anchoredPosition.y >= layout.padding.top + (headIndex + 1) * (layout.cellSize.y + layout.spacing.y)
          && tailIndex != totalCount - 1)
          {
              //将数据列表中的第一个元素移动到最后一个
              RectTransform item = dataList[0];
              dataList.Remove(item);
              dataList.Add(item);

              //设置位置
              SetPos(item, tailIndex + 1);
              //设置显示
              SetShow(item, tailIndex + 1);

              headIndex++;
              tailIndex++;
          }
          //向下滑
          while (content.anchoredPosition.y <= layout.padding.top + headIndex * (layout.cellSize.y + layout.spacing.y)
              && headIndex != 0)
          {
              //将数据列表中的最后一个元素移动到第一个
              RectTransform item = dataList.Last();
              dataList.Remove(item);
              dataList.Insert(0, item);

              //设置位置
              SetPos(item, headIndex - 1);
              //设置显示
              SetShow(item, headIndex - 1);

              headIndex--;
              tailIndex--;
          }
      }
      else if (scrollType == ScrollType.Horizontal)
      {
          //向左滑
          while (content.anchoredPosition.x <= -layout.padding.left - (headIndex + 1) * (layout.cellSize.x + layout.spacing.x)
          && tailIndex != totalCount - 1)
          {
              //将数据列表中的第一个元素移动到最后一个
              RectTransform item = dataList[0];
              dataList.Remove(item);
              dataList.Add(item);

              //设置位置
              SetPos(item, tailIndex + 1);
              //设置显示
              SetShow(item, tailIndex + 1);

              headIndex++;
              tailIndex++;
          }
          //向右滑
          while (content.anchoredPosition.x >= -layout.padding.left - headIndex * (layout.cellSize.x + layout.spacing.x)
          && headIndex != 0)
          {
              //将数据列表中的最后一个元素移动到第一个
              RectTransform item = dataList.Last();
              dataList.Remove(item);
              dataList.Insert(0, item);

              //设置位置
              SetPos(item, headIndex - 1);
              //设置显示
              SetShow(item, headIndex - 1);

              headIndex--;
              tailIndex--;
          }
      }
  }

  #endregion

  #region Tool

  /// <summary>
  /// 设置位置
  /// </summary>
  private void SetPos(RectTransform trans, int index)
  {
      if (scrollType == ScrollType.Horizontal)
      {
          trans.anchoredPosition = new Vector2
          (
              index == 0 ? layout.padding.left + firstItemAnchoredPos.x :
              layout.padding.left + firstItemAnchoredPos.x + index * (layout.cellSize.x + layout.spacing.x),
              firstItemAnchoredPos.y
          );
      }
      else if (scrollType == ScrollType.Vertical)
      {
          trans.anchoredPosition = new Vector2
          (
              firstItemAnchoredPos.x,
              index == 0 ? -layout.padding.top + firstItemAnchoredPos.y :
              -layout.padding.top + firstItemAnchoredPos.y - index * (layout.cellSize.y + layout.spacing.y)
          );
      }
  }

  #endregion

  #region 外部调用

  /// <summary>
  /// 初始化
  /// </summary>
  public void Init()
  {
      scrollRect = GetComponent<ScrollRect>();
      content = scrollRect.content;
      layout = content.GetComponent<GridLayoutGroup>();
      scrollRect.onValueChanged.AddListener((Vector2 v) => OnScroll(v));

      //设置布局
      SetLayout();

      //设置头下标和尾下标
      headIndex = 0;
      tailIndex = fixedCount - 1;

      //设置Content大小
      SetContentSize();

      //实例化Item
      InitItem();

      //得到第一个Item的锚点位置
      GetFirstItemAnchoredPos();
  }

  /// <summary>
  /// 设置显示
  /// </summary>
  public void SetShow(RectTransform trans, int index)
  {
      //=====根据需求进行编写
      trans.GetComponentInChildren<Text>().text = index.ToString();
      trans.name = index.ToString();
  }

  /// <summary>
  /// 设置总的数据数量
  /// </summary>
  public void SetTotalCount(int count)
  {
      totalCount = count;
  }

  /// <summary>
  /// 销毁所有的元素
  /// </summary>
  public void DestoryAll()
  {
      for (int i = dataList.Count - 1; i >= 0; i--)
      {
          DestroyImmediate(dataList[i].gameObject);
      }
      dataList.Clear();
  }

  #endregion
}

/// <summary>
/// 滑动类型
/// </summary>
public enum ScrollType
{
  Horizontal,//竖直滑动
  Vertical,//水平滑动
}
