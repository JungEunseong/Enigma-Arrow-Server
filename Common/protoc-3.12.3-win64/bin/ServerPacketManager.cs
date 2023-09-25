using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CMatchingReq, MakePacket<C_MatchingReq>);
		_handler.Add((ushort)MsgId.CMatchingReq, PacketHandler.C_MatchingReqHandler);		
		_onRecv.Add((ushort)MsgId.CSpawnplayerReq, MakePacket<C_SpawnplayerReq>);
		_handler.Add((ushort)MsgId.CSpawnplayerReq, PacketHandler.C_SpawnplayerReqHandler);		
		_onRecv.Add((ushort)MsgId.CMoveReq, MakePacket<C_MoveReq>);
		_handler.Add((ushort)MsgId.CMoveReq, PacketHandler.C_MoveReqHandler);		
		_onRecv.Add((ushort)MsgId.CTryAttack, MakePacket<C_TryAttack>);
		_handler.Add((ushort)MsgId.CTryAttack, PacketHandler.C_TryAttackHandler);		
		_onRecv.Add((ushort)MsgId.CAttackReq, MakePacket<C_AttackReq>);
		_handler.Add((ushort)MsgId.CAttackReq, PacketHandler.C_AttackReqHandler);		
		_onRecv.Add((ushort)MsgId.CLeaveRoom, MakePacket<C_LeaveRoom>);
		_handler.Add((ushort)MsgId.CLeaveRoom, PacketHandler.C_LeaveRoomHandler);		
		_onRecv.Add((ushort)MsgId.CSetUserinfo, MakePacket<C_SetUserinfo>);
		_handler.Add((ushort)MsgId.CSetUserinfo, PacketHandler.C_SetUserinfoHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}