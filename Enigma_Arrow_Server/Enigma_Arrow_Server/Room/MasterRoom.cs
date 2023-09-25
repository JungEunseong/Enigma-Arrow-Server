using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MasterRoom : JobSerializer
{
    public static MasterRoom Instance { get; set; } = new MasterRoom();
    
    public List<ClientSession> _sessions = new List<ClientSession>();
    public List<ClientSession> _matchingSessions= new List<ClientSession>();


    public void Update()
    {
        Push(Match);

        Flush();
    }


    public void Enter(ClientSession session)
    {
        if (session == null)
        {
            Console.WriteLine("MasterRoom Enter Failed");
            return;
        }

        _sessions.Add(session);

    }


    public void Leave(ClientSession session)
    {
        if(session == null) return;

        if (_sessions.Remove(session))
        {
            return;
        }
        Console.WriteLine("MasterRoom Leave Failed");
    }

    public void HandleMatching(ClientSession session,bool isCancel)
    {
        if (session == null)  return;
        if(isCancel == true)
        {
            if (_matchingSessions.Contains(session))
                _matchingSessions.Remove(session);

            return;
        }
        else _matchingSessions.Add(session);

    }

    public void Match()
    {
        if (_matchingSessions.Count < 2) return;

        // TODO: room 하나 만들고 넣어주고 연결 끊기
        // TODO: 서로의 ip를 건내주고 연결끊기
        ClientSession firstSession = _matchingSessions[0];
        ClientSession secondSession = _matchingSessions[1];

        _matchingSessions.Remove(firstSession);
        _matchingSessions.Remove(secondSession);

        GameRoom room = RoomManager.Instance.Add();
        room.EnterRoom(firstSession);
        room.EnterRoom(secondSession);
        // 알리기
        S_MatchingRes matchingRes = new S_MatchingRes();
        matchingRes.Users.Add(firstSession.UInfo);
        matchingRes.Users.Add(secondSession.UInfo);

        matchingRes.MineIdx = 0;
        firstSession.Send(matchingRes);

        matchingRes.MineIdx = 1;
        secondSession.Send(matchingRes);

    }
}
