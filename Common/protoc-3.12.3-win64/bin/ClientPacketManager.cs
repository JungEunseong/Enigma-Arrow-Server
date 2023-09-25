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
		_onRecv.Add((ushort)MsgId.SMatchingRes, MakePacket<S_MatchingRes>);
		_handler.Add((ushort)MsgId.SMatchingRes, PacketHandler.S_MatchingResHandler);		
		_onRecv.Add((ushort)MsgId.SSpawnRes, MakePacket<S_SpawnRes>);
		_handler.Add((ushort)MsgId.SSpawnRes, PacketHandler.S_SpawnResHandler);		
		_onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMoveRes, MakePacket<S_MoveRes>);
		_handler.Add((ushort)MsgId.SMoveRes, PacketHandler.S_MoveResHandler);		
		_onRecv.Add((ushort)MsgId.SAttackRes, MakePacket<S_AttackRes>);
		_handler.Add((ushort)MsgId.SAttackRes, PacketHandler.S_AttackResHandler);		
		_onRecv.Add((ushort)MsgId.SSetHp, MakePacket<S_SetHp>);
		_handler.Add((ushort)MsgId.SSetHp, PacketHandler.S_SetHpHandler);		
		_onRecv.Add((ushort)MsgId.SSetOutcome, MakePacket<S_SetOutcome>);
		_handler.Add((ushort)MsgId.SSetOutcome, PacketHandler.S_SetOutcomeHandler);
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