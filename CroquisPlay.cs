using croquis;
using Microsoft.VisualBasic;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


public class CroquisPlay
{
    
    //리턴 받을 객체 
    public Button PlayButton { get; set; }

    public delegate void Sleep(int seconds);
    public delegate void Show(ImageTreeViewItem item);
    public delegate void Stop();
    public delegate void Finish();
    public delegate void Delay(int seconds);

    public WaitCallback play { get; set; }
    
    public Sleep sleep { get; set; }
    public Show show { get; set; }

    public PlayStatus Status { get; set; } = PlayStatus.Stop;
    
    public int Interval { get; set; }
    public int RefreshInterval {get; set; }
    public int Page { get; set; } = 0;


    public CroquisPlay(Sleep sleep, Show show)
    {
        this.sleep = sleep;
        this.show = show; 
    }

    
   
    public async Task<bool> Run(object o)
    {
        if(show == null) { return false; }
        if ((this.Interval == 0) || (this.RefreshInterval == 0)) return false;
        
        
        if(this.Status == PlayStatus.Stop)
        {
            this.Status = PlayStatus.Play;

            return await PrivateRun(o);

        } else
        {
            return false;
        }
        
    }




    private async Task<bool> PrivateRun(object o)
    {
        return await Application.Current.Dispatcher.Invoke(async () =>
        {
            List<ImageTreeViewItem> CroquisList = o as List<ImageTreeViewItem>;
            if (CroquisList == null)
                return false;

            int croquisListCont = CroquisList.Count;

            if (this.Page == 0)
                this.Page = CroquisList.Count;

            for (int i = 0; i < this.Page; i++)
            {
                if (this.Status == PlayStatus.Stop)
                    break;

                ImageTreeViewItem item = CroquisList[RandomRun(croquisListCont)];
                show(item);
                await DelayPlayer(Interval);
                show(null);
                await delay(RefreshInterval);
                if (this.Status == PlayStatus.Skip)
                {
                    this.Status = PlayStatus.Play;
                }
            }
            return true;
        });
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    private int RandomRun(int max)
    {
        Random random = new Random();

        return random.Next(0, max);
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="second"></param>
    //public void DelayPlayer(int second)
    //{
    //    // 시작시간 
    //    DateTime now = DateTime.Now;

    //    //시작 ~ 종료 간격 
    //    TimeSpan duration = new TimeSpan(0, 0, second);

    //    //종료시간 
    //    DateTime end = now.Add(duration);

    //    while (end >= now)
    //    {
    //        if (this.Status == PlayStatus.Stop || this.Status == PlayStatus.Skip)
    //        {
    //            return;
    //        }

    //        //현재시간 체크 
    //        now = DateTime.Now;
    //        if (now.Equals(end))
    //        {
    //            ThreadPool.QueueUserWorkItem(playMusic);
    //        }

    //    }
    //}

    public async Task DelayPlayer(int second)
    {
        // 시작시간 
        DateTime now = DateTime.Now;

        //시작 ~ 종료 간격 
        TimeSpan duration = new TimeSpan(0, 0, second);

        //종료시간 
        DateTime end = now.Add(duration);

        await Task.Run(() =>
        {
            while (end >= now)
            {
                if (this.Status == PlayStatus.Stop || this.Status == PlayStatus.Skip)
                {
                    return;
                }

                //현재시간 체크 
                now = DateTime.Now;
                if (now >= end)
                {
                    ThreadPool.QueueUserWorkItem(playMusic);
                    break;  // 반복문 종료
                }
            }
        });
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public async void playMusic(object? state)
    {
        byte[] fileByte = Resource1.Alarm;
        Stream stream = new MemoryStream(fileByte);
        NAudio.Wave.Mp3FileReader reader = new NAudio.Wave.Mp3FileReader(stream);
        var waveOut = new WaveOut();
        waveOut.Init(reader);
        await Task.Factory.StartNew(() => {
            
            waveOut.Play(); 
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="second"></param>
    public async Task delay(int second)
    {
        if(this.Status == PlayStatus.Play)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(second * 1000); // seconds to milliseconds
            });
        }

        
    }

    /// <summary>
    /// 
    /// </summary>
    public void stop()
    {
        if(this.Status == PlayStatus.Stop) 
        {
            return;
        }

        if(this.Status == PlayStatus.Play)
        {
            this.Status = PlayStatus.Stop;  
        }
    }

    public void Skip()
    {
        this.Status = PlayStatus.Skip;
    }
    
    

}

