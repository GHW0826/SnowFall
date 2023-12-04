using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator;

internal class PacketFormat
{

    // {0} : 패킷 이름/번호 목록
    // {1} : 패킷 목록
    public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TCPServerCore;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
   ushort Protocol {{ get; }}
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}}

{1}
";
    // {0} : 패킷 이름
    // {1} : 패킷 번호
    public static string packetEnumFormat =
@"{0} = {1},";

    // {0} : 패킷 이름
    // {1} : 멤버 변수
    // {2} : 멤버 변수 Read
    // {3} : 멤버 변수 Write
    public static string packetFormat =
@"
public class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}
  

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        // openSegment
        var segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), Protocol);
        count += sizeof(ushort);
        {3}

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }}
}}

";

    // {0} : 변수 형식
    // {1} : 변수 이름
    public static string memberFormat =
@"public {0} {1};";


    // {0} : 리스트 이름 [대문자]
    // {1} : 리스트 이름 [소문자]
    // {2} : 멤버 변수
    // {3} : 멤버 변수 Read
    // {4} : 멤버 변수 Write
    public static string memberListFormat =
@"
public struct {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true;
        {4}

        return success;
    }}
    public {0}()
    {{}}
}}
public List<{0}> {1}s = new();
";

    // {0} : 변수 이름
    // {1} ; To 변수 형식
    // {2} : 변수 형식
    public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

    // {0} : 변수 이름
    // {1} : 변수 형식
    public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";

    // {0} : 변수 이름
    public static string readStringFormat =
@"ushort {0}Len = (ushort)BitConverter.ToInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.name = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

    // {0} : 리스트 이름 [대문자]
    // {1} : 리스트 이름 [소문자]
    public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}
";

    // {0} : 변수 이름
    // {1} : 변수 형식
    public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

    // {0} : 변수 이름
    // {1} : 변수 형식
    public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

    // {0} : 변수 이름
    public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(
   this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort)
  );
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

    // {0} : 리스트 이름 [대문자]
    // {1} : 리스트 이름 [소문자]
    public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach (var {1} in {1}s)
{{
    // TODO
    {1}.Write(s, ref count);
}}
";
}

