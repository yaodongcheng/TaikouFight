using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class StartFighting : MonoBehaviour
{

    public class ImageCom
    {
        public string pic_path;
        public Texture2D pic_texture;
        public GameObject gobj;
        public SpriteRenderer sr;

        public static void DrawNewPicture(string path, Vector3 pos, Vector2 pivot, ref ImageCom imageCom)
        {
            if (imageCom != null)
                imageCom.Hide();
            imageCom = null;
            imageCom = new ImageCom();
            imageCom.pic_path = path;
            imageCom.pic_texture = (Texture2D)Resources.Load(imageCom.pic_path);
            imageCom.gobj = new GameObject();
            imageCom.gobj.transform.position = pos;
            imageCom.sr = imageCom.gobj.AddComponent<SpriteRenderer>();
            imageCom.sr.sprite = Sprite.Create(imageCom.pic_texture, new Rect(0, 0, imageCom.pic_texture.width, imageCom.pic_texture.height), pivot);
            //gobj.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            imageCom.Show();
        }
        public static void ChangePicture(string path, ref ImageCom imageCom)
        {
            if (imageCom != null)
            {
                imageCom.pic_path = path;
                imageCom.pic_texture = (Texture2D)Resources.Load(imageCom.pic_path);
                imageCom.sr.sprite = Sprite.Create(imageCom.pic_texture, new Rect(0, 0, imageCom.pic_texture.width, imageCom.pic_texture.height), imageCom.sr.sprite.pivot);
            }
        }
        public static void DrawNewPicture(Sprite sprite, Vector3 pos, Vector2 pivot, ref ImageCom imageCom)
        {
            imageCom = new ImageCom();
            imageCom.gobj = new GameObject();
            imageCom.gobj.transform.position = pos;
            imageCom.sr = imageCom.gobj.AddComponent<SpriteRenderer>();
            imageCom.sr.sprite = sprite;
            //gobj.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            imageCom.Show();
        }
        public static void DrawNewPicture(string path, Vector3 pos, Vector2 pivot, ref ImageCom imageCom, bool enabled)
        {
            DrawNewPicture(path, pos, pivot, ref imageCom);
            imageCom.sr.enabled = enabled;
        }

        public void Hide()
        {
            sr.enabled = false;
        }
        public void Show()
        {
            sr.enabled = true;
        }
    }

    //代码进一步优化 设置基类 Person 派生类 Player AIPlayer 。AIPlayer继续派生出 Friend Enemy
    public class Person
    {
        public Direction dir = Direction.down;
        public ImageCom circle;
        protected Vector3 pos;
        public float move_radius = 1.6f;
        public StartFighting startFighting;
        protected float transparency_human = 0.7f;
        public float movelength_cum;

        //动画帧
        protected float intervalTime = 0.15f;
        protected float interval = 0.1f;
        public int frame_index = 0;
        public int frame_total = 0;
        public int act = 0;
        //行走角度

        //行走速度
        public float walk_speed;
        public float time_needed = 1.0f;
        public float distance;
        //攻击内径、外径、扩展角
        public float in_radium;
        public float out_radium;
        public float extend_radian;
        public bool wait = false;
        //血条底 血条红
        public ImageCom blood_floor;
        public ImageCom blood;
        public float power_full = 100;
        public float power_current = 100;
        public float power_justnow = 100;
        public float force = 50;
        public float armor = 50;
        public float damage_thisbout = 0;
        public bool isLife = true;
        public GameObject gobj;
        public SpriteRenderer sr;
        public int Act
        {
            get
            {
                return act;
            }
            set
            {
                act = value;
                frame_total = cols[act];
            }
        }
        public void Hide()
        {
            sr.enabled = false;
            circle.Hide();
            blood.Hide();
            blood_floor.Hide();
        }
        public void Show()
        {
            sr.enabled = true;
            circle.Show();
            blood.Show();
            blood_floor.Show();
        }
        public Person(Vector3 initPos, StartFighting startFighting_out, Direction face)
        {
            startFighting = startFighting_out;
            pos = initPos;
            frame_index = 0;
            in_radium = 0;
            out_radium = 0.8f;
            extend_radian = 30 * Mathf.PI / 180;
            gobj = new GameObject();
            sr = gobj.AddComponent<SpriteRenderer>();
            gobj.transform.position = pos;
            dir = face;
            sr.sprite = (idle_alldirs[(int)face] as ArrayList)[0] as Sprite;
            sr.material.color = new Color(1, 1, 1, transparency_human);
        }
        public void PlayAnimation()
        {
            interval -= Time.deltaTime;
            if (interval < 0)
            {
                interval = intervalTime;
                frame_index = (frame_index + 1) % cols[act];
                sr.sprite = (actions[act][(int)dir] as ArrayList)[frame_index] as Sprite;
            }
        }
        public void PlayAnimationOnce()
        {
            interval -= Time.deltaTime;
            if (interval < 0)
            {
                if (frame_index < cols[act] - 1)
                {
                    interval = intervalTime;
                    frame_index = (frame_index + 1) % cols[act];
                    sr.sprite = (actions[act][(int)dir] as ArrayList)[frame_index] as Sprite;
                }
                else
                {
                    interval = intervalTime;
                    act = 0;
                    frame_total = cols[act];
                    frame_index = (frame_index + 1) % cols[act];
                    sr.sprite = (actions[act][(int)dir] as ArrayList)[frame_index] as Sprite;
                }
            }
        }
        public void PlayAnimationAndKeepFinalAct()
        {
            interval -= Time.deltaTime;
            //比如 若cols[act] = 8 frame_index从0%8=0 1%8=1 .. 7%8== 7 8%8 =8因此 frameindex == frametotal-1时 则便已经结束
            if (interval < 0)
            {
                if (frame_index < cols[act] - 1)
                {
                    interval = intervalTime;
                    frame_index = (frame_index + 1) % cols[act];
                    sr.sprite = (actions[act][(int)dir] as ArrayList)[frame_index] as Sprite;
                }
            }
        }
        public void enterStatusChooseMovePoint() //进入选取移动点阶段
        {
            wait = false;
            damage_thisbout = 0;
        }
        public void statusChooseMovePoint()
        {
            PlayAnimation();
        }
        public void enterStatusChooseAttackDirection() //进入选取攻击方向阶段
        {
        }
        public void statusChooseAttackDirection()
        {
        }
        public void enterStatusMoveToTarget()
        {
            movelength_cum = 0;
            Act = 1;
        }
        public void enterStatusBrandishWeapon()
        {
        }
        public void enterStatusBeaten() //进入被攻击阶段
        {
            if (damage_thisbout > 0)
                Act = 6;
            else
                Act = 0;
        }
        public void Death()
        {
            interval -= Time.deltaTime;
            if (interval < 0)
            {
                if (frame_index < cols[act] - 1)
                {
                    interval = intervalTime;
                    frame_index = (frame_index + 1) % cols[act];
                    sr.sprite = (actions[act][(int)dir] as ArrayList)[frame_index] as Sprite;
                }
                else
                {
                    isLife = false;
                }
            }
        }
        public void statusZero()     //处在有人死亡阶段
        {
            Death();
        }
        public void enterStatusSkill_ReduceBlood() //进入技能扣血阶段
        {
            if (damage_thisbout > 0)
                Act = 6;
            else
                Act = 0;
        }

    }
    public class RealPlayer : Person
    {
        public ImageCom moverange;
        public float radian_PlayerToGhost;
        public List<int> enemy_hit_index;
        public Skill currentSkill;
        public int qili = 0;
        public int Qili
        {
            get
            {
                return qili;
            }
            set
            {
                qili = value;
                FightUI.playerCard.ChangeQili(value);
            }
        }
        public new void Hide()
        {
            ((Person)this).Hide();
            moverange.Hide();
        }
        public new void Show()
        {
            ((Person)this).Show();

            moverange.Show();
        }
        public Vector3 Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = new Vector3(value.x, value.y, pos.z);
                gobj.transform.position = new Vector3(value.x, value.y, gobj.transform.position.z);
                circle.gobj.transform.position = new Vector3(value.x, value.y, circle.gobj.transform.position.z);
                moverange.gobj.transform.position = new Vector3(value.x, value.y, moverange.gobj.transform.position.z);
                blood_floor.gobj.transform.position = new Vector3(value.x, value.y - 0.2f, blood_floor.gobj.transform.position.z);
                blood.gobj.transform.position = new Vector3(value.x - 0.27f, value.y - 0.2f, blood.gobj.transform.position.z);
            }
        }
        public RealPlayer(Vector3 initPos, StartFighting startFighting_out, Direction face) : base(initPos, startFighting_out, face)
        {

            enemy_hit_index = new List<int>();
            ImageCom.DrawNewPicture("fighter/move_range", new Vector3(pos.x, pos.y, (int)ShowLayer.MoveRange), new Vector2(0.5f, 0.5f), ref moverange);
            ImageCom.DrawNewPicture("fighter/blue_circle", new Vector3(pos.x, pos.y, (int)ShowLayer.Circle), new Vector2(0.5f, 0.7f), ref circle);
            ImageCom.DrawNewPicture("fighter/blue_floor", new Vector3(pos.x, pos.y - 0.2f, (int)ShowLayer.BloodFloor), new Vector2(0.5f, 0.5f), ref blood_floor);
            ImageCom.DrawNewPicture("fighter/blue_blood", new Vector3(pos.x - 0.27f, pos.y - 0.2f, (int)ShowLayer.Blood), new Vector2(0.05f, 0.5f), ref blood);
            power_justnow = power_current;
            Pos = initPos;
            force = 70;
        }
        public new void enterStatusChooseMovePoint() //进入选取移动点阶段
        {
            ((Person)this).enterStatusChooseMovePoint();
            blood_floor.sr.enabled = true;
            blood.sr.enabled = true;
            moverange.sr.enabled = true;
            circle.sr.enabled = true;
            Act = 0;
            power_justnow = power_current;
            armor = force;
        }
        //使用基类的statusChooseMovePoint()
        public new void enterStatusChooseAttackDirection() //进入选取攻击方向阶段
        {
            ((Person)this).enterStatusChooseAttackDirection();
            moverange.sr.enabled = false;
        }
        //使用基类的statusChooseAttackDirection
        public new void enterStatusMoveToTarget() //进入移动阶段
        {
            ((Person)this).enterStatusMoveToTarget();
            dir = TaikouMath.AngleToDirection(radian_PlayerToGhost * 180 / Mathf.PI);
            distance = TaikouMath.CalculateDistance(Pos, ghost.Pos);
            walk_speed = distance / time_needed;
            if (Qili < 8)
                Qili += 1;
        }
        public void statusMoveToTarget() //处在移动阶段
        {
            PlayAnimation();
            if (movelength_cum < distance)
            {
                float movelength_thisframe = Time.deltaTime * walk_speed;
                float move_y = movelength_thisframe * Mathf.Sin(radian_PlayerToGhost);
                float move_x = movelength_thisframe * Mathf.Cos(radian_PlayerToGhost);
                Pos += new Vector3(move_x, move_y, 0);
                movelength_cum += movelength_thisframe;
            }
            else
            {
                //到达目标点 换方向。换动作 是idle还是attack
                movelength_cum = 0;
                bool hasenemy = isTouchEnemy();
                dir = ghost.ghost_dir_decided;
                if (hasenemy)
                {
                    Act = 2;
                    wait = true;
                }
                else
                {
                    Act = 0;
                    wait = true;
                }
                frame_index = 0;
            }
        }
        public bool isTouchEnemy()//设置击中多少敌人
        {
            //现在只能攻击一个敌人 攻击多个敌人还需要考虑
            //三个核心概念 一是外径 二是内径 三是扩展角          
            enemy_hit_index.Clear();
            for (int i = 0; i < enemys.Count; i++)
            {
                Enemy tempEnemy = enemys[i];
                float distance_player2enemy = TaikouMath.CalculateDistance(Pos, tempEnemy.Pos);
                if (distance_player2enemy >= in_radium && distance_player2enemy <= out_radium)
                {
                    float radian_player2enemy = TaikouMath.getRadianFromTo(Pos, tempEnemy.Pos);
                    float delta_radian = radian_player2enemy - ghost.ghost_radian_decided;
                    if (delta_radian > Mathf.PI)
                        delta_radian -= Mathf.PI * 2;
                    if (delta_radian < -Mathf.PI)
                        delta_radian += Mathf.PI * 2;
                    if (delta_radian <= extend_radian && delta_radian >= -extend_radian)
                    {
                        enemy_hit_index.Add(i);
                    }
                }
            }
            if (enemy_hit_index.Count > 0)
                return true;
            return false;
        }
        //使用基类的enterStatusBrandishWeapon()
        public void statusBrandishWeapon() //处在挥刀阶段
        {
            if (Act != 5)
                PlayAnimationOnce();
        }
        //使用基类的enterStatusBeaten()
        public void statusBeaten() //处在被攻击阶段
        {
            PlayAnimationOnce();
            if (damage_thisbout > 0)
            {
                ReduceBlood();
            }
        }
        public void ReduceBlood()
        {
            if (act == 6 && interval > 0)
            {
                //最稳妥是求出本帧的掉血量
                float blood_lost = Time.deltaTime / intervalTime * damage_thisbout;
                power_current = power_current - blood_lost;
                power_current = power_current >= 0 ? power_current : 0;
                // power_current = power_justnow - (1 - interval / intervalTime) * damage_thisbout;
                blood.gobj.transform.localScale = new Vector3(power_current / power_full, 1, 1);
                FightUI.playerCard.ChangeBlood((int)(100.0 * power_current / power_full));
            }
        }
        public void enterStatusZero() //进入有人死亡阶段
        {
            Act = 7;
            interval = intervalTime;
            frame_index = 0;
        }
        //使用基类的statusZero()
        public void CheckBeBeaten() //设置敌人的受伤程度
        {
            //攻击看系数 伤害=武力值*角度系数*技能系数* (150-对方武力值)/150
            //正面攻击系数 0.6 侧面攻击系数 0.8 背面攻击系数 1.0
            //普通攻击系数 0.5 月影攻击系数 0.6 转攻击系数 1.0
            if (enemy_hit_index.Count > 0)
            {
                for (int i = 0; i < enemy_hit_index.Count; i++)
                {
                    Enemy tempEnemy = enemys[enemy_hit_index[i]];
                    //角度系数
                    int deltaDir = (int)dir - (int)tempEnemy.dir;
                    if (deltaDir > 4)
                        deltaDir -= 8;
                    else if (deltaDir < -4)
                        deltaDir += 8;
                    deltaDir = deltaDir >= 0 ? deltaDir : -deltaDir;
                    float ratio_angle = 0.5f * (1 + (4 - deltaDir) / 4.0f);
                    float ratio_kind = 0.5f;
                    float ratio_protect = (150 - tempEnemy.force) / 150;
                    float damage = force * ratio_angle * ratio_kind * ratio_protect;
                    //  Debug.Log("敌人"+ enemy_hit_index[i]+"受到伤害" + damage);
                    tempEnemy.damage_thisbout += damage;
                }
            }
        }
        public void enterStatusSkill_Charge()  //进入技能蓄力阶段
        {
            Qili -= SkillUse.GetSkill(currentSkill).qi_consume;
            switch (currentSkill)
            {
                case Skill.Fenshen:
                    SkillUse.Fenshen.enterStatusSkill_Charge();

                    break;
                case Skill.Kongchan:
                    SkillUse.Kongchan.enterStatusSkill_Charge();
                    break;
                case Skill.Renquan:
                    SkillUse.Renquan.enterStatusSkill_Charge();
                    break;
                case Skill.Yueying:
                    SkillUse.Yueying.enterStatusSkill_Charge();
                    break;
                case Skill.Zhiyu:
                    SkillUse.Zhiyu.enterStatusSkill_Charge();
                    break;
                case Skill.Zhuan:
                    SkillUse.Zhuan.enterStatusSkill_Charge();
                    break;
            }
        }
        public void statusSkill_Charge() //处在技能蓄力阶段
        {
            switch (currentSkill)
            {
                case Skill.Fenshen:
                    SkillUse.Fenshen.statusSkill_Charge();
                    break;
                case Skill.Kongchan:
                    SkillUse.Kongchan.statusSkill_Charge();
                    break;
                case Skill.Renquan:
                    SkillUse.Renquan.statusSkill_Charge();
                    break;
                case Skill.Yueying:
                    SkillUse.Yueying.statusSkill_Charge();
                    break;
                case Skill.Zhiyu:
                    SkillUse.Zhiyu.statusSkill_Charge();
                    break;
                case Skill.Zhuan:
                    SkillUse.Zhuan.statusSkill_Charge();
                    break;
            }
        }
        public void enterStatusSkill_Release() //进入技能释放阶段
        {
            switch (currentSkill)
            {
                case Skill.Fenshen:
                    SkillUse.Fenshen.enterStatusSkill_Release();
                    break;
                case Skill.Kongchan:
                    SkillUse.Kongchan.enterStatusSkill_Release();
                    break;
                case Skill.Renquan:
                    SkillUse.Renquan.enterStatusSkill_Release();
                    break;
                case Skill.Yueying:
                    SkillUse.Yueying.enterStatusSkill_Release();
                    break;
                case Skill.Zhiyu:
                    SkillUse.Zhiyu.enterStatusSkill_Release();
                    break;
                case Skill.Zhuan:
                    SkillUse.Zhuan.enterStatusSkill_Release();
                    break;
            }
        }
        public void statusSkill_Release()  //处在技能释放阶段
        {

            switch (currentSkill)
            {
                case Skill.Fenshen:
                    SkillUse.Fenshen.statusSkill_Release();
                    break;
                case Skill.Kongchan:
                    SkillUse.Kongchan.statusSkill_Release();
                    break;
                case Skill.Renquan:
                    SkillUse.Renquan.statusSkill_Release();
                    break;
                case Skill.Yueying:
                    SkillUse.Yueying.statusSkill_Release();
                    break;
                case Skill.Zhiyu:
                    SkillUse.Zhiyu.statusSkill_Release();
                    break;
                case Skill.Zhuan:
                    SkillUse.Zhuan.statusSkill_Release();
                    break;
            }
        }
        public void statusSkill_Release_After()
        {
            // SkillUse.Kongchan.statusSkill_Release_After();
            // SkillUse.Fenshen.statusSkill_Release_After();
            SkillUse.Zhiyu.statusSkill_Release_After();
            switch (currentSkill)
            {
                case Skill.Fenshen:
                    SkillUse.Fenshen.statusSkill_Release_After();
                    break;
                case Skill.Kongchan:
                    SkillUse.Kongchan.statusSkill_Release_After();
                    break;
                case Skill.Renquan:
                    SkillUse.Renquan.statusSkill_Release_After();
                    break;
                case Skill.Yueying:
                    SkillUse.Yueying.statusSkill_Release_After();
                    break;
                case Skill.Zhiyu:
                    SkillUse.Zhiyu.statusSkill_Release_After();
                    break;
                case Skill.Zhuan:
                    SkillUse.Zhuan.statusSkill_Release_After();
                    break;
            }
        }
        public void CheckBeBeatenBySkill() //设置敌人的受伤程度
        {
            //攻击看系数 伤害=武力值*角度系数*技能系数* (150-对方武力值)/150
            //正面攻击系数 0.6 侧面攻击系数 0.8 背面攻击系数 1.0
            //普通攻击系数 0.5 月影攻击系数 0.6 转攻击系数 1.0
            if (enemy_hit_index.Count > 0)
            {
                for (int i = 0; i < enemy_hit_index.Count; i++)
                {
                    Enemy tempEnemy = enemys[enemy_hit_index[i]];
                    //角度系数为1

                    float ratio_angle = 1.0f;
                    float ratio_kind = 1.0f;
                    float ratio_protect = (150 - tempEnemy.force) / 150;
                    float damage = force * ratio_angle * ratio_kind * ratio_protect;
                    //  Debug.Log("敌人"+ enemy_hit_index[i]+"受到伤害" + damage);
                    tempEnemy.damage_thisbout += damage;
                }
            }
        }
        //使用基类的enterStatusSkill_ReduceBlood()
        public void statusSkill_ReduceBlood()  //处在技能扣血阶段
        {
            PlayAnimationOnce();
            if (damage_thisbout > 0)
            {
                ReduceBlood();
            }
        }
        public void enterStatusSkill_MoveAfterSkill()
        {
            if (Act != 5)
                Act = 0;
            enemy_hit_index.Clear();
            damage_thisbout = 0;
        }
        public void statusSkill_MoveAfterSkill()
        {
            if (Act != 5)
                PlayAnimation();
        }
        public void enterStatusSkill_Zero()
        {
            enterStatusZero();
        }
        public void statusSkill_Zero()
        {
            statusZero();
        }
        public void enterStatusDefend()
        {
            Act = 5;
            frame_index = 0;
            if (Qili <= 6)
                Qili += 2;
            else if (Qili == 7)
                Qili += 1;
            //防御力提升
            armor += 20;
        }
        public void statusDefend()
        {
            PlayAnimationAndKeepFinalAct();
        }
        public void AddBlood(int increase)
        {
            power_current += increase;
            power_current = power_current > power_full ? power_full : power_current;
            blood.gobj.transform.localScale = new Vector3(power_current / power_full, 1, 1);
            FightUI.playerCard.ChangeBlood((int)(100 * power_current / power_full));
        }
    }
    public class AIPlayer : Person
    {
        public float AI = 0.5f;
        public float radian_ToTarget;
        public Direction dir_ToTarget = Direction.down;
        public Vector3 Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
                gobj.transform.position = value;
                circle.gobj.transform.position = value;
                blood_floor.gobj.transform.position = new Vector3(value.x, value.y - 0.2f, blood_floor.gobj.transform.position.z);
                blood.gobj.transform.position = new Vector3(value.x - 0.27f, value.y - 0.2f, blood.gobj.transform.position.z);
            }
        }
        public bool isHit;
        //Hide()与Show()完全继承基类
        public AIPlayer(Vector3 initPos, StartFighting startFighting_out, Direction face, float ai) : base(initPos, startFighting_out, face)
        {

            dir_ToTarget = dir;
            AI = ai;
            force = 30;
            move_radius = 1.6f * force / 100;
        }
        //使用基类的enterStatusChooseMovePoint() 
        //使用基类的statusChooseMovePoint()
        //使用基类的enterStatusChooseAttackDirection
        //使用基类的statusChooseAttackDirection
        public new void enterStatusMoveToTarget()
        {
            ((Person)this).enterStatusMoveToTarget();
            //判定行走方向与攻击范围
            //行走
            //以主角的旧坐标为目标还是新坐标（虚影坐标）为目标 
            float target_x = player.Pos.x + (ghost.Pos.x - player.Pos.x) * AI;
            float target_y = player.Pos.y + (ghost.Pos.y - player.Pos.y) * AI;
            radian_ToTarget = TaikouMath.getRadianFromTo(Pos, new Vector3(target_x, target_y, 0));
            dir_ToTarget = TaikouMath.AngleToDirection(radian_ToTarget * 180 / Mathf.PI);
            dir = dir_ToTarget;
            distance = TaikouMath.CalculateDistance(Pos, new Vector3(target_x, target_y, 0));
            distance = distance > move_radius ? move_radius : distance;
            walk_speed = distance / time_needed;
            //攻击 其实可以放在状态三结束时进行判断
        }


        //使用基类的enterStatusBrandishWeapon()
        public void statusBrandishWeapon()
        {
            //攻击
            PlayAnimationOnce();
        }
        //使用基类的enterStatusBeaten()
        public void statusBeaten()
        {
            PlayAnimationOnce();
            if (damage_thisbout > 0)
            {
                ReduceBlood();
            }
        }
        public void ReduceBlood()
        {
            if (act == 6 && interval > 0)
            {
                float blood_lost = Time.deltaTime / intervalTime * damage_thisbout;
                power_current = power_current - blood_lost;
                power_current = power_current >= 0 ? power_current : 0;
                blood.gobj.transform.localScale = new Vector3(power_current / power_full, 1, 1);
            }
        }
        public void enterStatusZero()
        {
            Act = 7;
        }
        //使用基类的statusZero()

        public void enterStatusSkill_Charge()  //进入技能蓄力阶段
        {

        }
        public void statusSkill_Charge() //处在技能蓄力阶段
        {

        }
        public void enterStatusSkill_Release() //进入技能释放阶段
        {

        }
        public void statusSkill_Release()  //处在技能释放阶段
        {

        }
        //使用基类的enterStatusSkill_ReduceBlood()
        public void statusSkill_ReduceBlood()  //处在技能扣血阶段
        {
            PlayAnimationOnce();
            if (damage_thisbout > 0)
            {
                ReduceBlood();
            }
        }
        public void enterStatusSkill_MoveAfterSkill()
        {

            Act = 1;

            movelength_cum = 0;

        }

        public void enterStatusSkill_Zero()
        {
            enterStatusZero();
        }
        public void statusSkill_Zero()
        {
            statusZero();
        }
        public void enterStatusDefend()
        {

        }
        public void statusDefend()
        {
            PlayAnimation();
        }
    }

    public class Player : RealPlayer
    {
        public Player(StartFighting startFighting_out, Vector3 initPos, Direction face) : base(initPos, startFighting_out, face)
        {

        }
    }
    public class Enemy : AIPlayer
    {
        public int target_playerorfriend = 10; //0表示是player >0表示是friend的编号 从1开始。因此求对应的friend的索引还要减一
        public Enemy(Vector3 initPos, Direction face, StartFighting startFighting_out, float iq) : base(initPos, startFighting_out, face, iq)
        {
            ImageCom.DrawNewPicture("fighter/red_circle", new Vector3(pos.x, pos.y, (int)ShowLayer.MoveRange), new Vector2(0.5f, 0.7f), ref circle);
            ImageCom.DrawNewPicture("fighter/red_floor", new Vector3(pos.x, pos.y - 0.2f, (int)ShowLayer.BloodFloor), new Vector2(0.5f, 0.5f), ref blood_floor);
            ImageCom.DrawNewPicture("fighter/red_blood", new Vector3(pos.x - 0.27f, pos.y - 0.2f, (int)ShowLayer.Blood), new Vector2(0.05f, 0.5f), ref blood);
            Pos = initPos;
        }
        public void ChooseTarget()
        {
            float randnum = Random.Range(0.0f, 1.0f);
            target_playerorfriend = (int)((friends.Count - 0 + 1) * randnum + 0);
            Debug.Log("friendscount" + friends.Count + "选择目标为" + target_playerorfriend);
        }
        public new void enterStatusMoveToTarget()
        {
            ((Person)this).enterStatusMoveToTarget();
            //判定行走方向与攻击范围
            //行走
            //以主角为目标还是以友军为目标
            float target_x;
            float target_y;
            ChooseTarget();
            if (target_playerorfriend == 0)
            {
                target_x = player.Pos.x + (ghost.Pos.x - player.Pos.x) * AI;
                target_y = player.Pos.y + (ghost.Pos.y - player.Pos.y) * AI;
            }
            else
            {
                target_x = friends[target_playerorfriend - 1].Pos.x;
                target_y = friends[target_playerorfriend - 1].Pos.y;
            }
            radian_ToTarget = TaikouMath.getRadianFromTo(Pos, new Vector3(target_x, target_y, 0));
            dir_ToTarget = TaikouMath.AngleToDirection(radian_ToTarget * 180 / Mathf.PI);
            dir = dir_ToTarget;
            distance = TaikouMath.CalculateDistance(Pos, new Vector3(target_x, target_y, 0));
            distance = distance > move_radius ? move_radius : distance;
            walk_speed = distance / time_needed;
            //攻击 其实可以放在状态三结束时进行判断
        }
        public new void enterStatusSkill_MoveAfterSkill()
        {
            damage_thisbout = 0;
            float target_x;
            float target_y;
            ChooseTarget();
            if (target_playerorfriend == 0)
            {
                target_x = player.Pos.x + (ghost.Pos.x - player.Pos.x) * AI;
                target_y = player.Pos.y + (ghost.Pos.y - player.Pos.y) * AI;
            }
            else
            {
                target_x = friends[target_playerorfriend - 1].Pos.x;
                target_y = friends[target_playerorfriend - 1].Pos.y;
            }
            radian_ToTarget = TaikouMath.getRadianFromTo(Pos, new Vector3(target_x, target_y, 0));
            dir_ToTarget = TaikouMath.AngleToDirection(radian_ToTarget * 180 / Mathf.PI);
            dir = dir_ToTarget;
            Act = 1;
            distance = TaikouMath.CalculateDistance(Pos, new Vector3(target_x, target_y, 0));
            distance = distance > move_radius ? move_radius : distance;
            walk_speed = distance / time_needed;
            movelength_cum = 0;
            Debug.Log("计算出的distance" + distance);
            //对于放技能的敌人
            //原地不动
        }
        public void statusMoveToTarget()
        {
            //行走到目标位置
            PlayAnimation();
            // Debug.Log(movelength_cum + " " + distance);
            if (movelength_cum < distance)
            {
                float movelength_thisframe = Time.deltaTime * walk_speed;
                float move_y = movelength_thisframe * Mathf.Sin(radian_ToTarget);
                float move_x = movelength_thisframe * Mathf.Cos(radian_ToTarget);
                Pos += new Vector3(move_x, move_y, 0);
                movelength_cum += movelength_thisframe;
            }
            else
            {
                //到达目标点 换方向。换动作 是idle还是attack
                float target_radium = 0;
                isHit = isTouchPlayerOrFriend(ref target_radium);
                movelength_cum = 0;
                //  distance = 0;
                // dir = TaikouMath.AngleToDirection(tar);
                if (isHit)
                {
                    Act = 2;
                    wait = true;
                    dir = TaikouMath.AngleToDirection(target_radium * 180 / Mathf.PI);
                }
                else
                {
                    Act = 0;
                    wait = true;
                }
                frame_index = 0;
            }
        }
        public bool isTouchPlayerOrFriend(ref float attackradium)
        {
            //三个核心概念 一是外径 二是内径 三是扩展角 但是对于AI来说 扩展角不需要
            Vector3 targetPos;
            if (target_playerorfriend == 0)
                targetPos = player.Pos;
            else
                targetPos = friends[target_playerorfriend - 1].Pos;

            float distance_enemy2player = TaikouMath.CalculateDistance(Pos, targetPos);
            if (distance_enemy2player >= in_radium && distance_enemy2player <= out_radium)
            {
                float radian_enemy2player = TaikouMath.getRadianFromTo(Pos, targetPos);
                attackradium = radian_enemy2player;
                return true;
            }
            attackradium = 0;
            return false;
        }
        public void statusSkill_MoveAfterSkill()
        {

            //对于没有放技能的敌人
            statusMoveToTarget();
            //对于放技能的敌人
            //原地不动
        }
        public void CheckBeBeaten()
        {
            if (isHit)
            {
                //角度系数
                if (target_playerorfriend == 0)
                {
                    int deltaDir = (int)player.dir - (int)dir;
                    if (deltaDir > 4)
                        deltaDir -= 8;
                    else if (deltaDir < -4)
                        deltaDir += 8;
                    deltaDir = deltaDir >= 0 ? deltaDir : -deltaDir;
                    float ratio_angle = 0.5f * (1 + (4 - deltaDir) / 4.0f);
                    float ratio_kind = 0.5f;
                    float ratio_protect = (150 - player.armor) / 150;
                    float damage = force * ratio_angle * ratio_kind * ratio_protect;
                    player.damage_thisbout += damage;
                }
                else
                {
                    int deltaDir = (int)friends[target_playerorfriend - 1].dir - (int)dir;
                    if (deltaDir > 4)
                        deltaDir -= 8;
                    else if (deltaDir < -4)
                        deltaDir += 8;
                    deltaDir = deltaDir >= 0 ? deltaDir : -deltaDir;
                    float ratio_angle = 0.5f * (1 + (4 - deltaDir) / 4.0f);
                    float ratio_kind = 0.5f;
                    float ratio_protect = (150 - friends[target_playerorfriend - 1].force) / 150;
                    float damage = force * ratio_angle * ratio_kind * ratio_protect;
                    friends[target_playerorfriend - 1].damage_thisbout += damage;
                }
            }
        }
    }
    public class Friend : AIPlayer
    {
        public int target_enemy = 10; //表示对应的敌人的索引
        public Friend(Vector3 initPos, Direction face, StartFighting startFighting_out, float iq) : base(initPos, startFighting_out, face, iq)
        {
            ImageCom.DrawNewPicture("fighter/green_circle", new Vector3(pos.x, pos.y, (int)ShowLayer.MoveRange), new Vector2(0.5f, 0.7f), ref circle);
            ImageCom.DrawNewPicture("fighter/green_floor", new Vector3(pos.x, pos.y - 0.2f, (int)ShowLayer.BloodFloor), new Vector2(0.5f, 0.5f), ref blood_floor);
            ImageCom.DrawNewPicture("fighter/green_blood", new Vector3(pos.x - 0.27f, pos.y - 0.2f, (int)ShowLayer.Blood), new Vector2(0.05f, 0.5f), ref blood);
            Pos = initPos;
        }
        public void ChooseTarget()
        {

            float randnum = Random.Range(0.0f, 1.0f);
            target_enemy = (int)((enemys.Count - 1 - 0 + 1) * randnum + 0);
            Debug.Log("enemysscount" + enemys.Count + "randnum" + randnum + "选择目标为" + target_enemy);

        }
        //enterStatusChooseMovePoint 与基类一致
        //statusChooseMovePoint 与基类一致
        //使用基类的enterStatusChooseAttackDirection
        //使用基类的statusChooseAttackDirection
        public new void enterStatusMoveToTarget()
        {
            ((Person)this).enterStatusMoveToTarget();
            //判定行走方向与攻击范围
            //选择一个敌人作为目标
            ChooseTarget();
            Debug.Log("确定目标敌人" + target_enemy);
            float target_x = enemys[target_enemy].Pos.x;
            float target_y = enemys[target_enemy].Pos.y;
            radian_ToTarget = TaikouMath.getRadianFromTo(Pos, new Vector3(target_x, target_y, 0));
            dir_ToTarget = TaikouMath.AngleToDirection(radian_ToTarget * 180 / Mathf.PI);
            dir = dir_ToTarget;
            distance = TaikouMath.CalculateDistance(Pos, new Vector3(target_x, target_y, 0));
            distance = distance > move_radius ? move_radius : distance;
            walk_speed = distance / time_needed;
        }
        public new void enterStatusSkill_MoveAfterSkill()
        {

            ChooseTarget();

            damage_thisbout = 0;
            //对于没有放技能的敌人
            float target_x = enemys[target_enemy].Pos.x;
            float target_y = enemys[target_enemy].Pos.y;
            radian_ToTarget = TaikouMath.getRadianFromTo(Pos, new Vector3(target_x, target_y, 0));
            dir_ToTarget = TaikouMath.AngleToDirection(radian_ToTarget * 180 / Mathf.PI);
            dir = dir_ToTarget;
            Act = 1;
            distance = TaikouMath.CalculateDistance(Pos, new Vector3(target_x, target_y, 0));
            distance = distance > move_radius ? move_radius : distance;
            walk_speed = distance / time_needed;
            movelength_cum = 0;
            Debug.Log("计算出的distance" + distance);
            //对于放技能的敌人
            //原地不动
        }
        public void statusMoveToTarget()
        {
            //行走到目标位置
            PlayAnimation();
            // Debug.Log(movelength_cum + " " + distance);
            if (movelength_cum < distance)
            {
                float movelength_thisframe = Time.deltaTime * walk_speed;
                float move_y = movelength_thisframe * Mathf.Sin(radian_ToTarget);
                float move_x = movelength_thisframe * Mathf.Cos(radian_ToTarget);
                Pos += new Vector3(move_x, move_y, 0);
                movelength_cum += movelength_thisframe;
            }
            else
            {
                //到达目标点 换方向。换动作 是idle还是attack
                float target_radium = 0;
                isHit = isTouchEnemy(ref target_radium);
                movelength_cum = 0;
                //  distance = 0;
                // dir = TaikouMath.AngleToDirection(tar);
                if (isHit)
                {
                    Act = 2;
                    wait = true;
                    dir = TaikouMath.AngleToDirection(target_radium * 180 / Mathf.PI);
                }
                else
                {
                    Act = 0;
                    wait = true;
                }
                frame_index = 0;
            }
        }
        public bool isTouchEnemy(ref float attackradium)
        {
            //三个核心概念 一是外径 二是内径 三是扩展角 但是对于AI来说 扩展角不需要
            if (target_enemy >= enemys.Count)
                ChooseTarget();
            float distance_friend2enemy = TaikouMath.CalculateDistance(Pos, enemys[target_enemy].Pos);
            if (distance_friend2enemy >= in_radium && distance_friend2enemy <= out_radium)
            {
                float radian_friend2enemy = TaikouMath.getRadianFromTo(Pos, enemys[target_enemy].Pos);
                attackradium = radian_friend2enemy;
                return true;
            }
            attackradium = 0;
            return false;
        }
        public void statusSkill_MoveAfterSkill()
        {
            //对于没有放技能的敌人
            statusMoveToTarget();
            //对于放技能的敌人
            //原地不动
        }
        public void CheckBeBeaten()
        {
            if (isHit)
            {
                //角度系数
                int deltaDir = (int)enemys[target_enemy].dir - (int)dir;
                if (deltaDir > 4)
                    deltaDir -= 8;
                else if (deltaDir < -4)
                    deltaDir += 8;
                deltaDir = deltaDir >= 0 ? deltaDir : -deltaDir;
                float ratio_angle = 0.5f * (1 + (4 - deltaDir) / 4.0f);
                float ratio_kind = 0.5f;
                float ratio_protect = (150 - enemys[target_enemy].force) / 150;
                float damage = force * ratio_angle * ratio_kind * ratio_protect;
                enemys[target_enemy].damage_thisbout += damage;
            }
        }
    }

    public enum Skill
    {
        Yueying,
        Fenshen,
        Renquan,
        Zhuan,
        Zhiyu,
        Kongchan
    }

    public class SkillUse
    {
        public int qi_consume;
        public string name;
        public string description;
        public SkillUse(int q, string n, string d)
        {
            qi_consume = q;
            name = n;
            description = d;
        }
        public static SkillUse GetSkill(Skill skill)
        {
            SkillUse skillUse = null;
            switch (skill)
            {
                case Skill.Yueying:
                    skillUse = new SkillUse(6, "月影藏刀", "向十六方发出强劲剑气");
                    break;
                case Skill.Fenshen:
                    skillUse = new SkillUse(4, "影分身术", "东瀛忍术，分出和本体同样能力的幻影");
                    break;
                case Skill.Renquan:
                    skillUse = new SkillUse(4, "通灵忍犬", "东瀛忍术，召唤出忍犬攻击敌人");
                    break;
                case Skill.Zhuan:
                    skillUse = new SkillUse(8, "奥义之转", "上泉信纲所创的新阴流奥义，躲无可躲");
                    break;
                case Skill.Zhiyu:
                    skillUse = new SkillUse(3, "治愈之术", "简单的治疗术");
                    break;
                case Skill.Kongchan:
                    skillUse = new SkillUse(4, "空蝉之术", "东瀛忍术，陷入包围时可金蝉脱壳");
                    break;
            }
            return skillUse;
        }
        public override string ToString()
        {
            int l1 = System.Text.Encoding.Default.GetBytes(name).Length;

            string empty = "".PadRight(28 - 2 * l1);

            return "   " + name + empty + "  耗气" + qi_consume.ToString() + "  " + description;
        }
        public string MyToString()
        {
            int l1 = System.Text.Encoding.Default.GetBytes(name).Length;

            string empty = "".PadRight(28 - 2 * l1);

            return "   " + name + empty + "  耗气" + qi_consume.ToString() + "  " + description;
        }
        public class Yueying
        {
            public static void enterStatusSkill_Charge()  //进入技能蓄力阶段
            {
                player.frame_index = 0;
                player.Act = 4;
                ghost.Hide();
                player.moverange.Hide();
                SpecialEffect tempSE = new SpecialEffect("skill/发动技能/2", 5, player.Pos.x, player.Pos.y);
                specialEffectList.Add(tempSE);
                //背景更换
                ImageCom.ChangePicture("fightplace/月影", ref backdrop);
                //对于月影来说 所有敌人都会被击中 //此后要改进为 只有16个方向的人会被击中
                player.enemy_hit_index.Clear();



                for (int i = 0; i < enemys.Count; i++)
                {
                    float delta_x = enemys[i].Pos.x - player.Pos.x;
                    float delta_y = enemys[i].Pos.y - player.Pos.y;
                    for (int j = 0; j < 16; j++)
                    {
                        float alpha = 22.5f * Mathf.PI * j / 180;
                        if (alpha == Mathf.PI / 2)
                            alpha += 0.00001f;
                        float z = 0.25f / Mathf.Abs(Mathf.Cos(alpha));
                        float z1 = Mathf.Abs(delta_y - Mathf.Tan(alpha) * delta_x);
                        Debug.Log("角度" + 22.5f * j + "敌人" + i + "z1=" + z1 + "z= " + z);
                        if (z1 <= z)
                        {
                            player.enemy_hit_index.Add(i);
                            break;
                        }
                    }
                }
            }
            public static void statusSkill_Charge() //处在技能蓄力阶段
            {
                player.PlayAnimationAndKeepFinalAct();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void enterStatusSkill_Release() //进入技能释放阶段
            {
                player.frame_index = 0;
                player.Act = 0;
                specialEffectList.Clear();
                for (int i = 0; i < 16; i++)
                {
                    SpecialEffect tempSE = new SpecialEffect("skill/月影/" + (i + 1), 3, player.Pos.x, player.Pos.y);
                    tempSE.SetMove(10, 1.5f, 22.5f * i * Mathf.PI / 180);
                    specialEffectList.Add(tempSE);
                }
            }
            public static void statusSkill_Release()  //处在技能释放阶段
            {
                player.PlayAnimation();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                    specialEffectList[i].Move();
                }
            }
            public static void statusSkill_Release_After()
            {

            }
        }
        public class Zhuan
        {

            public static void enterStatusSkill_Charge()  //进入技能蓄力阶段
            {
                player.frame_index = 0;
                player.Act = 4;
                ghost.Hide();
                player.moverange.Hide();
                SpecialEffect tempSE = new SpecialEffect("skill/发动技能/1", 9, player.Pos.x, player.Pos.y);
                specialEffectList.Add(tempSE);
                //背景更换
                ImageCom.ChangePicture("fightplace/转", ref backdrop);
                //对于转 月影来说 所有敌人都会被击中
                player.enemy_hit_index.Clear();
                for (int i = 0; i < enemys.Count; i++)
                {
                    player.enemy_hit_index.Add(i);
                }
            }
            public static void statusSkill_Charge() //处在技能蓄力阶段
            {
                player.PlayAnimationAndKeepFinalAct();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void enterStatusSkill_Release() //进入技能释放阶段
            {
                player.frame_index = 0;
                player.Act = 0;
                specialEffectList.Clear();
                for (int i = 0; i < enemys.Count; i++)
                {
                    SpecialEffect tempSE = new SpecialEffect("skill/转/转", 5, enemys[i].gobj.transform.position.x - 0.15f, enemys[i].gobj.transform.position.y);
                    tempSE.SetMove(0, 1.5f, Mathf.PI);
                    specialEffectList.Add(tempSE);
                }
            }
            public static void statusSkill_Release()  //处在技能释放阶段
            {
                player.PlayAnimation();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void statusSkill_Release_After()
            {

            }

        }
        public class Renquan
        {
            public static void enterStatusSkill_Charge()  //进入技能蓄力阶段
            {
                player.frame_index = 0;

                ghost.Hide();
                player.moverange.Hide();
                SpecialEffect tempSE = new SpecialEffect("skill/发动技能/2", 5, player.Pos.x, player.Pos.y);
                specialEffectList.Add(tempSE);
                //背景更换
                ImageCom.ChangePicture("fightplace/忍犬", ref backdrop);
                //对于忍犬来说 随机找一个敌人击中
                player.enemy_hit_index.Clear();
                int target = (int)((enemys.Count - 1 - 0 + 1) * Random.Range(0.0f, 1.0f) + 0);
                player.enemy_hit_index.Add(target);
                player.Act = 10;
            }
            public static void statusSkill_Charge() //处在技能蓄力阶段
            {
                player.PlayAnimationAndKeepFinalAct();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void enterStatusSkill_Release() //进入技能释放阶段
            {
                player.frame_index = 0;
                player.Act = 0;
                specialEffectList.Clear();
                for (int i = 0; i < player.enemy_hit_index.Count; i++)
                {
                    SpecialEffect tempSE = new SpecialEffect("skill/忍犬之术/1", 6, 4, enemys[player.enemy_hit_index[i]].Pos.y - 0.2f);
                    tempSE.SetMove(9, 1.5f, Mathf.PI);
                    specialEffectList.Add(tempSE);
                }
            }
            public static void statusSkill_Release()  //处在技能释放阶段
            {
                player.PlayAnimation();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimation();
                    specialEffectList[i].Move();
                }

            }
            public static void statusSkill_Release_After()
            {

            }

        }
        public class Kongchan
        {
            static bool release_after_done;
            public static void enterStatusSkill_Charge()  //进入技能蓄力阶段
            {
                player.frame_index = 0;
                player.Act = 10;
                ghost.Hide();
                player.moverange.Hide();
                SpecialEffect tempSE = new SpecialEffect("skill/发动技能/2", 5, player.Pos.x, player.Pos.y);
                specialEffectList.Add(tempSE);
                //背景更换
                ImageCom.ChangePicture("fightplace/空蝉", ref backdrop);
                //对于空蝉来说 没有敌人可击中
                player.enemy_hit_index.Clear();
            }
            public static void statusSkill_Charge() //处在技能蓄力阶段
            {
                player.PlayAnimationAndKeepFinalAct();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void enterStatusSkill_Release() //进入技能释放阶段
            {
                player.frame_index = 0;
                player.Act = 0;
                specialEffectList.Clear();
                for (int i = 0; i < 1; i++)
                {
                    SpecialEffect tempSE = new SpecialEffect("skill/替身术/1", 10, player.Pos.x, player.Pos.y);
                    tempSE.SetMove(0, 1.5f, Mathf.PI);
                    specialEffectList.Add(tempSE);
                }
                player.Hide();
                release_after_done = false;
            }
            public static void statusSkill_Release()  //处在技能释放阶段
            {
                player.PlayAnimation();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
                if (specialEffectList[0].frame_index == specialEffectList[0].frame_total - 3 && release_after_done == false)
                {
                    float new_x = Random.Range(-3.5f, 3.5f);
                    float new_y = Random.Range(-2.5f, 2.5f);
                    player.Pos = new Vector3(new_x, new_y, (int)ShowLayer.Human);
                    ((Person)player).Show();
                    Debug.Log("主角瞬移到" + new_x + " " + new_y);
                    release_after_done = true;
                }
            }
            public static void statusSkill_Release_After()
            {

            }
        }
        public class Fenshen
        {
            static float radian;
            static float distance;
            public static void enterStatusSkill_Charge()  //进入技能蓄力阶段
            {
                player.frame_index = 0;

                ghost.Hide();
                player.moverange.Hide();
                SpecialEffect tempSE = new SpecialEffect("skill/发动技能/2", 5, player.Pos.x, player.Pos.y);
                specialEffectList.Add(tempSE);
                //背景更换
                ImageCom.ChangePicture("fightplace/分身", ref backdrop);
                //对于分身来说 没有敌人击中
                player.enemy_hit_index.Clear();
                player.Act = 10;
            }
            public static void statusSkill_Charge() //处在技能蓄力阶段
            {
                player.PlayAnimationAndKeepFinalAct();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void enterStatusSkill_Release() //进入技能释放阶段
            {
                player.frame_index = 0;
                player.Act = 0;
                specialEffectList.Clear();
                for (int i = 0; i < 1; i++)
                {
                    SpecialEffect tempSE = new SpecialEffect("skill/分身术/" + ((int)player.dir + 1), 3, player.Pos.x, player.Pos.y);
                    radian = TaikouMath.getRadianFromTo(player.Pos, new Vector3(0, 0, 0));
                    distance = 1;
                    tempSE.SetMove(distance, 1.5f, radian);
                    specialEffectList.Add(tempSE);
                }
            }
            public static void statusSkill_Release()  //处在技能释放阶段
            {
                player.PlayAnimation();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimation();
                    specialEffectList[i].Move();
                }
            }
            public static void statusSkill_Release_After()
            {
                float new_x = player.Pos.x + Mathf.Cos(radian) * distance;
                float new_y = player.Pos.y + Mathf.Sin(radian) * distance;
                Friend newfrd = new Friend(new Vector3(new_x, new_y, (int)ShowLayer.Human), player.dir, startFighting_this, 1);
                newfrd.ChooseTarget();
                friends.Add(newfrd);
                totalfriends.Add(newfrd);
            }

        }
        public class Zhiyu
        {
            public static void enterStatusSkill_Charge()  //进入技能蓄力阶段
            {
                player.frame_index = 0;
                player.Act = 10;
                ghost.Hide();
                player.moverange.Hide();
                SpecialEffect tempSE = new SpecialEffect("skill/发动技能/2", 5, player.Pos.x, player.Pos.y);
                specialEffectList.Add(tempSE);
                //背景更换
                ImageCom.ChangePicture("fightplace/治愈", ref backdrop);
                //对于空蝉来说 没有敌人可击中
                player.enemy_hit_index.Clear();
            }

            public static void statusSkill_Charge() //处在技能蓄力阶段
            {
                player.PlayAnimationAndKeepFinalAct();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void enterStatusSkill_Release() //进入技能释放阶段
            {
                player.frame_index = 0;
                player.Act = 0;
                specialEffectList.Clear();
                for (int i = 0; i < 1; i++)
                {
                    SpecialEffect tempSE = new SpecialEffect("skill/治愈术/1", 6, player.Pos.x, player.Pos.y);
                    tempSE.SetMove(0, 1.5f, Mathf.PI);
                    specialEffectList.Add(tempSE);
                }

            }
            public static void statusSkill_Release()  //处在技能释放阶段
            {
                player.PlayAnimation();
                for (int i = 0; i < specialEffectList.Count; i++)
                {
                    specialEffectList[i].PlayAnimationAndKeepFinalAct();
                }
            }
            public static void statusSkill_Release_After()
            {
                player.AddBlood(30);
            }
        }

    }
    public static List<SpecialEffect> specialEffectList = new List<SpecialEffect>();
    public class SpecialEffect
    {
        float interval = 0.15f;
        float intervalTime = 0.15f;
        ImageCom effect_imgcom;
        public int frame_index;
        public int frame_total;
        List<Sprite> spriteList;
        Texture2D sourceTexture;
        public float movelength_cum = 0;
        public float distance;
        public float walk_speed;
        public float walk_radian;
        public float walk_time;
        private Vector3 pos;
        public Vector3 Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
                effect_imgcom.gobj.transform.position = value;
            }
        }
        public SpecialEffect(string path, int frame_total_out, float x, float y)
        {
            sourceTexture = Resources.Load(path) as Texture2D;
            frame_total = frame_total_out;
            frame_index = 0;
            spriteList = new List<Sprite>();
            //遍历每一个方向的动作数
            for (int j = 0; j < frame_total; j++)
            {
                Rect spriteRect = new Rect(sourceTexture.width * j / frame_total, 0, sourceTexture.width / frame_total, sourceTexture.height);
                Sprite tempSprite = Sprite.Create(sourceTexture, spriteRect, new Vector2(0.5f, 0.3f));
                spriteList.Add(tempSprite);
            }
            //
            pos = new Vector3(x, y, (int)ShowLayer.Skill);
            ImageCom.DrawNewPicture(spriteList[0], pos, Vector2.zero, ref effect_imgcom);

        }
        public void Hide()
        {
            effect_imgcom.Hide();
        }
        public void SetMove(float distance_out, float time_out, float radian_out)
        {
            distance = distance_out;
            walk_time = time_out;
            walk_speed = distance / walk_time;
            walk_radian = radian_out;
            movelength_cum = 0;
        }
        public void Move()
        {
            //radian 的范围 从x正轴逆时针算
            if (movelength_cum < distance)
            {
                float movelength_thisframe = Time.deltaTime * walk_speed;
                float move_y = movelength_thisframe * Mathf.Sin(walk_radian);
                float move_x = movelength_thisframe * Mathf.Cos(walk_radian);
                Pos += new Vector3(move_x, move_y, 0);
                movelength_cum += movelength_thisframe;
            }
            else
            {
                //到达目标点 换方向。换动作 是idle还是attack                
            }
        }
        public void PlayAnimationAndKeepFinalAct()
        {
            interval -= Time.deltaTime;
            if (interval < 0)
            {
                if (frame_index < frame_total - 1)
                {
                    interval = intervalTime;
                    frame_index = (frame_index + 1) % frame_total;
                    effect_imgcom.sr.sprite = spriteList[frame_index];
                }
            }
        }
        public void PlayAnimation()
        {
            interval -= Time.deltaTime;
            if (interval < 0)
            {
                interval = intervalTime;
                frame_index = (frame_index + 1) % frame_total;
                effect_imgcom.sr.sprite = spriteList[frame_index];
            }
        }
        public void SetFirstPos(float x, float y)
        {
            effect_imgcom.gobj.transform.position = new Vector3(x, y, (int)ShowLayer.Skill);
        }
    }
    public class Ghost
    {
        ImageCom ghost_pic;
        private Vector3 pos;
        public ImageCom attackrange;
        float transparency_ghost = 0.5f;
        ImageCom circle;
        public void Hide()
        {
            ghost_pic.Hide();
            circle.Hide();
            attackrange.Hide();
        }
        public void Show()
        {
            ghost_pic.Show();
            circle.Show();
            attackrange.Show();
        }
        public Ghost()
        {

            ghost_pic = new ImageCom();
            ghost_pic.gobj = new GameObject();
            ghost_pic.sr = ghost_pic.gobj.AddComponent<SpriteRenderer>();
            ghost_pic.sr.sprite = StartFighting.player.sr.sprite;
            ghost_pic.sr.material.color = new Color(1, 1, 1, transparency_ghost);
            pos = new Vector3(StartFighting.player.Pos.x + 0.5f, StartFighting.player.Pos.y, (int)ShowLayer.Ghost);
            ghost_pic.gobj.transform.position = pos;
            ImageCom.DrawNewPicture("fighter/sword_range", pos, new Vector2(0.5f, 0.5f), ref attackrange, false);
            ImageCom.DrawNewPicture("fighter/justice_circle", new Vector3(pos.x, pos.y, (int)ShowLayer.Circle), new Vector2(0.5f, 0.7f), ref circle);
            circle.sr.material.color = ghost_pic.sr.material.color;
        }
        public Vector3 Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = new Vector3(value.x, value.y, pos.z);
                ghost_pic.gobj.transform.position = new Vector3(value.x, value.y, ghost_pic.gobj.transform.position.z);
                circle.gobj.transform.position = new Vector3(value.x, value.y, circle.gobj.transform.position.z);
            }
        }
        public Direction ghost_dir_decided;
        public float ghost_radian_decided;
        Direction new_dir;
        public void enterStatusChooseMovePoint()
        {

            attackrange.sr.enabled = false;
            ghost_pic.sr.enabled = true;

        }
        public void enterStatusChooseAttackDirection()
        {
            attackrange.sr.enabled = true;
            attackrange.gobj.transform.position = Pos;
        }
        public void statusChooseAttackDirection()
        {
            ghost_radian_decided = getGhostRadian();
            float angle = ghost_radian_decided * 180 / Mathf.PI;
            attackrange.gobj.transform.localEulerAngles = new Vector3(0, 0, angle);
            //根据angle确定direction
            new_dir = TaikouMath.AngleToDirection(angle);
            ghost_pic.sr.sprite = (actions[player.act][(int)new_dir] as ArrayList)[0] as Sprite;
        }
        public void enterStatusMoveToTarget()
        {
            ghost_pic.sr.enabled = false;
            attackrange.sr.enabled = false;
            ghost_dir_decided = new_dir;
            circle.sr.enabled = false;

        }
        public void enterStatusDefend()
        {
            Hide();
        }
        public void statusMoveToTarget()
        {

        }
        public static void UpdateGhost()
        {
            Vector2 mouse_pos_pixel = Input.mousePosition;
            Vector3 mouse_pos_world = Camera.main.ScreenToWorldPoint(new Vector3(mouse_pos_pixel.x, mouse_pos_pixel.y, 0));
            float distance = TaikouMath.CalculateDistance(player.Pos, mouse_pos_world);
            if (distance <= player.move_radius)
            {
                ghost.Pos = new Vector3(mouse_pos_world.x, mouse_pos_world.y, (int)ShowLayer.Ghost);
            }
            else
            {
                float newx = player.Pos.x + (player.move_radius / distance) * (mouse_pos_world.x - player.Pos.x);
                float newy = player.Pos.y + (player.move_radius / distance) * (mouse_pos_world.y - player.Pos.y);
                ghost.Pos = new Vector3(newx, newy, (int)ShowLayer.Ghost);
            }
        }
        public float getGhostRadian()
        {
            Vector2 mouse_pos_pixel = Input.mousePosition;
            Vector3 mouse_pos_world = Camera.main.ScreenToWorldPoint(new Vector3(mouse_pos_pixel.x, mouse_pos_pixel.y, 0));
            float deltaY = mouse_pos_world.y - Pos.y;
            float deltaX = mouse_pos_world.x - Pos.x;
            float angle_radian = TaikouMath.CalculateRadianAngle(deltaX, deltaY);
            return angle_radian;
        }
    }
    public class TaikouMath
    {
        public static float CalculateDistance(Vector3 pos1, Vector3 pos2)
        {
            return Mathf.Sqrt((pos1.x - pos2.x) * (pos1.x - pos2.x) + (pos1.y - pos2.y) * (pos1.y - pos2.y));
        }
        public static float getRadianFromTo(Vector3 from, Vector3 to)
        {
            float deltaY = to.y - from.y;
            float deltaX = to.x - from.x;
            float angle_radian = TaikouMath.CalculateRadianAngle(deltaX, deltaY);
            return angle_radian;
        }
        public static float CalculateRadianAngle(float deltaX, float deltaY)
        {
            deltaX = deltaX == 0.0f ? 0.0001f : deltaX;
            float k = deltaY / deltaX;
            float angle_radian = 0;
            //Mathf.Atan(k)
            if (deltaX >= 0)
            {
                //规定角度范围为 -PI~+PI
                angle_radian = Mathf.Atan(k);
            }
            else
            {
                angle_radian = Mathf.Atan(k);
                if (angle_radian > 0)
                    angle_radian -= Mathf.PI;
                else
                    angle_radian += Mathf.PI;
                //例如 45
            }
            return angle_radian;
        }
        public static Direction AngleToDirection(float angle)
        {
            if (angle >= 0)
            {
                if (angle <= 22.5f)
                    return Direction.right;
                else if (angle <= 67.5f)
                    return Direction.rightup;
                else if (angle <= 112.5f)
                    return Direction.up;
                else if (angle <= 157.5f)
                    return Direction.leftup;
                else
                    return Direction.left;
            }
            else
            {
                if (angle >= -22.5f)
                    return Direction.right;
                else if (angle >= -67.5f)
                    return Direction.rightdown;
                else if (angle >= -112.5f)
                    return Direction.down;
                else if (angle >= -157.5f)
                    return Direction.leftdown;
                else
                    return Direction.left;
            }
        }
    }


    public enum Direction
    {
        right = 0,
        rightdown = 1,
        down = 2,
        leftdown = 3,
        left = 4,
        leftup = 5,
        up = 6,
        rightup = 7
    }
    public enum FightStatus
    {
        Zero = 0,
        ChooseMovePoint = 1,
        ChooseAttackDirection,
        MoveToTarget,
        BrandishWeapon,
        Beaten,
        End,
        Openning,
        RightMenu,
        SkillMenu,
        ConfirmSkill,
        WinCondition,
        Defend,
        Skill,
        Skill_ReduceBlood,
        Skill_Charge,
        Skill_Release,
        Skill_Zero,
        Skill_MoveAfterSkill

    }
    public enum ShowLayer
    {
        BackGround = 0,
        MoveRange = -1,
        Circle = -2,
        AttackRange = -3,
        Ghost = -4,
        Human = -5,
        BloodFloor = -6,
        Blood = -7,
        Skill = -8,
        UI = -9
    }

    public static ImageCom backdrop;
    public static Player player;
    public static Ghost ghost;
    public static List<Enemy> enemys;
    public static List<Enemy> totalenemys;
    public static List<Friend> friends;
    public static List<Friend> totalfriends;
    public static FightStatus fightStatus = FightStatus.Openning;
    public static ArrayList idle_alldirs; //八方小步走
    public static ArrayList walk_alldirs; //八方大步走
    public static ArrayList downcut_alldirs; //八方砍刀
    public static ArrayList upcut_alldirs; //八方撩刀
    public static ArrayList doublecut_alldirs; //八方双刀
    public static ArrayList defend_alldirs; //八方格挡
    public static ArrayList beaten_alldirs; //八方被砍
    public static ArrayList fall_alldirs; //八方扑地
    public static ArrayList salute_alldirs; //八方起手式
    public static ArrayList bow_alldirs;  //八方收刀
    public static ArrayList boxing_alldirs; //八方猜拳
    public static List<int> cols;
    public static List<ArrayList> actions;
    public static int bout_index = 1;
    public static List<ImageCom> bout_pic;
    public static float interval_out = 0.15f;
    public static float intervalTime_out = 0.15f;
    public static ImageCom win_pic;
    public static StartFighting startFighting_this;
    public static int FriendsInitNum = 1;
    public static int EnemysInitNum = 2;
    // Use this for initialization
    void Start()
    {
        //更改相机显示范围
        startFighting_this = this;
        fightStatus = FightStatus.Openning;
        //  Openning();
        PrepareForFight();
        FightUI.StartUI();
        /*
        FightUI.options.Hide();
        FightUI.listbox.Hide();
        FightUI.buttonGroup.Hide();
        FightUI.skillConfirmGroup.Hide();
        FightUI.skillAsk.Hide();
        FightUI.winCon.Hide();
        FightUI.playerCard.Hide();
        */

    }

    public static void ReturnToOpenning()
    {
        fightStatus = FightStatus.Openning;
        FightUI.opening.Show();
        // FightUI.playerCard.Hide();
        FightUI.playerCard.ChangeBlood(100);
        player.Hide();
        player = null;
        ghost.Hide();
        ghost = null;
        for (int i = 0; i < totalenemys.Count; i++)
        {
            totalenemys[i].Hide();
            totalenemys[i] = null;
        }
        totalenemys.Clear();
        totalenemys = null;
        for (int i = 0; i < totalfriends.Count; i++)
        {
            totalfriends[i].Hide();
            totalfriends[i] = null;
        }
        totalfriends.Clear();
        totalfriends = null;
        win_pic.Hide();
        win_pic = null;
    }
    public static void PrepareForFight()
    {
        Camera.main.GetComponent<Camera>().orthographicSize = 3;
        ImageCom.DrawNewPicture("fightplace/bg", new Vector3(-3 * 4.0f / 3, -3, (int)ShowLayer.BackGround), Vector2.zero, ref backdrop);
        InitAllAction();

        
          //载入音乐
          GameObject emptyGobj = GameObject.Find("GameObject");
          AudioSource audioSource = emptyGobj.AddComponent<AudioSource>();
          audioSource.clip = Resources.Load("music/fight") as AudioClip;
          audioSource.Play();
          audioSource.loop = true;
          
    }
    public static void EnterFightPlace()
    {
        player = new Player(startFighting_this, new Vector3(0, 0, (int)ShowLayer.Human), Direction.down);
        ghost = new Ghost();
        enemys = new List<Enemy>();
        player.Qili = 0;
        friends = new List<Friend>();
        for (int i = 0; i < EnemysInitNum; i++)
        {
            float rand_x = Random.Range(-3.5f, 3.5f);
            float rand_y = Random.Range(-2.5f, 2.5f);
            Enemy enemy1 = new Enemy(new Vector3(rand_x, rand_y, (int)ShowLayer.Human), Direction.down, startFighting_this, 0.3f);
            enemys.Add(enemy1);
        }
        for (int i = 0; i < FriendsInitNum; i++)
        {
            float rand_x = Random.Range(-3.5f, 3.5f);
            float rand_y = Random.Range(-2.5f, 2.5f);
            Friend friend1 = new Friend(new Vector3(rand_x, rand_y, (int)ShowLayer.Human), Direction.down, startFighting_this, 0.3f);
            friends.Add(friend1);
        }




        for (int i = 0; i < enemys.Count; i++)
            enemys[i].ChooseTarget();
        for (int i = 0; i < friends.Count; i++)
            friends[i].ChooseTarget();

        totalenemys = new List<Enemy>();
        for (int i = 0; i < enemys.Count; i++)
            totalenemys.Add(enemys[i]);
        totalfriends = new List<Friend>();
        for (int i = 0; i < friends.Count; i++)
            totalfriends.Add(friends[i]);

        player.Hide();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].Hide();
        ghost.Hide();
        backdrop.Hide();

        fightStatus = FightStatus.ChooseMovePoint;
        player.Show();
        player.enterStatusChooseMovePoint();
        for (int i = 0; i < enemys.Count; i++)
        {
            enemys[i].Show();
            enemys[i].enterStatusChooseMovePoint();
        }
        ghost.Show();
        ghost.enterStatusChooseMovePoint();
        FightUI.playerCard.Show();
        FightUI.opening.Hide();
        backdrop.Show();
    }

    public static void InitAction(ref ArrayList actionList, string path)
    {
        actionList = new ArrayList();
        for (int i = 0; i < 8; i++)
        {
            actionList.Add(new ArrayList());
        }
        ArrayList textureList = new ArrayList();
        Texture2D tempTexture;
        for (int i = 0; i < 8; i++)
        {
            string texturepath = string.Format("{0}/{1}", path, i + 1);
            tempTexture = Resources.Load(texturepath) as Texture2D;
            textureList.Add(tempTexture);
        }
        tempTexture = (textureList[0] as Texture2D);
        int tcols = tempTexture.width / tempTexture.height;
        cols.Add(tcols);
        //遍历八个方向
        for (int i = 0; i < 8; i++)
        {

            ArrayList tempArray = actionList[i] as ArrayList;
            tempTexture = (textureList[i] as Texture2D);
            //遍历每一个方向的动作数
            for (int j = 0; j < tcols; j++)
            {
                //Debug.Log(path + " i=" + i + " j=" + j);
                Rect spriteRect = new Rect(tempTexture.width * j / tcols, 0, tempTexture.width / tcols, tempTexture.height);
                Sprite tempSprite = Sprite.Create(tempTexture, spriteRect, new Vector2(0.5f, 0.3f));
                tempArray.Add(tempSprite);
            }
        }
        actions.Add(actionList);
    }
    public static void InitAllAction()
    {
        cols = new List<int>();
        actions = new List<ArrayList>();
        InitAction(ref idle_alldirs, "fighter/八方小步走");
        InitAction(ref walk_alldirs, "fighter/八方大步走");
        InitAction(ref downcut_alldirs, "fighter/八方砍刀");
        InitAction(ref upcut_alldirs, "fighter/八方撩刀");
        InitAction(ref doublecut_alldirs, "fighter/八方双刀");
        InitAction(ref defend_alldirs, "fighter/八方格挡");
        InitAction(ref beaten_alldirs, "fighter/八方被砍");
        InitAction(ref fall_alldirs, "fighter/八方扑地");
        InitAction(ref salute_alldirs, "fighter/八方起手式");
        InitAction(ref bow_alldirs, "fighter/八方收刀");
        InitAction(ref boxing_alldirs, "fighter/八方猜拳");
    }

    // Update is called once per frame
    void Update()
    {
        switch (fightStatus)
        {
            case FightStatus.ChooseMovePoint:
                statusChooseMovePoint();
                break;
            case FightStatus.ChooseAttackDirection:
                statusChooseAttackDirection();
                break;
            case FightStatus.MoveToTarget:
                statusMoveToTarget();
                break;
            case FightStatus.BrandishWeapon:
                statusBrandishWeapon();
                break;
            case FightStatus.Beaten:
                statusBeaten();
                break;
            case FightStatus.Zero:
                statusZero();
                break;
            case FightStatus.Openning:
                break;
            case FightStatus.RightMenu:
                statusRightMenu();
                break;
            case FightStatus.WinCondition:
                statusWinCondition();
                break;
            case FightStatus.SkillMenu:
                statusSkillMenu();
                break;
            case FightStatus.ConfirmSkill:
                statusConfirmSkill();
                break;
            case FightStatus.End:
                statusEnd();
                break;
            case FightStatus.Defend:
                statusDefend();
                break;
            case FightStatus.Skill_Release:
                statusSkill_Release();
                break;
            case FightStatus.Skill_Charge:
                statusSkill_Charge();
                break;
            case FightStatus.Skill_ReduceBlood:
                statusSkill_ReduceBlood();
                break;
            case FightStatus.Skill_Zero:
                statusSkill_Zero();
                break;
            case FightStatus.Skill_MoveAfterSkill:
                statusSkill_MoveAfterSkill();
                break;
        }
    }
    void statusChooseMovePoint()
    {
        Ghost.UpdateGhost();
        player.statusChooseMovePoint();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusChooseMovePoint();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusChooseMovePoint();

        if (Input.GetMouseButtonDown(0))
        {
            fightStatus = FightStatus.ChooseAttackDirection;
            player.enterStatusChooseAttackDirection();
            ghost.enterStatusChooseAttackDirection();
            player.radian_PlayerToGhost = TaikouMath.getRadianFromTo(player.Pos, ghost.Pos);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            FightUI.buttonGroup.Show((int)x, (int)y);
            fightStatus = FightStatus.RightMenu;
        }
    }
    void statusRightMenu()
    {
        //部分事件的触发在FightUI的函数里
        if (Input.GetMouseButtonDown(1))
        {
            FightUI.buttonGroup.Hide();
            fightStatus = FightStatus.ChooseMovePoint;
        }
    }
    void statusSkillMenu()
    {
        if (Input.GetMouseButtonDown(1))
        {
            FightUI.listbox.Hide();
            FightUI.buttonGroup.Show();
            fightStatus = FightStatus.RightMenu;
        }
    }
    void statusWinCondition()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //listbox.Hide(); 胜利条件面板隐藏
            FightUI.winCon.Hide();
            FightUI.buttonGroup.Show();
            fightStatus = FightStatus.RightMenu;
        }
    }
    void statusConfirmSkill()
    {
        if (Input.GetMouseButtonDown(1))
        {
            FightUI.listbox.Show();
            FightUI.skillConfirmGroup.Hide();
            FightUI.skillAsk.Hide();
            fightStatus = FightStatus.SkillMenu;
        }
    }
    void statusChooseAttackDirection()
    {
        ghost.statusChooseAttackDirection();

        if (Input.GetMouseButtonDown(1))
        {
            fightStatus = FightStatus.ChooseMovePoint;
            player.enterStatusChooseMovePoint();
            ghost.enterStatusChooseMovePoint();

        }
        else if (Input.GetMouseButtonDown(0))
        {
            fightStatus = FightStatus.MoveToTarget;

            ghost.enterStatusMoveToTarget();
            player.enterStatusMoveToTarget();

            for (int i = 0; i < enemys.Count; i++)
                enemys[i].enterStatusMoveToTarget();
            for (int i = 0; i < friends.Count; i++)
                friends[i].enterStatusMoveToTarget();

        }
    }
    void statusMoveToTarget()
    {
        //播放行走动画
        player.statusMoveToTarget();

        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusMoveToTarget();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusMoveToTarget();
        //应从此处判定是否进入其他状态
        bool exit = player.wait;
        for (int i = 0; i < enemys.Count; i++)
            exit = exit && enemys[i].wait;
        for (int i = 0; i < friends.Count; i++)
            exit = exit && friends[i].wait;
        if (exit)
            fightStatus = FightStatus.BrandishWeapon;
    }
    void statusBrandishWeapon()
    {
        //攻击顺序应该按照武力值排序 若同时攻击也可以        
        player.statusBrandishWeapon();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusBrandishWeapon();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusBrandishWeapon();

        //如何回到第一状态呢..动画播放完毕吗 如果所有人的act都变成idle 则进入下一状态
        int totalact = player.Act;
        if (player.Act == 5)
            totalact = 0;
        for (int i = 0; i < enemys.Count && totalact == 0; i++)
            totalact += enemys[i].act;
        for (int i = 0; i < friends.Count && totalact == 0; i++)
            totalact += friends[i].act;
        if (totalact == 0)
        {
            //进入被击中状态
            fightStatus = FightStatus.Beaten;
            player.CheckBeBeaten();
            for (int i = 0; i < enemys.Count; i++)
                enemys[i].CheckBeBeaten();
            for (int i = 0; i < friends.Count; i++)
                friends[i].CheckBeBeaten();

            for (int i = 0; i < enemys.Count; i++)
                enemys[i].enterStatusBeaten();
            player.enterStatusBeaten();
            for (int i = 0; i < friends.Count; i++)
                friends[i].enterStatusBeaten();
            /*
            player.enterStatusOne();
            ghost.enterStatusOne();
            for (int i = 0; i < enemys.Count; i++)
                enemys[i].wait = false;
                */
        }
        else
        {
            //仍在此状态
        }
    }
    void statusEnd()
    {
        interval_out -= Time.deltaTime;
        if (interval_out < 0)
        {
            ReturnToOpenning();
        }
    }
    void statusZero()
    {
        //不需要显示回合数
        //开头可以显示胜利数 失败数

        if (player.power_current <= 0)
            player.statusZero();
        else
            player.PlayAnimation();
        for (int i = 0; i < enemys.Count; i++)
            if (enemys[i].power_current <= 0)
                enemys[i].statusZero();
            else
                enemys[i].PlayAnimation();
        for (int i = 0; i < friends.Count; i++)
            if (friends[i].power_current <= 0)
                friends[i].statusZero();
            else
                friends[i].PlayAnimation();



        //检查所有的currentpower<0的enemy是否isLife 即如果出现有人currentpower<0但islife为true 则不进入状态一 否则进入状态一
        /*
        for (int i = 0; i < enemys.Count; i++)
            Debug.Log("敌人"+i+"是否生存"+enemys[i].isLife);
            */

        //问题在于 当主角死亡动画播放一半时 而敌人没有人死 goTonext为true 此时会进入阶段一
        //四种情况
        //1 主角死亡动画播放完 游戏结束
        //2 敌人数量归零 游戏结束
        //3 部分的死亡敌人动画播放完 且敌人数量大于零 进入新一回合
        //4 动画还没有播放完 继续到下一帧播放

        //goToNext 是否前往下一回合 初始值为true
        bool goToNext = true;
        for (int i = enemys.Count - 1; i >= 0; i--)
        {
            //遍历每一个敌人
            //如果存在敌人的血小于等于零 则可能不前往下一回合
            //        扣血动画还没结束 不前往下一回合 扣血动画若结束 则可能前往下一回合
            //如果所有敌人的血都大于零 则前往下一回合
            if (enemys[i].power_current <= 0)
            {
                if (enemys[i].isLife == true)
                    goToNext = false;
                else
                {
                    enemys.RemoveAt(i);
                    Debug.Log("删除了敌人" + i);
                }
            }
        }
        for (int i = friends.Count - 1; i >= 0; i--)
        {
            //遍历每一个友军
            //如果存在友军的血小于等于零 则可能不前往下一回合
            //        扣血动画还没结束 不前往下一回合 扣血动画若结束 则可能前往下一回合
            //如果所有友军的血都大于零 则前往下一回合
            if (friends[i].power_current <= 0)
            {
                if (friends[i].isLife == true)
                    goToNext = false;
                else
                {
                    friends.RemoveAt(i);
                    Debug.Log("删除了友军" + i);
                }
            }
        }
        if (player.isLife == false)
        {
            ImageCom.DrawNewPicture("fightplace/gameover", new Vector3(0, 0, (int)(ShowLayer.UI)), new Vector2(0.5f, 0.5f), ref win_pic);
            fightStatus = FightStatus.End;
            FightUI.playerCard.Hide();
            interval_out = 2;
        }
        else if (enemys.Count == 0)
        {
            ImageCom.DrawNewPicture("fightplace/win", new Vector3(0, 0, (int)ShowLayer.UI), new Vector2(0.5f, 0.5f), ref win_pic);
            win_pic.gobj.transform.localScale = new Vector3(2, 2, 1);
            fightStatus = FightStatus.End;
            FightUI.playerCard.Hide();
            interval_out = 2;
        }
        else if (goToNext && enemys.Count > 0 && player.power_current > 0)
        {
            fightStatus = FightStatus.ChooseMovePoint;
            player.enterStatusChooseMovePoint();
            ghost.enterStatusChooseMovePoint();
            for (int i = 0; i < enemys.Count; i++)
                enemys[i].enterStatusChooseMovePoint();
        }


    }
    void statusBeaten()
    {
        //播放被砍动画以及血减少的动画
        player.statusBeaten();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusBeaten();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusBeaten();
        int totalact = player.act;
        for (int i = 0; i < enemys.Count && totalact == 0; i++)
            totalact += enemys[i].act;
        for (int i = 0; i < friends.Count && totalact == 0; i++)
            totalact += friends[i].act;
        if (totalact == 0)
        {
            player.power_current = player.blood.gobj.transform.localScale.x * player.power_full;
            bool hasAIPlayerDie = false;
            for (int i = 0; i < enemys.Count; i++)
            {
                enemys[i].power_current = enemys[i].blood.gobj.transform.localScale.x * enemys[i].power_full;
                if (enemys[i].power_current <= 0)
                {
                    hasAIPlayerDie = true;
                    enemys[i].enterStatusZero();
                }
            }
            for (int i = 0; i < friends.Count; i++)
            {
                friends[i].power_current = friends[i].blood.gobj.transform.localScale.x * friends[i].power_full;
                if (friends[i].power_current <= 0)
                {
                    hasAIPlayerDie = true;
                    friends[i].enterStatusZero();
                }
            }


            //如果有人死亡 则进入零状态 否则直接进入一状态

            Debug.Log("是否有AI血条低于0" + hasAIPlayerDie);


            if (player.power_current <= 0 || hasAIPlayerDie)
            {
                fightStatus = FightStatus.Zero;
                if (player.power_current <= 0)
                {
                    player.enterStatusZero();
                }
                interval_out = 3 * intervalTime_out;
            }
            else
            {
                //进入第一状态
                fightStatus = FightStatus.ChooseMovePoint;
                player.enterStatusChooseMovePoint();
                ghost.enterStatusChooseMovePoint();
                for (int i = 0; i < enemys.Count; i++)
                    enemys[i].enterStatusChooseMovePoint();
                for (int i = 0; i < friends.Count; i++)
                    friends[i].enterStatusChooseMovePoint();
            }
        }
    }
    void statusDefend()
    {
        //播放行走动画
        player.statusDefend();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusDefend();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusDefend();
        Debug.Log(player.frame_index + " " + player.frame_total);
        if (player.frame_index == player.frame_total - 1)
        {

            fightStatus = FightStatus.Skill_MoveAfterSkill;
            player.enterStatusSkill_MoveAfterSkill();
            //ghost.enterStatusChooseMovePoint();
            for (int i = 0; i < enemys.Count; i++)
                enemys[i].enterStatusSkill_MoveAfterSkill();
            for (int i = 0; i < friends.Count; i++)
                friends[i].enterStatusSkill_MoveAfterSkill();

        }
    }
    void statusSkill_Charge()
    {
        player.statusSkill_Charge();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].PlayAnimation();
        for (int i = 0; i < friends.Count; i++)
            friends[i].PlayAnimation();
        bool over = player.frame_total - 1 == player.frame_index;
        // Debug.Log(player.act + " " + over + " " + player.frame_total + " " + player.frame_index);
        for (int i = 0; i < specialEffectList.Count; i++)
            over = over && (specialEffectList[i].frame_index == specialEffectList[i].frame_total - 1);
        if (over)
        {
            Debug.Log("进入释放阶段");
            fightStatus = FightStatus.Skill_Release;
            for (int i = 0; i < specialEffectList.Count; i++)
                specialEffectList[i].Hide();
            player.enterStatusSkill_Release();
            time_skill_charge = 0;
        }
    }
    public static float time_skill_charge = 0;
    void statusSkill_Release()
    {
        time_skill_charge += Time.deltaTime;
        player.statusSkill_Release();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].PlayAnimation();
        for (int i = 0; i < friends.Count; i++)
            friends[i].PlayAnimation();
        bool over = player.frame_total - 1 == player.frame_index;

        //Debug.Log(player.act + " "+  over +" "+  player.frame_total + " " + player.frame_index);
        for (int i = 0; i < specialEffectList.Count; i++)
        {
            bool action_end = (specialEffectList[i].movelength_cum >= specialEffectList[i].distance);
            bool ani_end = (specialEffectList[i].frame_index == specialEffectList[i].frame_total - 1);

            over = over && ani_end && action_end;
            if (over == false)
                break;
        }
        if (time_skill_charge > 2)
            over = true;

        if (over)
        {
            Debug.Log("进入扣血阶段");
            fightStatus = FightStatus.Skill_ReduceBlood;
            player.statusSkill_Release_After();
            player.enterStatusSkill_ReduceBlood();
            player.CheckBeBeatenBySkill();
            for (int i = 0; i < enemys.Count; i++)
            {
                enemys[i].enterStatusSkill_ReduceBlood();
            }
            for (int i = 0; i < friends.Count; i++)
            {
                friends[i].enterStatusSkill_ReduceBlood();
            }
            for (int i = 0; i < specialEffectList.Count; i++)
                specialEffectList[i].Hide();

        }

    }
    void statusSkill_ReduceBlood()
    {
        //播放被砍动画以及血减少的动画       
        player.statusSkill_ReduceBlood();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusSkill_ReduceBlood();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusSkill_ReduceBlood();
        int totalact = player.act;
        for (int i = 0; i < enemys.Count && totalact == 0; i++)
            totalact += enemys[i].act;
        for (int i = 0; i < friends.Count && totalact == 0; i++)
            totalact += friends[i].act;
        if (totalact == 0)
        {

            player.power_current = player.blood.gobj.transform.localScale.x * player.power_full;
            bool hasAIPlayerDie = false;
            for (int i = 0; i < enemys.Count; i++)
            {
                enemys[i].power_current = enemys[i].blood.gobj.transform.localScale.x * enemys[i].power_full;
                if (enemys[i].power_current <= 0)
                {
                    hasAIPlayerDie = true;
                    enemys[i].enterStatusSkill_Zero();
                }
            }
            for (int i = 0; i < friends.Count; i++)
            {
                friends[i].power_current = friends[i].blood.gobj.transform.localScale.x * friends[i].power_full;
                if (friends[i].power_current <= 0)
                {
                    hasAIPlayerDie = true;
                    friends[i].enterStatusSkill_Zero();
                }
            }
            //如果有人死亡 则进入零状态 否则直接进入一状态

            //背景换回正常背景
            ImageCom.ChangePicture("fightplace/bg", ref backdrop);
            Debug.Log("是否有AI血条低于0" + hasAIPlayerDie);

            if (player.power_current <= 0 || hasAIPlayerDie)
            {
                fightStatus = FightStatus.Skill_Zero;
                if (player.power_current <= 0)
                {
                    player.enterStatusSkill_Zero();
                }
                interval_out = 3 * intervalTime_out;
            }
            else
            {
                //进入第一状态
                fightStatus = FightStatus.Skill_MoveAfterSkill;
                player.enterStatusSkill_MoveAfterSkill();
                //ghost.enterStatusChooseMovePoint();
                for (int i = 0; i < enemys.Count; i++)
                    enemys[i].enterStatusSkill_MoveAfterSkill();
                for (int i = 0; i < friends.Count; i++)
                    friends[i].enterStatusSkill_MoveAfterSkill();

            }



        }
    }
    void statusSkill_Zero()
    {
        //不需要显示回合数
        //开头可以显示胜利数 失败数

        if (player.power_current <= 0)
            player.statusZero();
        else
            player.PlayAnimation();
        for (int i = 0; i < enemys.Count; i++)
            if (enemys[i].power_current <= 0)
                enemys[i].statusZero();
            else
                enemys[i].PlayAnimation();
        for (int i = 0; i < friends.Count; i++)
            if (friends[i].power_current <= 0)
                friends[i].statusZero();
            else
                friends[i].PlayAnimation();


        //检查所有的currentpower<0的enemy是否isLife 即如果出现有人currentpower<0但islife为true 则不进入状态一 否则进入状态一
        /*
        for (int i = 0; i < enemys.Count; i++)
            Debug.Log("敌人"+i+"是否生存"+enemys[i].isLife);
            */

        //问题在于 当主角死亡动画播放一半时 而敌人没有人死 goTonext为true 此时会进入阶段一
        //四种情况
        //1 主角死亡动画播放完 游戏结束
        //2 敌人数量归零 游戏结束
        //3 部分的死亡敌人动画播放完 且敌人数量大于零 进入新一回合
        //4 动画还没有播放完 继续到下一帧播放

        //goToNext 是否前往下一回合 初始值为true
        bool goToNext = true;
        for (int i = enemys.Count - 1; i >= 0; i--)
        {
            //遍历每一个敌人
            //如果存在敌人的血小于等于零 则可能不前往下一回合
            //        扣血动画还没结束 不前往下一回合 扣血动画若结束 则可能前往下一回合
            //如果所有敌人的血都大于零 则前往下一回合
            if (enemys[i].power_current <= 0)
            {
                if (enemys[i].isLife == true)
                    goToNext = false;
                else
                {
                    enemys.RemoveAt(i);
                    Debug.Log("删除了敌人" + i);
                }
            }
        }
        for (int i = friends.Count - 1; i >= 0; i--)
        {

            if (friends[i].power_current <= 0)
            {
                if (friends[i].isLife == true)
                    goToNext = false;
                else
                {
                    friends.RemoveAt(i);
                    Debug.Log("删除了友军" + i);
                }
            }
        }
        if (player.isLife == false)
        {
            ImageCom.DrawNewPicture("fightplace/gameover", new Vector3(0, 0, (int)(ShowLayer.UI)), new Vector2(0.5f, 0.5f), ref win_pic);
            fightStatus = FightStatus.End;
            FightUI.playerCard.Hide();
            interval_out = 2;

        }
        else if (enemys.Count == 0)
        {
            ImageCom.DrawNewPicture("fightplace/win", new Vector3(0, 0, (int)ShowLayer.UI), new Vector2(0.5f, 0.5f), ref win_pic);
            win_pic.gobj.transform.localScale = new Vector3(2, 2, 1);
            fightStatus = FightStatus.End;
            FightUI.playerCard.Hide();
            interval_out = 2;
        }
        else if (goToNext && enemys.Count > 0 && player.power_current > 0)
        {

            //此时不应该进入下一回合 而是应该进入移动阶段
            fightStatus = FightStatus.Skill_MoveAfterSkill;
            player.enterStatusSkill_MoveAfterSkill();
            //ghost.enterStatusChooseMovePoint();
            for (int i = 0; i < enemys.Count; i++)
                enemys[i].enterStatusSkill_MoveAfterSkill();
            for (int i = 0; i < friends.Count; i++)
                friends[i].enterStatusSkill_MoveAfterSkill();
        }

    }
    public void statusSkill_MoveAfterSkill()
    {
        player.statusSkill_MoveAfterSkill();
        for (int i = 0; i < enemys.Count; i++)
            enemys[i].statusSkill_MoveAfterSkill();
        for (int i = 0; i < friends.Count; i++)
            friends[i].statusSkill_MoveAfterSkill();
        //应从此处判定是否进入其他状态
        bool exit = true;
        for (int i = 0; i < enemys.Count; i++)
            exit = exit && enemys[i].wait;
        for (int i = 0; i < friends.Count; i++)
            exit = exit && friends[i].wait;
        if (exit)
            fightStatus = FightStatus.BrandishWeapon;
        //此时要保证 玩家的攻击目标清空 玩家当前所受伤害归零 敌人当前所受伤害归零
    }
    public class FightUI
    {
        public static ButtonGroup buttonGroup;
        public static ListBox listbox;
        public static ButtonGroup skillConfirmGroup;
        public static PicAndText skillAsk;
        public static PicAndText winCon;
        public static PicAndText playerCard;
        public static PicAndText opening;
        public static PicAndText options;
        public static int status = 1;
        public static void StartUI()
        {
            TaikouGUI2D.SetCanvas();
            TaikouGUI2D.SetCanvas2();

            options = new PicAndText(ref TaikouGUI2D.canvas_gobj2, "fightplace/战斗选项底板", 200, 200);            
            options.SetOptions();
            options.Hide();

            opening = new PicAndText(ref TaikouGUI2D.canvas_gobj, "FightOpening/Back_1", 0, 0);
            opening.SetOpening();
            buttonGroup = new ButtonGroup(400, 400, 120, 38, 1);
            buttonGroup.Hide();
            skillConfirmGroup = new ButtonGroup(100, 100, 80, 30, 2);
            skillConfirmGroup.Hide();
            skillAsk = new PicAndText(ref TaikouGUI2D.canvas_gobj2, "fightplace/询问秘技底板", 305, 125);
            skillAsk.Hide();
            skillAsk.SetSkillAskText();
            winCon = new PicAndText(ref TaikouGUI2D.canvas_gobj2, "fightplace/胜利条件底板", 300, 200);
            winCon.Hide();
            winCon.SetWinConText();
            winCon.AddButton();
            playerCard = new PicAndText(ref TaikouGUI2D.canvas_gobj2, "fightplace/个人战名片", 0, 490);
            playerCard.Hide();
            playerCard.SetPlayerHeadCard();
            //创建一个listview
            listbox = new ListBox(ref TaikouGUI2D.canvas_gobj2, 500, 350, 350, 20);
           // listbox.Hide();
            listbox.AddButton(Skill.Zhiyu);
            listbox.AddButton(Skill.Fenshen);
            listbox.AddButton(Skill.Kongchan);
            listbox.AddButton(Skill.Renquan);
            listbox.AddButton(Skill.Yueying);
            listbox.AddButton(Skill.Zhuan);
           
            /*
            listbox.AddButton(Skill.Renquan);
            
            
                 
           */
            listbox.Hide();
            buttonGroup.Hide();
            skillConfirmGroup.Hide();
            skillAsk.Hide();
            winCon.Hide();
            playerCard.Hide();
        }
        public static void ButtonCancel()
        {
            Debug.Log("停止");
            buttonGroup.Hide();
            fightStatus = FightStatus.ChooseMovePoint;
        }
        public static void ButtonDefend()
        {
            Debug.Log("防御");
            buttonGroup.Hide();
            fightStatus = FightStatus.Defend;
            ghost.Hide();
            player.enterStatusDefend();
            for (int i = 0; i < enemys.Count; i++)
                enemys[i].enterStatusDefend();
        }
        public static void ButtonSkill()
        {
            Debug.Log("秘技");
            buttonGroup.Hide();
            listbox.Show();
            fightStatus = FightStatus.SkillMenu;
        }
        public static void ButtonWinCon()
        {
            Debug.Log("胜利条件");
            winCon.Show();
            buttonGroup.Hide();
            fightStatus = FightStatus.WinCondition;
        }
        public static void ButtonSkillYes()
        {
            Debug.Log("确认释放秘技");
            fightStatus = FightStatus.Skill_Charge;
            skillConfirmGroup.Hide();
            skillAsk.Hide();
            playerCard.ChangeQili(4);
            player.enterStatusSkill_Charge();
        }
        public static void ButtonSkillNo()
        {
            Debug.Log("取消释放秘技");
            listbox.Show();
            skillConfirmGroup.Hide();
            skillAsk.Hide();
            fightStatus = FightStatus.SkillMenu;
        }


    }
    public class PicAndText
    {
        GameObject gobj;
        Image pic;
        Texture2D texture;
        GameObject text_gobj;
        Text text;
        List<Text> textList;
        List<GameObject> qiliList;
        Texture2D tiliTexture;
        GameObject dropdown_fri;
        GameObject dropdown_ene;

        public PicAndText(ref GameObject canvas_gobj, string path, int left, int bottom)
        {
            textList = new List<Text>();
            gobj = new GameObject("PicAndText", typeof(Image));

            pic = gobj.GetComponent<Image>();
            texture = (Texture2D)Resources.Load(path);
            pic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            gobj.transform.parent = canvas_gobj.transform;
            TaikouGUI2D.SetRectTransform_leftbottom(gobj, left, bottom, texture.width, texture.height);
           // gobj.SetActive(false);
        }
        public void AddText(string str, int left, int bottom, int width, int height, TextAnchor textAnchor)
        {
            GameObject text_gobj = new GameObject("Text", typeof(Text));
            Text text = text_gobj.GetComponent<Text>();
            text.text = str;
            text.color = new Color(0, 0, 0);
            text.font = TaikouGUI2D.huawen;
            text.fontSize = 16;
            text.alignment = textAnchor;
            text_gobj.transform.parent = gobj.transform;
            TaikouGUI2D.SetRectTransform_leftbottom(text_gobj, left, bottom, width, height);
            textList.Add(text);
        }
        public void SetOpening()
        {
            TaikouButton start = new TaikouButton(ref gobj, "", 350, 380, 100, 30, "开始战斗", TaikouGUI2D.huawen);
            TaikouButton fightoption = new TaikouButton(ref gobj, "", 350, 340, 100, 30, "战斗选项", TaikouGUI2D.huawen);
            TaikouButton exit = new TaikouButton(ref gobj, "", 350, 300, 100, 30, "离开游戏", TaikouGUI2D.huawen);
            start.button_gobj.transform.parent = gobj.transform;
            fightoption.button_gobj.transform.parent = gobj.transform;
            exit.button_gobj.transform.parent = gobj.transform;
            start.button_btn.onClick.AddListener(delegate () { EnterFightPlace(); });
            fightoption.button_btn.onClick.AddListener(delegate () { FightUI.options.Show(); });
            exit.button_btn.onClick.AddListener(delegate () { Debug.Log("结束游戏"); Application.Quit(); });
        }
        public void SetOptions()
        {
            dropdown_fri = Instantiate(Resources.Load("dropdown") as GameObject);
            dropdown_ene = Instantiate(Resources.Load("dropdown") as GameObject);
            dropdown_fri.transform.parent = gobj.transform;
            dropdown_ene.transform.parent = gobj.transform;
            TaikouGUI2D.SetRectTransform_leftbottom(dropdown_fri, 52, 65, 120, 30);
            TaikouGUI2D.SetRectTransform_leftbottom(dropdown_ene, 217, 65, 120, 30);
            TaikouButton close = new TaikouButton(ref gobj, "fightplace/确认", 160, 58, 65, 35, "", TaikouGUI2D.huawen);
            close.button_btn.onClick.AddListener(delegate ()
            {
                FriendsInitNum = dropdown_fri.GetComponent<Dropdown>().value + 1;
                EnemysInitNum = dropdown_ene.GetComponent<Dropdown>().value + 1;
                Hide();
            });
            

        }
        public void AddButton()
        {
            TaikouButton close = new TaikouButton(ref gobj, "fightplace/关闭", 247, 55, 65, 35, "", TaikouGUI2D.huawen);
            close.button_btn.onClick.AddListener(delegate ()
            {
                Hide();
                FightUI.buttonGroup.Show();
                FightUI.status = 2;
            });
        }
        public void AddPhoto(string path, int left, int bottom)
        {
            GameObject head_photo_gobj = new GameObject("headphoto", typeof(Image));
            Texture2D head_photo_tex = (Texture2D)Resources.Load(path);
            head_photo_gobj.GetComponent<Image>().sprite = Sprite.Create(head_photo_tex, new Rect(0, 0, head_photo_tex.width, head_photo_tex.height), Vector2.zero);
            head_photo_gobj.transform.parent = gobj.transform;
            TaikouGUI2D.SetRectTransform_leftbottom(head_photo_gobj, left, bottom, head_photo_tex.width, head_photo_tex.height);
            qiliList.Add(head_photo_gobj);
        }
        public void SetSkillAskText()
        {
            AddText("要使用浮舟吗？", 0, 0, texture.width, texture.height, TextAnchor.MiddleCenter);
        }
        public void SetWinConText()
        {
            AddText("击败所有敌人。", 21, 90, 210, 18, TextAnchor.MiddleLeft);
            AddText("吕归尘被击倒。", 21, 42, 210, 18, TextAnchor.MiddleLeft);
        }
        public void SetPlayerHeadCard()
        {
            qiliList = new List<GameObject>();
            tiliTexture = Resources.Load("fightplace/左上角蓝条") as Texture2D;
            AddPhoto("face/10-1", 16, 15);
            AddText("吕归尘", 120, 80, 135, 18, TextAnchor.MiddleLeft);
            AddText("影月", 158, 7, 135, 18, TextAnchor.MiddleLeft);

            for (int i = 0; i < 8; i++)
                AddPhoto("fightplace/气力点", 162 + 16 * i, 35);
            AddPhoto("fightplace/左上角蓝条", 163, 59);
            ChangeQili(0);
            gobj.SetActive(false);
        }
        public void ChangeBlood(int bloodpercent)
        {
            qiliList[qiliList.Count - 1].GetComponent<RectTransform>().sizeDelta = new Vector2(tiliTexture.width * bloodpercent / 100.0f, tiliTexture.height);
        }
        public void ChangeQili(int qili)
        {

            for (int i = 1; i <= 8; i++)
            {
                if (i <= qili)
                    qiliList[i].SetActive(true);
                else
                    qiliList[i].SetActive(false);
            }
        }
        public void Show()
        {
            gobj.SetActive(true);            
        }

        public void Show(string words)
        {
            gobj.SetActive(true);
            textList[0].text = words;
        }
        public void Hide()
        {
            gobj.SetActive(false);
        }
    }
    public class TaikouButton
    {

        Texture2D pic_texture;
        public GameObject button_gobj;
        GameObject text_gobj;
        Image button_img;
        public Button button_btn;
        Text button_text;
        RectTransform button_rt;
        RectTransform text_rt;
        public int qili_consume;
        public TaikouButton(ref GameObject canvas_gobj, string pic_path, int left, int up, int width, int height, string text_in, Font font)
        {

            float cameraWidth = Camera.main.pixelWidth;
            float cameraHeight = Camera.main.pixelHeight;
            button_gobj = new GameObject("按钮", typeof(Button));
            button_gobj.transform.parent = canvas_gobj.transform;
            button_btn = button_gobj.GetComponent<Button>();
            pic_texture = (Texture2D)Resources.Load(pic_path);

            if (pic_texture != null)
            {
                button_img = button_gobj.AddComponent<Image>();
                button_img.sprite = Sprite.Create(pic_texture, new Rect(0, 0, pic_texture.width, pic_texture.height), Vector2.zero);
                button_btn.targetGraphic = button_img;
            }


            TaikouGUI2D.SetRectTransform_leftup(button_gobj, left, up, width, height);
            text_gobj = new GameObject("Text", typeof(Text));
            button_text = text_gobj.GetComponent<Text>();
            button_text.text = text_in;
            text_gobj.transform.parent = button_gobj.transform;
            button_text.font = font;
            button_text.color = new Color(0, 0, 0);
            button_text.fontSize = 20;
            TaikouGUI2D.SetRectTransform_leftbottom(text_gobj, 0, 0, width, height);
            button_text.alignment = TextAnchor.MiddleCenter;
        }
        public TaikouButton(ref GameObject content, string pic_path, int width, int itemlength, int itemnum, string text_in, int qili_con, Font font, UnityAction call)
        {

            float cameraWidth = Camera.main.pixelWidth;
            float cameraHeight = Camera.main.pixelHeight;
            qili_consume = qili_con;
            pic_texture = (Texture2D)Resources.Load(pic_path);
            button_gobj = new GameObject("按钮", typeof(Button));
            button_img = button_gobj.AddComponent<Image>();
            button_btn = button_gobj.GetComponent<Button>();
            button_img.sprite = Sprite.Create(pic_texture, new Rect(0, 0, 100, 30), Vector2.zero);
            button_btn.targetGraphic = button_img;
            button_gobj.transform.parent = content.transform;
            int x = 0;
            int y = -itemlength * itemnum;

            RectTransform rt = button_gobj.AddComponent<RectTransform>();
            if (rt == null)
                rt = button_gobj.GetComponent<RectTransform>();
            rt.localScale = new Vector3(1, 1, 1);
            rt.sizeDelta = new Vector2(width, itemlength);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, y);

            text_gobj = new GameObject("Text", typeof(Text));
            button_text = text_gobj.GetComponent<Text>();
            button_text.text = text_in;
            text_gobj.transform.parent = button_gobj.transform;
            button_text.font = font;
            button_text.color = new Color(0, 0, 0);
            button_text.fontSize = 16;
            TaikouGUI2D.SetRectTransform_leftbottom(text_gobj, 0, 0, width, itemlength);
            button_text.alignment = TextAnchor.MiddleLeft;
            button_btn.onClick.AddListener(call);
        }
    }

    public class ButtonGroup
    {
        public TaikouButton cancel;
        public TaikouButton defend;
        public TaikouButton skill;
        public TaikouButton wincondition;
        public GameObject canvas_gobj;
        public TaikouButton yes;
        public TaikouButton no;
        Canvas canvas_component;
        Font huawen;
        int back_height;
        int back_width;
        public void Hide()
        {
            buttongroup_back_gobj.SetActive(false);
        }
        public void Show()
        {
            buttongroup_back_gobj.SetActive(true);
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            Show((int)x, (int)y);

        }
        public void Show(int x, int y)
        {
            buttongroup_back_gobj.SetActive(true);
            if (y < back_height)
                y = back_height;
            if (x > Camera.main.pixelWidth - back_width)
                x = Camera.main.pixelWidth - back_width;
            buttongroup_back_gobj.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
        GameObject buttongroup_back_gobj;
        Image buttongroup_back_img;
        Texture2D img;
        public ButtonGroup(int x, int y, int width, int height, int type)
        {
            if (type == 1)
            {
                int y_offset = 4;
                GetCanvas();
                SetButtonBack(x, y, width, 170, "fightplace/右键底板_1");
                back_height = img.height;
                back_width = img.width;
                huawen = TaikouGUI2D.huawen;
                cancel = new TaikouButton(ref buttongroup_back_gobj, "fightplace/停止", 0, back_height - y_offset + 1, width, height, "", huawen);
                defend = new TaikouButton(ref buttongroup_back_gobj, "fightplace/防御", 0, back_height - height - y_offset * 2, width, height, "", huawen);
                skill = new TaikouButton(ref buttongroup_back_gobj, "fightplace/秘技", 0, back_height - height * 2 - y_offset * 3, width, height, "", huawen);
                wincondition = new TaikouButton(ref buttongroup_back_gobj, "fightplace/胜利条件", 0, back_height - height * 3 - y_offset * 4, width, height, "", huawen);
                cancel.button_btn.onClick.AddListener(delegate () { FightUI.ButtonCancel(); });
                defend.button_btn.onClick.AddListener(delegate () { FightUI.ButtonDefend(); });
                skill.button_btn.onClick.AddListener(delegate () { FightUI.ButtonSkill(); });
                wincondition.button_btn.onClick.AddListener(delegate () { FightUI.ButtonWinCon(); });
            }
            else if (type == 2)
            {
                int y_offset = 10;
                GetCanvas();
                SetButtonBack(x, y, 120, 90, "fightplace/确认秘技底板");
                back_height = img.height;
                back_width = img.width;
                huawen = TaikouGUI2D.huawen;
                yes = new TaikouButton(ref buttongroup_back_gobj, "fightplace/确认秘技是", 20, back_height - y_offset + 1, width, height, "", huawen);
                no = new TaikouButton(ref buttongroup_back_gobj, "fightplace/确认秘技否", 20, back_height - height - y_offset * 2, width, height, "", huawen);
                yes.button_btn.onClick.AddListener(delegate () { FightUI.ButtonSkillYes(); });
                no.button_btn.onClick.AddListener(delegate () { FightUI.ButtonSkillNo(); });
            }

        }


        public void GetCanvas()
        {
            canvas_gobj = TaikouGUI2D.canvas_gobj;
            canvas_component = TaikouGUI2D.canvas_component;
        }
        public void SetButtonBack(int x, int y, int width, int height, string path)
        {
            img = Resources.Load(path) as Texture2D;
            buttongroup_back_gobj = new GameObject("buttonbackimg", typeof(Image));
            buttongroup_back_img = buttongroup_back_gobj.GetComponent<Image>();
            buttongroup_back_img.sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0, 1));
            buttongroup_back_gobj.transform.parent = canvas_gobj.transform;
            RectTransform back_rt = buttongroup_back_gobj.GetComponent<RectTransform>();
            back_rt.localScale = new Vector3(1, 1, 1);
            back_rt.sizeDelta = new Vector2(width, height);
            //button_rt.localPosition = new Vector3(left, up, 0);
            back_rt.anchorMin = new Vector2(0, 0);
            back_rt.anchorMax = new Vector2(0, 0);
            back_rt.pivot = new Vector2(0, 1);
            back_rt.anchoredPosition = new Vector2(x, y);
        }
    }
    public class TaikouGUI2D
    {
        public static GameObject canvas_gobj;
        public static Canvas canvas_component;
        public static GameObject canvas_gobj2;
        public static Canvas canvas_component2;
        public static Font huawen = (Font)Resources.Load("华文新魏");
        public static void SetRectTransform_First(GameObject gobj, int width, int height)
        {
            RectTransform rt = gobj.GetComponent<RectTransform>();
            if (rt == null)
                rt = gobj.AddComponent<RectTransform>();
            rt.localScale = new Vector3(1, 1, 1);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
        }
        public static void SetRectTransform_leftup(GameObject gobj, int left, int up, int width, int height)
        {

            SetRectTransform_First(gobj, width, height);
            RectTransform rt = gobj.GetComponent<RectTransform>();
            if (rt == null)
                rt = gobj.AddComponent<RectTransform>();
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(left, up);
        }
        public static void SetRectTransform_leftbottom(GameObject gobj, int left, int bottom, int width, int height)
        {

            SetRectTransform_First(gobj, width, height);
            RectTransform rt = gobj.GetComponent<RectTransform>();
            if (rt == null)
                rt = gobj.AddComponent<RectTransform>();
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(left, bottom);
        }
        public static void SetCanvas()
        {
            canvas_gobj = new GameObject("acancas", typeof(Canvas));
            canvas_component = canvas_gobj.GetComponent<Canvas>();
            canvas_component.renderMode = RenderMode.ScreenSpaceCamera;
            canvas_component.worldCamera = Camera.main;
            canvas_gobj.AddComponent<CanvasScaler>();
            canvas_gobj.AddComponent<GraphicRaycaster>();
            canvas_component.sortingOrder = 1;
            huawen = (Font)Resources.Load("华文新魏");
           // canvas_gobj.SetActive(false);
        }
        public static void SetCanvas2()
        {
            canvas_gobj2 = new GameObject("acancas2", typeof(Canvas));
            canvas_component2 = canvas_gobj2.GetComponent<Canvas>();
            canvas_component2.renderMode = RenderMode.ScreenSpaceCamera;
            canvas_component2.worldCamera = Camera.main;
            canvas_gobj2.AddComponent<CanvasScaler>();
            canvas_gobj2.AddComponent<GraphicRaycaster>();
            canvas_component2.sortingOrder = 2;
            //huawen = (Font)Resources.Load("华文新魏");
            // canvas_gobj.SetActive(false);
        }
    }
    public class ListBox
    {
        GameObject scrollView;
        GameObject viewPort;
        GameObject content;
        int width;
        int height;
        int maxlength;
        int itemlength;
        int itemnum = 0;
        GameObject scrollView_gobj;
        Image scrollView_back_img;
        Texture2D scrollView_texture;

        TaikouButton cancel;
        public List<TaikouButton> skillList_btn;
        public void SetScrollViewBack(ref GameObject canvas_gobj, int left, int bottom, int width, int height)
        {

            scrollView_gobj = new GameObject("buttonbackimg", typeof(Image));
            scrollView_back_img = scrollView_gobj.GetComponent<Image>();
            scrollView_back_img.sprite = Sprite.Create(scrollView_texture, new Rect(0, 0, scrollView_texture.width, scrollView_texture.height), new Vector2(0, 0));
            scrollView_back_img.transform.parent = canvas_gobj.transform;
            TaikouGUI2D.SetRectTransform_leftbottom(scrollView_gobj, left, bottom, width, height);
            scrollView_gobj.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, 75);
        }
        public ListBox(ref GameObject canvas_gobj, int contentwidth, int contentheight, int contentmaxlength, int itemheight)
        {
            skillList_btn = new List<TaikouButton>();
            scrollView_texture = Resources.Load("fightplace/秘技底板_1") as Texture2D;
            SetScrollViewBack(ref canvas_gobj, 0, 0, scrollView_texture.width, scrollView_texture.height);
            width = contentwidth;
            height = contentheight;
            maxlength = contentmaxlength;
            itemlength = itemheight;
            itemnum = 0;
            scrollView = Instantiate(Resources.Load("scrollview") as GameObject);
            scrollView.transform.parent = scrollView_gobj.transform;
            TaikouGUI2D.SetRectTransform_leftbottom(scrollView, 10, 50, width, height);
            viewPort = scrollView.transform.Find("Viewport").gameObject;
            content = viewPort.transform.Find("Content").gameObject;
            TaikouGUI2D.SetRectTransform_leftbottom(content, 0, 0, width, maxlength);
            content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentheight - contentmaxlength - 20);

            //增加停止按钮
            cancel = new TaikouButton(ref scrollView_gobj, "fightplace/秘技底板_停止", 235, 43, 50, 35, "", TaikouGUI2D.huawen);
            cancel.button_btn.onClick.AddListener(delegate ()
            {
                Hide();
                FightUI.buttonGroup.Show();                
            });
           // Hide();
        }
        public void Hide()
        {
            scrollView_gobj.SetActive(false);
        }
        public void Show()
        {
            scrollView_gobj.SetActive(true);
           // scrollView.SetActive(true);
            content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height - maxlength - 20);
            //只显示满足气力要求的秘技
            
            for (int i = 0; i < skillList_btn.Count; i++)
            {
                if (skillList_btn[i].qili_consume > player.Qili)
                {
                    skillList_btn[i].button_gobj.SetActive(false);
                }
                else
                {
                    skillList_btn[i].button_gobj.SetActive(true);
                }
            }
            scrollView_gobj.SetActive(true);
            
        }

        public void AddButton(Skill skill)
        {
            TaikouButton newbtn = new TaikouButton(ref content, "diban/51-1", width, itemlength, itemnum, 
                SkillUse.GetSkill(skill).MyToString(), SkillUse.GetSkill(skill).qi_consume, TaikouGUI2D.huawen, 
                delegate () {
                    Debug.Log(SkillUse.GetSkill(skill).MyToString());
                    FightUI.listbox.Hide(); FightUI.skillConfirmGroup.Show();
                    FightUI.skillAsk.Show("要使用" + SkillUse.GetSkill(skill).name + "吗?");
                    fightStatus = FightStatus.ConfirmSkill;
                    player.currentSkill = skill; });
            //newbtn.button_gobj.SetActive(false);
            skillList_btn.Add(newbtn);
            itemnum += 1;
            //如果技能个数超过了长度 则延伸长度
            if (itemlength * itemnum > maxlength)
            {
                maxlength = itemlength * itemnum;
                content.GetComponent<RectTransform>().sizeDelta = new Vector2(width, maxlength);
            }

        }
    }
}
