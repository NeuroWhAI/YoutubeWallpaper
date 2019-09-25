using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YoutubeWallpaper
{
    public class Option
    {
        public enum Type
        {
            OneVideo,
            Playlist,
        }

        public enum Job
        {
            Nothing,
            Mute,
            Toggle,
        }

        //#############################################################################################
        
        public Type IdType
        { get; set; } = Type.OneVideo;

        public string Id
        { get; set; } = "";

        /// <summary>
        /// 소리 크기
        /// (0~100)
        /// </summary>
        public int Volume
        { get; set; } = 100;

        /// <summary>
        /// 모니터 번호
        /// (0~n)
        /// </summary>
        public int ScreenIndex
        { get; set; } = 0;

        /// <summary>
        /// 실시간 영상인가?
        /// ※ 더이상 사용되지 않습니다.
        /// </summary>
        public bool IsLive
        { get; set; } = false;

        public Job JobWhenOverlayed
        { get; set; } = Job.Nothing;

        //#############################################################################################

        public void SaveToFile(string filename)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Create)))
            {
                bw.Write((int)IdType);
                bw.Write(Id);
                bw.Write(Volume);
                bw.Write(ScreenIndex);
                bw.Write(IsLive);
                bw.Write((int)JobWhenOverlayed);


                bw.Close();
            }
        }

        public void LoadFromFile(string filename)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                try
                {
                    IdType = (Type)br.ReadInt32();
                    Id = br.ReadString();
                    Volume = br.ReadInt32();
                    ScreenIndex = br.ReadInt32();
                    IsLive = br.ReadBoolean();
                    JobWhenOverlayed = (Job)br.ReadInt32();
                }
                catch (EndOfStreamException)
                {
                    // NOTE: 파일이 구버전용임.
                }
                finally
                {
                    br.Close();
                }
            }
        }
    }
}
