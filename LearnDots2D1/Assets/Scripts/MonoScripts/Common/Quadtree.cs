using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using Vector3 = UnityEngine.Vector3;

//节点位置
enum QuadNodePos
{
    //左上
    LeftTop = 0,
    //左下
    LeftBot = 1,
    //右下
    RightTop = 2,
    //右上
    RightBot = 3,
    Max = 4
}

public class Quadtree<T> where T : GameObjectBase
{
    private QuadRamOptimize<T> m_optimize = new QuadRamOptimize<T>();
    private QuadTreeNode<T> m_root;
    private Dictionary<int, Vector4> m_saveObjLastPosMssage = new Dictionary<int, Vector4>();
    
    public Quadtree(ref Vector3 lb,ref  Vector3 rs)
    {
        m_root = m_optimize.AllocQuadTreeNode();
        Quadtree<T> cur = this;
        m_root.Init( lb, rs, cur,null);
    }

    public  QuadTreeNode<T>  CreateNode( )
    {
        return m_optimize.AllocQuadTreeNode(); 
    }

    public void RecoveryNode(QuadTreeNode<T> node)
    {
        m_optimize.RecoveryQuadTreeNode(node);
    }

    public void Insert(T t)
    {
        m_root.Insert(t);
    }

    public void GetObjectList(out List<T> list,Vector3 pos)
    {
        m_root.GetObjectList(out list,pos);
    } 

    public void UpdateNode(T t)
    {
        Vector4 rectMessage;
        if (m_saveObjLastPosMssage.TryGetValue(t.ObjectId,out rectMessage))
        {
            float minX = rectMessage.X,minY = rectMessage.Y;
            float maxX = rectMessage.Z + rectMessage.X;
            float maxY = rectMessage.Y + rectMessage.W;
       
            bool isInRect = t.trans.position.x  >= minX && t.trans.position.y >= minY && t.trans.position.x <= maxX && t.trans.position.y <= maxY;
            if (isInRect)
            {
                return;
            }
        }

        m_root.Remove(t);
        m_root.Insert(t);
    }

    public void RemoveObjectIdMessage(int objectId)
    {
        if (m_saveObjLastPosMssage.ContainsKey(objectId))
        {
            m_saveObjLastPosMssage.Remove(objectId);
        }
    }
    
    public void SaveObject(int objectId,Vector4 message)
    {
        if (!m_saveObjLastPosMssage.ContainsKey(objectId))
        { 
            m_saveObjLastPosMssage.Add(objectId,message);
        }
        else
        {
            m_saveObjLastPosMssage[objectId] = message;
        }
    }

    public void DeleteNode(T t)
    {
        m_root.Remove(t);
    }
}

public class QuadTreeNode<T> where T : GameObjectBase{
    public List<T>  m_objects ;
    private QuadTreeNode<T> m_fatherNode;
    private List<QuadTreeNode<T>> m_nodesList;

    public int Level = 0;
    //记录当前四叉数节点矩形
    private Vector3 m_leftBot;  //左下角坐标
    private Vector3 m_rectSize;  //宽度，高度
    private Quadtree<T> m_tree;

    public void Init( Vector3 lb, Vector3 rs, Quadtree<T> m, QuadTreeNode<T> father)
    {
        m_leftBot = lb;
        m_rectSize = rs;
        m_tree = m;
        m_fatherNode = father;
    }

    //判断某个坐标是否在改四叉树节点下
    public bool CheckPosIsInNode(Vector3 pos)
    {
        float minX = m_leftBot.x,minY = m_leftBot.y;
        float maxX = m_rectSize.x + m_leftBot.x;
        float maxY = m_rectSize.y + m_leftBot.y;
       
        return pos.x  >= minX && pos.y >= minY && pos.x <= maxX && pos.y <= maxY;
    }

    public void Insert(T t)
    { 
        m_objects.Add(t);
        m_tree.SaveObject(t.ObjectId,new Vector4(m_leftBot.x,m_leftBot.y,m_rectSize.x,m_rectSize.y));
        if (m_objects.Count > QuadTreeTool<T>.NODE_MAX_COUNT && Level <= QuadTreeTool<T>.SPLIT_MAX_COUNT)
        {  
            Split();
            int sIndex = (int)QuadNodePos.LeftTop;
            int eIndex = (int)QuadNodePos.Max;
            QuadTreeNode<T> nodeTmp;
            T tmp;
            for (int i = 0;i<m_objects.Count;i++)
            {
                tmp = m_objects[i];
                for (int j = sIndex;j < eIndex;j++)
                {
                    nodeTmp = m_nodesList[j];
                    if (nodeTmp.CheckPosIsInNode(tmp.trans.position))
                    {
                        nodeTmp.Insert(tmp);
                        break;
                    }
                }
            }
            
            m_objects.Clear();
        }  
    }

