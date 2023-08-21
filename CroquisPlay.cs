using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


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

    public void delay(int second)
    {
        DateTime now = DateTime.Now;
        TimeSpan duration = new TimeSpan(0, 0, 0, 0, second);

        DateTime dateTimeAdd = now.Add(duration);

        while(dateTimeAdd >= now)
        {
            now = DateTime.Now;
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

            Debug.WriteLine(s);
            show(s);
            //sleep(Interval);
            delay(Interval);
            show(null);
            //sleep(RefreshInterval);
            delay(RefreshInterval);
            
        }
    }
    

}

