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
    public int ATKIdx;//-1��ʾû�й���Ŀ��
    public bool death;
    public int Owner;
    public int Num;
    public Vector3 Target; //��ǰĿ���
    public bool IsInway; //�Ƿ񵽴�Ŀ���
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

    /******************** ���� ********************/
    public bool Attack(Animator animator,ref eventController evc, ref int idx)
    {
        //ˢ��������ʱ��
        data.FreeTime = common.currentFrame;

        //�����Ƕ�Ϊ��Ӧ��λ����

        //ִ�ж���Ч��
        AttackAmination(animator);

        //����idx���Obj�ܵ��˺��¼�
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

    /******************** ���� ********************/
    public bool UpdateHP(int num)
    {
        data.HP += num;
        data.HP = Math.Min(data.HP, type.HP);
        data.HP = Math.Max(data.HP, 0); 
        //ִ�ж���Ч��

        //�����������
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

    /******************** Ѱ· ********************/

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

    //��ײ��⣺Ŀǰ�ѵ�λ��Ϊԭ�㣬��ִ����ײ���
    public bool ImpactChecking()
    {
        return false;
    }

    //Ŀǰ����Ҫ����A*�㷨
    public void AstarFindWay(ref Queue<Vector3> que)
    {

    }

    public void move(Animator animator)
    {
        //�����֡���ߵ���һ��Ŀ��㣬����ȵ�����һ��
        double dis = Time.deltaTime * type.moveSpeed;
        while (dis > Vector3.Distance(obj.transform.position, data.WayQueue.Peek())){
            dis -= Vector3.Distance(obj.transform.position, data.WayQueue.Peek());
            data.WayQueue.Dequeue();
        }
        //�ƶ�����Ӧλ��:��Ӧ��λ����*dis

        //��ɫ�Ƕȵ���Ϊ��λ�����ķ���

        //ִ���ƶ�����
        MoveAmination(animator);
    }

    public void MoveAmination(Animator animator)
    {
        animator.SetBool("IsMove", true);
    }

    public void CheckFinishWay(Animator animator,Vector3 end)
    {
        if(Vector3.Distance(obj.transform.position, end) < float.Epsilon){
            //��ȡ��Target��Ŀǰ�������ϰ����ݲ����ǣ����Լ��ж�ʤ������
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
    public int Idx; //��eventArr[Frame]�������

    public eventFormatPoint(int frame,int idx)
    {
        Frame = frame; Idx = idx;
    }
}

public class eventFormat
{
    public bool vaild;
    public int sendObj; //����Object���
    public int receObj; //����Object���
    public int typeIdx; //�¼�����
    public string extra;//�������
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
    const int eventArrLen = 100; // �¼�������
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
                    Debug.LogErrorFormat("�¼����ʹ����¼����ͣ�%d",curEvent.typeIdx);
                    break;
            }
            idx++;
        }
    }

    //�˺�����
    public bool dealEventAttack(List<CharacterObject> charObjList,eventFormat curEvent)
    {
        eventAttack eve = JsonConvert.DeserializeObject<eventAttack>(curEvent.extra);
        bool ret = charObjList[curEvent.receObj].UpdateHP(eve.damageNum);

        if (ret)
        {
            //��ɫ�������������������������й����߷�����Ϣ
            foreach (eventFormatPoint evp in charObjList[curEvent.receObj].data.AttackedQueue)
            {
                SetEventUnvaild(evp);
            }
            Debug.Log("traverse queue finished��size is " + charObjList[curEvent.receObj].data.AttackedQueue.Count);
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

    //��¡����
    void InstantCharObj(int num,Vector3 tr,Quaternion rota,int owner){
        //Step 1.���ƶ���
        GameObject instObj = Instantiate(charTypeArr[num].OBJ, tr, rota) as GameObject;
        //Step 2.װ�ض���
        CharacterObject charObj = new CharacterObject(ref instObj, ref charTypeArr[num], ref num, ref currentFrame,ref owner);
        //Step 3.add���б�
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
            //Step 1: ������ǰ֡�¼��б�
            evc.TraverseEvent(charObjList);
            //Step 2: ��յ�ǰ֡�¼��б�
            evc.ClearCurrentEvent();
            //Step 3: ��������
            foreach (var obj in charObjList)
            {
                //Step 3.1 ��鹥��״̬
                int ATKIdx = obj.data.ATKIdx;
                //Step 3.1.1������ޱ��
                if (ATKIdx == -1)
                {
                    //Step 3.1.2 û�б����������п��ܵĵ�λ��Ѱ���Ƿ������㵥λ��
                    double ATKRange = 1000000;//�˴���һ������ֵ��ģ�����ֵ
                    for (int i = 0; i < charObjList.Count;i++)
                    {
                        //�˴��п���obj.inATKRange���Ĳ���charObjList[i]��������
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

                //Step 3.1.3 ִ�й���
                if (ATKIdx != -1)
                {
                    if (charObjList[ATKIdx].data.death)
                    {
                        Debug.LogError("���" + ATKIdx + "�Ķ�����������ȴ��Ȼ����ΪĿ�깥��");
                        continue;
                    }
                    obj.Attack(animator,ref evc, ref ATKIdx);
                    continue;
                }
                //Step 3.2 �����ƶ�
                //Step 3.2.1 ѡ�����Ѱ·��
                obj.FindTarget(obj.data.Target);
                //Step 3.2.2 A*�㷨����
               // obj.FindWay();
                //Step 3.2.3 ����Ƿ񵽴�
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
    /*��������������*/
    public Animator animator;
    /*��ʼ���������*/
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
