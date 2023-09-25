using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

public class Player : GameObject
{
    public ClientSession Session { get; set; }
    public Player()
    {
        _objectType = ObjectType.Player;
        _collisionRadius = 0.6f;
    }
    public bool isTopPosition { get; set; }
    int _hp = 30;

    public Vec _moveDir = new Vec() { X = 0, Y = 0, Z = 0 };



    long _nextAttackTick = 0;

    public bool CanAttack
    {
        get { return _nextAttackTick <= Environment.TickCount64; }
    }
    public void AttackTickUpdate(int tick)
    {
        _nextAttackTick = Environment.TickCount64 + tick;
    }
    public void Attack(C_AttackReq req)
    {
        AttackTickUpdate(200);

        Bullet bullet = JoinedRoom._objectManager.Add<Bullet>();
        bullet.OwnerId = Id;
        bullet.Info.Position = req.Position;
        bullet.Info.Rotate = req.Rotation;
        bullet.Info.Type = ObjectType.Bullet;
        bullet._moveDir = req.Dir;
        JoinedRoom.Push(JoinedRoom.EnterGame,bullet);
    }

    long _nextMoveTick = 0;

    public override void Update()
    {
        if (Session == null) return;
        MoveUpdate();
    }

    public override void MoveUpdate()
    {
        if (_nextMoveTick > Environment.TickCount64)
            return;

        int moveTick = (int)(1000 / Speed);
        _nextMoveTick = Environment.TickCount64 + moveTick;

        Move(_moveDir);

        S_MoveRes res = new S_MoveRes();
        res.Id = Id;
        res.Position = Pos;

        if(_moveDir.X != 0 || _moveDir.Y != 0 || _moveDir.Z != 0)
            if(Session.JoinedRoom != null) Session.JoinedRoom.Broadcast(res);
    }
    public override void Move(Vec dir)
    {
        // Vector 크기 구하기
        float vectorSize = MathF.Sqrt(MathF.Pow(dir.X, 2) + MathF.Pow(dir.Y, 2) + MathF.Pow(dir.Z, 2));

        if (vectorSize == 0) return;
        // Vector 정규화
        dir.X /= vectorSize;
        dir.Y /= vectorSize;
        dir.Z /= vectorSize;

        // 이동
        Pos.X += dir.X;
        Pos.Y += dir.Y;
        Pos.Z += dir.Z;

        Pos.X = Math.Clamp(Pos.X, -40, 20);
    }



    public override void OnDamage(int damage, GameObject attacker)
    {
        _hp -= damage;

        S_SetHp setHp = new S_SetHp();
        setHp.Id = Id;
        setHp.Hp = _hp;

        JoinedRoom.Push(JoinedRoom.Broadcast,setHp);

        if (_hp <= 0)
            JoinedRoom.Push(OnDead);
    }

    public override void OnDead()
    {
        // TODO: 승/패 UI 띄우기
        S_SetOutcome outcome = new S_SetOutcome();
        outcome.IsWin = false;
        Session.Send(outcome);

        outcome.IsWin = true;
        List<Player> players = JoinedRoom.Players;

        foreach(Player p in players)
            if(p.Id != Id) p.Session.Send(outcome);
            
    }
}