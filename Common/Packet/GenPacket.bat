START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/PDL.XML
XCOPY /y GenPacket.cs "../../TCPDummyClient/Packet"
XCOPY /y GenPacket.cs "../../TCPServerCore/Packet"