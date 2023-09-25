using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class GameObject
{
    public ObjectType _objectType { get { return Info.Type; } protected set { Info.Type = value; } }
    public int Id
    {
        get { return Info.Id; }
        set { Info.Id = value; }
    }
    
    public GameRoom JoinedRoom { get; set; }

    public ObjectInfo Info { get; set; } = new ObjectInfo();

    public Vec Pos { get { return Info.Position; } }

    private float _speed = 10;

    public float Speed { get { return _speed; } set { _speed = value; }  }

    protected float _collisionRadius;

    public float CollisionRadius { get { return _collisionRadius; } }
    public abstract void Update();
    public abstract void MoveUpdate();
    public abstract void Move(Vec dir);


    public abstract void OnDamage(int damage, GameObject attacker);

    public abstract void OnDead();
}
