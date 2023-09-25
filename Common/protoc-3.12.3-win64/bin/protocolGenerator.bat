protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 PAUSE

START ../../../Enigma_Arrow_Server/ProtocolGenerator/bin/Debug/net6.0/ProtocolGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../Enigma_Arrow_Server/Enigma_Arrow_Server/Packet"
XCOPY /Y ServerPacketManager.cs "../../../Enigma_Arrow_Server/Enigma_Arrow_Server/Packet"
XCOPY /Y Protocol.cs "../../../../Enigma_Arrow_Client/Assets/Scripts/Networking/Packet"
XCOPY /Y ClientPacketManager.cs  "../../../../Enigma_Arrow_Client/Assets/Scripts/Networking/Packet"