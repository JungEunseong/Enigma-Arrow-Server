using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class GameRoom : JobSerializer
{
    public int RoomId { get; set; }
    public ObjectManager _objectManager { get; set; } = new ObjectManager();

    Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    Dictionary<int, Player> _players = new Dictionary<int, Player>();
    Dictionary<int, Bullet> _bullets = new Dictionary<int, Bullet>();

    public List<Player> Players{
        get
        {
            return _players.Values.ToList();
        }
    }

    public int PlayerCount { get { return _players.Count; } }
    /// <summary>
    /// 매칭 후 게임 방에 들어 올 때 실행하는 함수
    /// </summary>
    /// <param name="session">들어오는 세션</param>
    public void EnterRoom(ClientSession session)
    {
        _sessions.Add(session.SessionId, session);
        session.JoinedRoom = this;
    }

    /// <summary>
    /// 게임 입장 후 게임 시작 시 플레이어 스폰할 때 사용하는 함수
    /// </summary>
    public void EnterGame(GameObject gameObject)
    {
        gameObject.JoinedRoom = this;


        //TODO: 플레이어 소환
        if (gameObject.Info.Type == ObjectType.Player) {
            Player spawnPlayer = gameObject as Player;

            if (spawnPlayer.isTopPosition)
                gameObject.Info.Position = new Vec() { X = 0, Y = 1, Z = 0 };
            else
                gameObject.Info.Position = new Vec() { X = 0, Y = 1, Z = -33 };

            {
                _players.Add(spawnPlayer.Id, spawnPlayer);
                spawnPlayer.Session.MyPlayer = spawnPlayer;
                S_SpawnRes res = new S_SpawnRes();

                ObjectInfo info = spawnPlayer.Info;
                info.IsMine = true;

                res.Objects.Add(info);
                
                spawnPlayer.Session.Send(res);

                info.IsMine = false;
            }

            //TODO: 원래 있던 플레이어 소환
            {
                S_SpawnRes res = new S_SpawnRes();
                
                foreach (Player player in _players.Values)
                    if(player != spawnPlayer) res.Objects.Add(player.Info);
                
                spawnPlayer.Session.Send(res);
            }
        }
        else if(gameObject.Info.Type == ObjectType.Bullet)
        {
            _bullets.Add(gameObject.Id, gameObject as Bullet);
            gameObject.JoinedRoom = this;
        }

        //TODO: 원래 있던 플레이어에게 플레이어 소환
        {
            S_SpawnRes res = new S_SpawnRes();
            res.Objects.Add(gameObject.Info);

            foreach (Player player in _players.Values)
                if (player.Id != gameObject.Id) player.Session.Send(res);
        }

    }

    public void LeaveGame(int gameObjectId)
    {
        GameObject obj = _objectManager.FindById(gameObjectId);

        if (obj == null) return;

        S_Despawn despawn = new S_Despawn();
        despawn.ObjectId.Add(obj.Id);

        Broadcast(despawn);

        if (obj._objectType == ObjectType.Player)
            _players.Remove(gameObjectId);
        else if(obj._objectType == ObjectType.Bullet)
            _bullets.Remove(gameObjectId);

        
    }

    public void HandleMove(ClientSession session,C_MoveReq req)
    {
        session.MyPlayer._moveDir = req.InputDir;
    }
    public void HandleTryAttack(ClientSession session)
    {
        S_AttackRes res = new S_AttackRes();
        res.ObjectId = session.MyPlayer.Id;
        res.CanAttack = session.MyPlayer.CanAttack;

        Broadcast(res);

        if(session.MyPlayer.CanAttack)
            session.MyPlayer.AttackTickUpdate(100000); // Attack Tick 갱신은 Bulletㅇ르 생성할 때 됨.
                                                       // 하지만 Bullet생성은 애니메이션이 실행될 때 0.6초 뒤에 일어남
                                                       // 애니메이션이 실행될 때 한번더 Try Attack이 들어온다면 CanAttack은 True로 나올 것
                                                       // 그래서 AttackTick에 100000 밀리세컨드를 더해줌.
                                                       // 어짜피 Bullet을 소환할 때 200밀리세컨드 뒤로 AttackTick을 갱신해주기 때문!
                                                       // 추가)
                                                       // 하지만 여러번 Handle이 실행 되었을 때 JobQueue로 인해 나중에 갱신하는 것을 생각못함. => 여러번 누르면 한번 공격 후 공격이 안됨
                                                       // 그래서 CanAttack일 때만 갱신할 수 있도록 수정하였음!    
    }
    public void HandleAttack(ClientSession session,C_AttackReq req)
    {
        session.MyPlayer.Attack(req);
    }

    public void ExitRoom(ClientSession session)
    {
        _sessions.Remove(session.SessionId);
        MasterRoom.Instance.Enter(session);
        session.MyPlayer.Session = null;
        session.MyPlayer = null;
        session.JoinedRoom = null;

        if (_sessions.Count == 0) RoomManager.Instance.Remove(RoomId);
    }

    public void Broadcast(IMessage packet)
    {
        foreach (ClientSession session in _sessions.Values)
            session.Send(packet);
    }

    public void Update()
    {
        foreach (Player player in _players.Values)
            player.Update();
        foreach (Bullet bullet in _bullets.Values) 
            bullet.Update();

        Flush();
    }
}
