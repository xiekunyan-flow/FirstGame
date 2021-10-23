using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

public static class common
{
    public const int EVENT_ATTACK = 1;
    public static int currentFrame = 0;
    public static float eps = 0.1F;
    public static void Changeeps(float num)
    {
        eps = num;
    }
}

public class CharacterType
{
    public GameObject OBJ;
    public int HP;
    public int ATK;
    public double ATKRange;
    public int ATKSpeed;
    public int hurtSpeed;
    public int moveSpeed;
    public int moveRange;
    public CharacterType(ref GameObject obj, ref int hp, ref int atk, ref int atkrange, ref int atkspeed,ref int movespeed,ref int moverange)
    {
        OBJ = obj; HP = hp; ATK = atk; ATKRange = atkrange;ATKSpeed = atkspeed; moveSpeed = movespeed; moveRange = moverange;
    }
}

public class CharacterData
{
    public int HP;
    public int FreeTime;
    public int ATKIdx;//-1表示没有攻击目标
    public bool death;
    public int Owner;
    public int Num;
    public Vector3 Target; //当前目标点
    public bool IsInway; //是否到达目标点
    public Queue<Vector3> WayQueue;
    public Queue<eventFormatPoint> AttackedQueue;
    public CharacterData(ref int num,ref CharacterType charType,ref int curtime,ref int owner)
    {
        HP = charType.HP; FreeTime = curtime; ATKIdx = -1; death = false;Owner = owner; Num = num;
    }
}

public class CharacterObject
{
    public CharacterType type;
    public GameObject obj;
    public CharacterData data;
    public CharacterObject(ref GameObject gameObj, ref CharacterType charType, ref int num, ref int curTime,ref int owner)
    {
        obj = gameObj; data = new CharacterData(ref num,ref charType,ref curTime,ref owner);
    }

    /******************** 攻击 ********************/
    public bool Attack(Animator animator,ref eventController evc, ref int idx)
    {
        //刷新最后空闲时间
        data.FreeTime = common.currentFrame;

        //调整角度为对应单位方向

        //执行动画效果
        AttackAmination(animator);

        //挂载idx编号Obj受到伤害事件
        eventAttack eveAttack = new eventAttack(type.ATK);
        string jsonAttack = JsonConvert.SerializeObject(eveAttack);
        eventFormat eve = new eventFormat(data.Num,idx, common.EVENT_ATTACK,jsonAttack);
        evc.AddCurrentEvent(eve,this);

        return true;
    }

    public void AttackAmination(Animator animator)
    {
        animator.SetBool("IsAttack", true);
    }

    /******************** 死亡 ********************/
    public bool UpdateHP(int num)
    {
        data.HP += num;
        data.HP = Math.Min(data.HP, type.HP);
        data.HP = Math.Max(data.HP, 0); 
        //执行动画效果

        //检测有无死亡
        if(data.HP == 0)
        {
            data.death = true;
            return true;
        }
        return false;
    }

    public void PopEvent(eventFormat evf,int idx)
    {
        eventFormatPoint evp = data.AttackedQueue.Dequeue();
        if (idx != evp.Idx)
            Debug.Log("evp Is Error,charidx is " + idx + "evpidx is " + evp.Idx);
    }
    public bool inATKRange(CharacterObject obj)
    {
        if (data.Owner == obj.data.Owner) return false;
        if (distanceTo(ref obj) <= type.ATKRange) return true;
        return false; 
    }

    /******************** 寻路 ********************/

    public void FindTarget(Vector3 end)
    {
        Vector3 beg = obj.transform.position;
        Vector3 mid = (beg + end) / 2 ;
        while (Vector3.Distance(mid, end) > common.eps)
        {
            if (ImpactChecking())
            {
                AstarFindWay(ref data.WayQueue);
            }
            else
            {
                data.WayQueue.Enqueue(mid);
            }
            beg = mid;
            mid = (beg + end) / 2;
        }
    }

    //碰撞检测：目前把单位视为原点，不执行碰撞检测
    public bool ImpactChecking()
    {
        return false;
    }

    //目前不需要考虑A*算法
    public void AstarFindWay(ref Queue<Vector3> que)
    {

    }

    public void move(Animator animator)
    {
        //如果本帧能走到下一个目标点，则过度到再下一个
        double dis = Time.deltaTime * type.moveSpeed;
        while (dis > Vector3.Distance(obj.transform.position, data.WayQueue.Peek())){
            dis -= Vector3.Distance(obj.transform.position, data.WayQueue.Peek());
            data.WayQueue.Dequeue();
        }
        //移动到对应位置:对应单位向量*dis

        //角色角度调整为单位向量的方向

        //执行移动动画
        MoveAmination(animator);
    }

