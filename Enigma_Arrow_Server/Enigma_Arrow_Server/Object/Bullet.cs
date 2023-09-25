using Google.Protobuf.Protocol;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bullet : GameObject
{
    public ClientSession Session { get; set; }
    
    
    public int OwnerId;
    int damage = 10;
    public Bullet()
    {
        _objectType = ObjectType.Bullet;
        Speed = 100;
        _collisionRadius = 1f;
    }

    public Vec _moveDir = new Vec() { X = 0, Y = 0, Z = 0 };

    long _nextMoveTick = 0;

    public override void Update()
    {
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

        if (_moveDir.X != 0 || _moveDir.Y != 0 || _moveDir.Z != 0)
            JoinedRoom.Broadcast(res);
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

        if(Pos.Z > 90 || Pos.Z < -130)
        {
            // TODO: 제거
            JoinedRoom.Push(JoinedRoom.LeaveGame, Id);
        }

        List<GameObject> objects = new List<GameObject>();
        List<Player> players = JoinedRoom.Players;

        foreach (Player p in players)
            objects.Add(p);


        OverlapCheck(objects);
    }

    void OverlapCheck(List<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            float dotDistance = MathF.Sqrt(MathF.Abs(obj.Pos.X - Pos.X) + MathF.Abs(obj.Pos.Y - Pos.Y) + MathF.Abs(obj.Pos.Z - Pos.Z));
            if (obj.CollisionRadius + CollisionRadius >= dotDistance)
            {
                //TODO: 피격
                if (OwnerId != obj.Id)
                {
                    obj.OnDamage(damage, this);
                    OnDamage(damage, this);
                    break;
                }
            }
        }
    }
    public override void OnDamage(int damage, GameObject attacker)
    {
        OnDead();
    }

    public override void OnDead()
    {
        JoinedRoom.Push(JoinedRoom.LeaveGame, Id);
    }
}