    //合并节点
    public void MergerNode()
    {
        QuadTreeNode<T> tmp;
        for (int i=0;i<m_nodesList.Count;i++)
        {
            tmp = m_nodesList[i];
            m_objects.AddRange(tmp.m_objects);
            m_tree.RecoveryNode(tmp);
        }
        m_nodesList.Clear();
    }

    //去掉这个节点
    public void Remove(T t)
    {
        T tmp;
        bool isDelete = false;
        for (int i = m_objects.Count - 1;i >= 0;i--)
        {
            tmp = m_objects[i];
            if (tmp.ObjectId == t.ObjectId)
            {
                m_objects.RemoveAt(i);
                isDelete = true;
                m_tree.RemoveObjectIdMessage(t.ObjectId);
                break;
            }
        }

        if (isDelete)
        {
            if (m_fatherNode == null)
            {
                return;
            }
            
            m_fatherNode.MergerNode();
            return;
        }

        if (m_nodesList.Count > 0)
        {
            QuadTreeNode<T> nodeTmp;
            for (int i = 0;i < m_nodesList.Count;i++)
            {
                nodeTmp = m_nodesList[i];
                nodeTmp.Remove(t);
            }
        }
    }

    public void GetObjectList(out List<T> list,Vector3 pos)
    {
        if (!CheckPosIsInNode(pos))
        {
            list = null;
            return;
        }

        if (m_nodesList.Count <= 0)
        {
            list = m_objects;
            return;
        }
        
        m_nodesList[0].GetObjectList(out list,pos);
        m_nodesList[1].GetObjectList(out list,pos);
        m_nodesList[2].GetObjectList(out list,pos);
        m_nodesList[3].GetObjectList(out list,pos);
    }

    private void Split()
    {
        Vector3 subRectSize = m_rectSize / 2f;
        m_nodesList.Clear();
        
        Split(new Vector3(m_leftBot.x,m_leftBot.y + subRectSize.y),subRectSize); 
        Split(new Vector3(m_leftBot.x,m_leftBot.y),subRectSize); 
        Split(new Vector3(m_leftBot.x + subRectSize.x,m_leftBot.y),subRectSize); 
        Split(new Vector3(m_leftBot.x + subRectSize.x,m_leftBot.y + subRectSize.y),subRectSize);  
    }

    private void Split(Vector3 lb,Vector3 size)
    {
        QuadTreeNode<T> lt = m_tree.CreateNode(); 
        lt.Init(lb,size,m_tree,this);
        lt.Level = Level + 1;
        m_nodesList.Add(lt); 
    } 
    

    public void Clear()
    {
        m_objects.Clear();
        m_nodesList.Clear();
        Level = 0;
    }
}

public class QuadRamOptimize<T> where T : GameObjectBase
{
    private Stack<QuadTreeNode<T>> m_qadTreeNodePool = new Stack<QuadTreeNode<T>>();
    private Stack<List<T>> m_objectListPool = new Stack<List<T>>();  //对象列表的对象池

    public QuadTreeNode<T> AllocQuadTreeNode()
    {
        if (m_qadTreeNodePool.Count > 0)
        {
            return m_qadTreeNodePool.Pop();
        }

        var t = new QuadTreeNode<T>();
        t.m_objects = AllocQuadList(); 
        return t;
    }

    public void RecoveryQuadTreeNode(QuadTreeNode<T> node)
    {
        RecoveryQuadList(node.m_objects);
        node.Clear();
        m_qadTreeNodePool.Push(node);
    }

    public List<T> AllocQuadList()
    {
        if (m_objectListPool.Count > 0)
        {
            return m_objectListPool.Pop();
        }

        return new List<T>();
    }

    public void RecoveryQuadList(List<T> objectList)
    {
        m_objectListPool.Push(objectList);
    }
}

public class QuadTreeTool<T> where T : GameObjectBase
{
    public const int NODE_MAX_COUNT = 20;  //一个节点可以容纳的最大的个数（超过就进行分割）

    public const int SPLIT_MAX_COUNT = 4;  //最多切割的次数
    //辅助用的
    public static List<QuadTreeNode<T>> m_auxiliaryList = new List<QuadTreeNode<T>>();
}