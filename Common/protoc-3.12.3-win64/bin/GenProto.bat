protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 PAUSE

START ../../../CA_Server/PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../CA_Client/Assets/Scripts/Packet"
XCOPY /Y Protocol.cs "../../../CA_Server/Server/Packet"
XCOPY /Y ClientPacketManager.cs "../../../CA_Client/Assets/Scripts/Packet"
XCOPY /Y ServerPacketManager.cs "../../../CA_Server/Server/Packet"