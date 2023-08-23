using croquis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

/*
        CC0 1.0 Universal made by DayDreamSound
        https://youtu.be/kfnh9QAfDgA

     */
/// <summary>
/// 
/// </summary>
/// 


public class Alarm
{

    MediaPlayer player;
    public Alarm()
    {
        byte[] fileByte = Resource1.Alarm;
        using (Stream stream = new MemoryStream())
        {
            NAudio.Wave.Mp3FileReader reader = new NAudio.Wave.Mp3FileReader(stream);
            var waveOut = new WaveOut();
            waveOut.Init(reader);
            waveOut.Play();
        }
    }


    public void play()
    {
        player.Play();
    }

    public void Stop()
    {
        player?.Stop();
    }
}