    public void MoveAmination(Animator animator)
    {
        animator.SetBool("IsMove", true);
    }

    public void CheckFinishWay(Animator animator,Vector3 end)
    {
        if(Vector3.Distance(obj.transform.position, end) < float.Epsilon){
            //获取新Target（目前不会有障碍，暂不考虑），以及判断胜利条件
        }
        move(animator);
    }

    public double distanceTo(ref CharacterObject atkObj)
    {
        double xdis = (obj.transform.position.x - atkObj.obj.transform.position.x);
        double ydis = (obj.transform.position.y - atkObj.obj.transform.position.y);
        double zdis = (obj.transform.position.z - atkObj.obj.transform.position.z);
        return Math.Sqrt(xdis * xdis + zdis * zdis); 
    }

    public double distanceTo( CharacterObject atkObj)
    {
        double xdis = (obj.transform.position.x - atkObj.obj.transform.position.x);
        double ydis = (obj.transform.position.y - atkObj.obj.transform.position.y);
        double zdis = (obj.transform.position.z - atkObj.obj.transform.position.z);
        return Math.Sqrt(xdis * xdis + zdis * zdis);
    }
}

public class eventFormatPoint
{
    public int Frame;
    public int Idx; //在eventArr[Frame]里的索引

    public eventFormatPoint(int frame,int idx)
    {
        Frame = frame; Idx = idx;
    }
}

public class eventFormat
{
    public bool vaild;
    public int sendObj; //发出Object编号
    public int receObj; //接受Object编号
    public int typeIdx; //事件类型
    public string extra;//额外参数
    public eventFormat(int sendobj,int receobj,int typeidx,string extr)
    {
        vaild = true;
        sendObj = sendobj;
        receObj = receobj;
        typeIdx = typeidx;
        extra = extr;
    }

}

public class eventAttack
{
    public eventAttack(int damagenum)
    {
        damageNum = damagenum;
    }
    public int damageNum;
}

public class eventController
{
    public const int EVENT_ATTACK = 1;
    const int eventArrLen = 100; // 事件链表长度
    public List<eventFormat>[] eventArr;
    public void TraverseEvent(List<CharacterObject> charObjList)
    {
        int idx = 0;
        foreach(eventFormat curEvent in eventArr[common.currentFrame % eventArrLen])
        {
            if (!curEvent.vaild) continue;
            charObjList[curEvent.receObj].PopEvent(curEvent,idx);
            switch(curEvent.typeIdx){
                case EVENT_ATTACK:
                    bool ret = dealEventAttack(charObjList,curEvent);
                    break;
                default:
                    Debug.LogErrorFormat("事件类型错误，事件类型：%d",curEvent.typeIdx);
                    break;
            }
            idx++;
        }
    }

    //伤害结算
    public bool dealEventAttack(List<CharacterObject> charObjList,eventFormat curEvent)
    {
        eventAttack eve = JsonConvert.DeserializeObject<eventAttack>(curEvent.extra);
        bool ret = charObjList[curEvent.receObj].UpdateHP(eve.damageNum);

        if (ret)
        {
            //角色死亡：遍历被攻击队列向所有攻击者发送消息
            foreach (eventFormatPoint evp in charObjList[curEvent.receObj].data.AttackedQueue)
            {
                SetEventUnvaild(evp);
            }
            Debug.Log("traverse queue finished，size is " + charObjList[curEvent.receObj].data.AttackedQueue.Count);
        return true;
        }
        return false;
    }

    public void AddCurrentEvent(eventFormat eve,CharacterObject obj)
    {
        eventFormatPoint evp = new eventFormatPoint(common.currentFrame % eventArrLen,eventArr.Length);
        eventArr[common.currentFrame % eventArrLen].Add(eve);
        obj.data.AttackedQueue.Enqueue(evp);
    }

    public void ClearCurrentEvent()
    {
        eventArr[common.currentFrame % eventArrLen].Clear();
    }

    public void SetEventUnvaild(eventFormatPoint evp)
    {
        eventArr[evp.Frame][evp.Idx].vaild = false;
    }
}

public class MyController : MonoBehaviour
{
    public CharacterType[] charTypeArr;
    List<CharacterObject> charObjList;
    eventController evc;
    int currentFrame;
    bool isInWar;

