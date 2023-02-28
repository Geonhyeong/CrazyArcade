pushd %~dp0
protoc.exe -I=./ --cpp_out=./ --csharp_out=./ ./Protocol.proto

GenPackets.exe --path=./Protocol.proto --output=ClientPacketHandler --recv=C_ --send=S_
GenPackets.exe --path=./Protocol.proto --output=ServerPacketHandler --recv=S_ --send=C_

IF ERRORLEVEL 1 PAUSE

XCOPY /Y Protocol.pb.h "../../../CA_CppServer/GameServer"
XCOPY /Y Protocol.pb.cc "../../../CA_CppServer/GameServer"
XCOPY /Y ClientPacketHandler.h "../../../CA_CppServer/GameServer"

XCOPY /Y Protocol.pb.h "../../../CA_CppServer/DummyClient"
XCOPY /Y Protocol.pb.cc "../../../CA_CppServer/DummyClient"
XCOPY /Y ServerPacketHandler.h "../../../CA_CppServer/DummyClient"

START ../../../CA_Server/PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../CA_Client/Assets/Scripts/Packet"
XCOPY /Y Protocol.cs "../../../CA_Server/Server/Packet"
XCOPY /Y ClientPacketManager.cs "../../../CA_Client/Assets/Scripts/Packet"
XCOPY /Y ServerPacketManager.cs "../../../CA_Server/Server/Packet"

DEL /Q /F *.pb.h
DEL /Q /F *.pb.cc
DEL /Q /F *.h
DEL /Q /F *.cs

PAUSE
