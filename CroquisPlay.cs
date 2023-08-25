using croquis;
using Microsoft.VisualBasic;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class CroquisPlay
{
    

    public delegate void Sleep(int milliseconds);
    public delegate void Show(string path);
    public delegate void Stop();
    public delegate void Finish();
    public delegate void Delay(int milliseconds);

    public WaitCallback play { get; set; }
    
    public Sleep sleep { get; set; }
    public Show show { get; set; }
    
    


    private PlayStatus status = PlayStatus.Play;
    
    public int Interval { get; set; }
    public int RefreshInterval {get; set; }
    public int count { get; set; } = 0;


    public CroquisPlay(Sleep sleep, Show show)
    {
        this.sleep = sleep;
        this.show = show; 
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    public void run(object o)
    {
        if(show == null) { return; }
        if ((this.Interval == 0) || (this.RefreshInterval == 0)) return;

        ThreadPool.QueueUserWorkItem(PrivateRun, o);
    }


    public void DelayPlayer(int second)
    {
        
        DateTime now = DateTime.Now;
        TimeSpan duration = new TimeSpan(0, 0, second);
        TimeSpan alramCount = new TimeSpan(0, 0, second - 3);

        DateTime alramTime = now.Add(alramCount);
        DateTime end = now.Add(duration);

        while (end >= now)
        {
            now = DateTime.Now;
            if (now.Equals(end))
            {
                ThreadPool.QueueUserWorkItem(playMusic);
            }
            if (status == PlayStatus.Stop)
            {
                return;
            }
        }
        
    }

   
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


    public void delay(int second)
    {
        DateTime now = DateTime.Now;
        TimeSpan duration = new TimeSpan(0, 0, second);
        DateTime end = now.Add(duration);

        while(end >= now)
        {
            now = DateTime.Now;
            if (status == PlayStatus.Stop)
            {
                return;
            }
        }
    }

    public void stop()
    {
        if(status == PlayStatus.Stop) 
        {
            return;
        }

        if(status == PlayStatus.Play)
        {
            status = PlayStatus.Stop;  
        }
    }


    private int randomRun(int max)
    {
        Random random = new Random();

       return random.Next(0,max);
    }

    private void PrivateRun(object o)
    {
        List<string> path = o as List<string>;
        if(path == null) { return; };

        int c = this.count;
        //int i = 0;
        if(c == 0) 
        {
            c = path.Count;
        }

        for(int i = 0;i < c;i++)
        {
            if(status == PlayStatus.Stop)
            {
                status = PlayStatus.Play;
                return;
            }
            string s= path[randomRun(path.Count)];

            show(s);
            DelayPlayer(Interval);
            show(null);
            delay(RefreshInterval);
            
        }
    }
    

}