    //克隆对象
    void InstantCharObj(int num,Vector3 tr,Quaternion rota,int owner){
        //Step 1.复制对象
        GameObject instObj = Instantiate(charTypeArr[num].OBJ, tr, rota) as GameObject;
        //Step 2.装载对象
        CharacterObject charObj = new CharacterObject(ref instObj, ref charTypeArr[num], ref num, ref currentFrame,ref owner);
        //Step 3.add进列表
        charObjList.Add(charObj);
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            ist[i] = true;
        }
        
        InitCharacterType();
        currentFrame = 0;
        isInWar = false;
    }
    bool[] ist = new bool[12];

    // Update is called once per frame
    void Update()
    {
        //TestTranslate();
        currentFrame++;
        if (!isInWar)
        {
            //Step 1: 遍历当前帧事件列表
            evc.TraverseEvent(charObjList);
            //Step 2: 清空当前帧事件列表
            evc.ClearCurrentEvent();
            //Step 3: 遍历对象
            foreach (var obj in charObjList)
            {
                //Step 3.1 检查攻击状态
                int ATKIdx = obj.data.ATKIdx;
                //Step 3.1.1检查有无标记
                if (ATKIdx == -1)
                {
                    //Step 3.1.2 没有标记则遍历所有可能的单位，寻找是否有满足单位。
                    double ATKRange = 1000000;//此处设一个极大值来模拟最大值
                    for (int i = 0; i < charObjList.Count;i++)
                    {
                        //此处有可能obj.inATKRange传的参数charObjList[i]是他本身
                        if (!charObjList[i].data.death  && obj.inATKRange(charObjList[i]))
                        {
                            double dis = obj.distanceTo(charObjList[i]);
                            if(dis < ATKRange)
                            {
                                ATKIdx = i;
                                ATKRange = dis;
                            }
                        }
                        
                    }
                }
                obj.data.ATKIdx = ATKIdx;

                //Step 3.1.3 执行攻击
                if (ATKIdx != -1)
                {
                    if (charObjList[ATKIdx].data.death)
                    {
                        Debug.LogError("编号" + ATKIdx + "的对象已死亡，却仍然被作为目标攻击");
                        continue;
                    }
                    obj.Attack(animator,ref evc, ref ATKIdx);
                    continue;
                }
                //Step 3.2 进行移动
                //Step 3.2.1 选择最近寻路点
                obj.FindTarget(obj.data.Target);
                //Step 3.2.2 A*算法遍历
               // obj.FindWay();
                //Step 3.2.3 检测是否到达
                obj.CheckFinishWay(animator,obj.data.Target);
            }

            //
        }
        else
        {

        }
    }

   

    void InitCharacterType()
    {
       charTypeArr = new CharacterType[charTypeCnt];
       for(int i = 0; i < charTypeCnt; i++)
        {
            charTypeArr[i] = new CharacterType(ref CharObjects[i], ref CharHP[i], ref CharATK[i], ref CharATKRange[i], ref CharATKSpeed[i],ref CharmoveSpeed[i],ref CharmoveRange[i]);
        }
    }
    /*程序调用相关数据*/
    public Animator animator;
    /*初始化相关数据*/
    public const int charTypeCnt = 4;
    public GameObject[] CharObjects;
    public GameObject[] CharObjectsTemp;
    public int[] CharHP;
    public int[] CharATK;
    public int[] CharATKRange;
    public int[] CharATKSpeed;
    public int[] CharmoveSpeed;
    public int[] CharmoveRange;
    void TestTranslate()
    {
        Debug.LogFormat("in");
        for (int i = 0; i < 2; i++)
        {
            Vector3 vc = new Vector3(-1, 0, 8);
            CharObjectsTemp[i].transform.Translate(vc * Time.deltaTime);
        }
        for (int i = 2; i < 4; i++)
        {
            Vector3 vc = new Vector3(1, 0, -8);
            CharObjectsTemp[i].transform.Translate(vc * Time.deltaTime);
        }

        for (int t = 0; t < 2; t++)
        {
            for (int i = 0; i < 2; i++)
            {
                if (!ist[i]) continue;
                for (int j = 2; j < 4; j++)
                {
                    if (!ist[j]) continue;
                    if (Math.Abs((CharObjectsTemp[i].transform.position - CharObjectsTemp[j].transform.position).magnitude) < 1)
                    {
                        Random a = new Random();
                        if (a.Next(100) % 2 == 0)
                        {
                            CharObjectsTemp[i].SetActive(false);
                            ist[i] = false;
                        }
                        else
                        {
                            CharObjectsTemp[j].SetActive(false);
                            ist[j] = false;
                        }
                        break;
                    }
                }
            }
        }
        return;
    }

}
