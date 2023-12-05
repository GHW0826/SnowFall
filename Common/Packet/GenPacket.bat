START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/PDL.XML
XCOPY /Y GenPackets.cs "../../TCPDummyClient/Packet/"
XCOPY /Y GenPackets.cs "../../TCPServerExample/Packet/"

XCOPY /Y ClientPacketManager.cs "../../TCPDummyClient/Packet/"
XCOPY /Y ServerPacketManager.cs "../../TCPServerExample/Packet/"