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


    private void PrivateRun(object o)
    {
        List<string> path = o as List<string>;
        if(path == null) { return; };

        int c = this.count;

        if(c == 0) 
        {
            c = path.Count;
        }
        
        for(int i = 0; i < c; i++)
        {
            if(status == PlayStatus.Stop)
            {
                Debug.WriteLine("종료");
                status = PlayStatus.Play;
                break;
            }

            show(path[i]);
            sleep(Interval);
            show(null);
            sleep(RefreshInterval);
        }

    }
    

}

