/*

Copyright 2010 (C) Peter Gill <peter@majorsilence.com>

This file is part of LibMPlayerCommon.

LibMPlayerCommon is free software; you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

LibMPlayerCommon is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

/**
 * added some new functions
 * */

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;
using ErrorReport;

namespace LibMPlayerCommon
{

    /// <summary>
    /// The seek type that is used when seeking a new position in the video stream.
    /// </summary>
    /// 
    
    public enum Seek
    {
        Relative = 0,
        Percentage = 1,
        Absolute=2
    }

    /// <summary>
    /// Status of the mplayer.exe instance.
    /// </summary>
    public enum MediaStatus
    {
        Paused,
        Playing,
        Stopped
    }

    /// <summary>
    /// The video output backend that mplayer is using.
    /// </summary>
    public enum MplayerBackends
    {
        OpenGL,
		GL, // Simple Version
		GL2, // Simple Version.  Variant of the OpenGL  video  output  driver.   
			// Supports  videos larger  than  the maximum texture size but lacks 
			// many of the ad‐vanced features and optimizations of the gl driver  
			// and  is  un‐likely to be extended further.
        Direct3D, // Windows
		DirectX, // Windows
		X11, // Linux
		VESA,
		Quartz, // Mac OS X
		CoreVideo, // Mac OS X
        SDL, // Cross Platform
		Vdpau, // Linux
		ASCII, // ASCII art video output driver that works on a text console.
		ColorASCII, // Color  ASCII  art  video output driver that works on a text console.
    	Directfb, // Linux.  Play video using the DirectFB library.
		Wii, // Linux.  Nintendo Wii/GameCube specific video output driver.
		V4l2, // Linux.   requires Linux 2.6.22+ kernel,  Video output driver for 
			// V4L2 compliant cards with built-in hardware MPEG decoder.
	
	}
    public class MPlayer
    {
        private int _wid;
        private string _font;
        private int _fontautoscale;
        private float _textscale; 
        private string _subcp;
        private int _subpos;
        private bool _fullscreen;
        private int _mplayerProcessID=-1;
        private MplayerBackends _mplayerBackend;
        private int _currentPosition = 0; // Current position in seconds in stream.
        private int _totalTime = 0; // The total length that the video is in seconds.
        private string currentFilePath;
        private string _mplayerPath="";
        private string _cache;
        private string _percentpos;
        private string _getfileinfofilename;
        private string _getinfotitle;
        private string _finalfilecode;
        private string _scanning;
        private string _filesub;
        private string _audiochannel;
        private string _setaudiolang;
        public int volumemain;
        private BackendPrograms _backendProgram;
        public event MplayerEventHandler VideoExited;

        /// <summary>
        /// This event is the most accurate way to get the current position of the current playing file.
        /// Whenever the postion changes this event will notify that the positon has changed with the value
        /// being the new position (seconds into the file).
        /// </summary>
        public event MplayerEventHandler CurrentPosition;
        public event MplayerEventHandler cache;
        public event MplayerEventHandler finalfile;
        public event MplayerEventHandler consola;
        public event MplayerEventHandler scanfonts;
        public event MplayerEventHandler filesub;
        public event MplayerEventHandler audiochannel;
        public event MplayerEventHandler setaudiolang;
        private System.Timers.Timer _currentPostionTimer;


        private MPlayer(){}
        public MPlayer(string font, int fontautoscale, float textscale, string subcp, int subpos, int volume, int wid, MplayerBackends backend) : this(font, fontautoscale, textscale, subcp, subpos, volume,  wid, backend, "") { }

        /// <summary>
        /// Create a new instance of mplayer class.
        /// </summary>
        /// <param name="wid">Window ID that mplayer should attach itself</param>
        /// <param name="backend">The video output backend that mplayer will use.</param>
        /// <param name="mplayerPath">The full filepath to mplayer.exe.  If mplayerPath is left empty it will search for mplayer.exe in 
        /// "current directory\backend\mplayer.exe" on windows and mplayer in the path on linux.</param>
        public MPlayer(string font, int fontautoscale, float textscale, string subcp, int subpos, int volume, int wid, MplayerBackends backend, string mplayerPath)
        {
            UnhandledExceptionManager.AddHandler();
            this._font = font;
            this._fontautoscale = fontautoscale;
            this._textscale = textscale;
            this._subcp = subcp;
            this._subpos = subpos;
            this._wid = wid;
            this.volumemain = volume;
            this._fullscreen = false;
            this.MplayerRunning = false;
            this._mplayerBackend = backend;
            this._mplayerPath = mplayerPath;
            this.CurrentStatus = MediaStatus.Stopped;

            this._backendProgram = new BackendPrograms(mplayerPath);

            MediaPlayer = new System.Diagnostics.Process();

            // This timer will send an event every second with the current video postion when video
            // is in play mode.
            this._currentPostionTimer = new System.Timers.Timer(1000);
            this._currentPostionTimer.Elapsed += new ElapsedEventHandler(_currentPostionTimer_Elapsed);
            this._currentPostionTimer.Enabled = true;

        }

        /// <summary>
        /// Cleanup resources.  Currently this means that mplayer is closed if it is still running.
        /// </summary>
        ~MPlayer()
        {
            // Cleanup

            if (this._mplayerProcessID != -1)
            {
                try
                {
                    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(this._mplayerProcessID);
                    if (p.HasExited == false)
                    {
                        p.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Instance.WriteLine(ex);
                }
            }
        }


        /// <summary>
        /// Is mplayer alreadying running?  True or False.
        /// </summary>
        public bool MplayerRunning{get; set; }

        /// <summary>
        /// The current status of the player.
        /// </summary>
        public MediaStatus CurrentStatus { get; set; }

        public bool HardwareAccelerated { get; set; }


        /// <summary>
        /// The process that is running mplayer.
        /// </summary>
        private System.Diagnostics.Process MediaPlayer { get; set; }

		private string MplayerBackend()
		{
			string backend;
            if (this._mplayerBackend == MplayerBackends.Direct3D)
            {
                backend = "direct3d";
            }
			else if (this._mplayerBackend == MplayerBackends.X11)
            {
                backend = "x11";
            }
			else if (this._mplayerBackend == MplayerBackends.Quartz)
            {
                backend = "quartz";
            }
			else if (this._mplayerBackend == MplayerBackends.CoreVideo)
            {
                backend = "corevideo";
            }
            else if (this._mplayerBackend == MplayerBackends.SDL)
            {
                backend = "sdl";
            }
			else if (this._mplayerBackend == MplayerBackends.GL)
            {
                backend = "gl";
            }
			else if (this._mplayerBackend == MplayerBackends.GL2)
            {
                backend = "gl2";
            }
			else if (this._mplayerBackend == MplayerBackends.ASCII)
            {
                backend = "aa";
            }
			else if (this._mplayerBackend == MplayerBackends.ColorASCII)
            {
                backend = "caca";
            }
			else if (this._mplayerBackend == MplayerBackends.Directfb)
            {
                backend = "directfb";
            }
			else if (this._mplayerBackend == MplayerBackends.Wii)
            {
                backend = "wii";
            }
			else if (this._mplayerBackend == MplayerBackends.V4l2)
            {
                backend = "v4l2";
            }
			else if (this._mplayerBackend == MplayerBackends.VESA)
            {
                backend = "vesa";
            }
            else
            {
                backend = "opengl";
            }	
			
			return backend;
		}

        #region load , play, seek, get percent, get time
        /// <summary>
        /// Load and start playing a video.
        /// </summary>
        /// <param name="filePath"></param>
        public void Play(string filePath)
        {
            this.currentFilePath = filePath;

            
            

            if (this.MplayerRunning)
            {
                LoadFile(filePath);
                this.CurrentStatus = MediaStatus.Playing;
                return;
            }

            this._currentPostionTimer.Start();

            MediaPlayer.StartInfo.CreateNoWindow = true;
            MediaPlayer.StartInfo.UseShellExecute = false;
            MediaPlayer.StartInfo.ErrorDialog = false;
            MediaPlayer.StartInfo.RedirectStandardOutput = true;
            MediaPlayer.StartInfo.RedirectStandardInput = true;
            MediaPlayer.StartInfo.RedirectStandardError = true;

         		
            string backend = MplayerBackend();

            MediaPlayer.StartInfo.Arguments = string.Format("-slave -quiet -idle -priority abovenormal -nodr -double -nokeepaspect -cache 8192 -nofs -autosync 100 -mc 2.0 -nomouseinput -framedrop -osdlevel 0 -lavdopts threads=4 -ao dsound -v -monitorpixelaspect 1 -ontop -font \"{0}\" -subfont-autoscale {1} -subfont-text-scale {2} -subcp {3} -subpos {4} -volume {5} -vo {6} -wid {7} \"{8}\"", this._font, this._fontautoscale, this._textscale, this._subcp, this._subpos, this.volumemain, backend, this._wid, filePath);
            MediaPlayer.StartInfo.FileName = this._backendProgram.MPlayer;

            MediaPlayer.Start();

            this.CurrentStatus = MediaStatus.Playing;

            this.MplayerRunning = true;
            this._mplayerProcessID = MediaPlayer.Id;

            //System.IO.StreamWriter mw = MediaPlayer.StandardInput;
            //mw.AutoFlush = true;

            MediaPlayer.OutputDataReceived += HandleMediaPlayerOutputDataReceived;
            MediaPlayer.ErrorDataReceived += HandleMediaPlayerErrorDataReceived;
            MediaPlayer.BeginErrorReadLine();
            MediaPlayer.BeginOutputReadLine();

            this.LoadCurrentPlayingFileLength();


        }

        /// <summary>
        /// Starts a new video/audio file immediatly.  Requires that Play has been called.
        /// </summary>
        /// <param name="filePath">string</param>
        public void LoadFile(string filePath)
        {
            string LoadCommand = @"" + string.Format("loadfile \"{0}\"", PrepareFilePath(filePath));
            MediaPlayer.StandardInput.WriteLine(LoadCommand);
            MediaPlayer.StandardInput.Flush();
            this.LoadCurrentPlayingFileLength();
            System.Threading.Thread.Sleep(1000);
            Volume(volumemain);
        }
        /// <summary>
        /// Prepare filepaths to be used witht the loadfile command.  
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks>
        /// For some reason it strips the DirectorySeperatorChar so we double it up here.
        /// </remarks>
        private string PrepareFilePath(string filePath)
        {

            string preparedPath = filePath.Replace("" + System.IO.Path.DirectorySeparatorChar, "" + System.IO.Path.DirectorySeparatorChar + System.IO.Path.DirectorySeparatorChar);

            return preparedPath;
        }
        public void Pause()
        {
            if (this.CurrentStatus != MediaStatus.Playing && this.CurrentStatus != MediaStatus.Paused)
            {
                return;
            }

            try
            {
                MediaPlayer.StandardInput.WriteLine("pause");
                MediaPlayer.StandardInput.Flush();
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(ex);
                return;
            }

            if (this.CurrentStatus == MediaStatus.Paused)
            {
                this.CurrentStatus = MediaStatus.Playing;
            }
            else
            {
                this.CurrentStatus = MediaStatus.Paused;
            }
        }

        /// <summary>
        /// Stop the current video.
        /// </summary>
        public void Stop()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("stop");
                MediaPlayer.StandardInput.Flush();
            }
            this.CurrentStatus = MediaStatus.Stopped;
        }

        /// <summary>
        /// Close MPlayer instance.
        /// </summary>
        public void Quit()
        {
            try
            {
                MediaPlayer.StandardInput.WriteLine("quit");
                MediaPlayer.StandardInput.Flush();
            }
            catch (ObjectDisposedException ex)
            {
                Logging.Instance.WriteLine(ex);
            }
        }
        public void setpercent(int v)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property percent_pos {0}", v));
            MediaPlayer.StandardInput.Flush();
        }
        public void MovePosition(int timePosition)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property time_pos {0}", timePosition));
            MediaPlayer.StandardInput.Flush();
        }
        public void Seek(int value, Seek type)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("seek {0} {1}", value, type));
            MediaPlayer.StandardInput.Flush();
        }
        public int CurrentPlayingFileLength()
        {
            return this._totalTime;
        }
        // Sets in motions events to set this._totalTime.  Is called as soon as the video starts.
        private void LoadCurrentPlayingFileLength()
        {
            // This works even with streaming.
            Discover file = new Discover(this.currentFilePath, this._backendProgram.MPlayer);
            this._totalTime = file.Length;
        }


        private void _currentPostionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.CurrentStatus == MediaStatus.Playing)
            {
                MediaPlayer.StandardInput.WriteLine("get_time_pos");
                MediaPlayer.StandardInput.Flush();
            }
        }

        /// <summary>
        /// Get the current postion in the file being played.
        /// </summary>
        /// <remarks>It is highly recommended to use the CurrentPostion event instead.</remarks>
        /// <returns></returns>
        public int GetCurrentPosition()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_time_pos");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._currentPosition;
            }
            return -1;
        }
        public string getpercentpos()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_percent_pos");
                MediaPlayer.StandardInput.Flush();
                return this._percentpos;
            }
            return "";
            this._percentpos = "";
        }
        #endregion
        /// <summary>
        /// Move to a new position in the video.
        /// </summary>
        /// <param name="timePosition">Seconds.  The position to seek move to.</param>
       
        //inserir legendas
        #region legendas
        public void inserirlegendas(string filepath)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("sub_load \"{0}\"", PrepareFilePath(filepath)));
            MediaPlayer.StandardInput.WriteLine(string.Format("sub_file 0"));
            MediaPlayer.StandardInput.Flush();
        }
        //visibilidade das legendas
        public void visibilidade(int v)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property sub_visibility {0}", v));
            MediaPlayer.StandardInput.Flush();
        }
        //posicao das legendas bottom//center//up
        public void subpos(int v)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property sub_pos {0}", v));
            MediaPlayer.StandardInput.Flush();
        }
        //tamanho da letra
        
          public void subscale(int v)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property sub_scale {0}", v));
            MediaPlayer.StandardInput.Flush();
        }
        //remover legendas
        public void removerlegendas()
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("sub_remove -1"));
            MediaPlayer.StandardInput.Flush();
        }
        //atraso das legendas
        public void subdelay(int v)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("sub_delay {0}", v));
            MediaPlayer.StandardInput.Flush();
        }
        #endregion
        //inserir a percentagem do ficheiro
      
        #region get info
        //get audio bitrate
        public string getaudiobitrate()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_audio_bitrate");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get audio codec
        public string getaudiocodec()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_audio_codec");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get audio samples
        public string getaudiosamples()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_audio_samples");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get filename
        public string getfilename()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_file_name");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get album
        public string getalbum()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_album");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get artist
        public string getartist()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_artist");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get comment
        public string getcomment()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_comment");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get comment
        public string getgenre()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_genre");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get title
        public string gettitle()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_title");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getinfotitle;
            }
            return "";
        }
        //get meta track
        public string gettrack()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_track");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get year
        public string getyear()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_meta_year");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get videobitrate
        public string getvideobitrate()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_video_bitrate");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        //get video resolution
        public string getvideoresolution()
        {
            if (this.CurrentStatus != MediaStatus.Stopped)
            {
                MediaPlayer.StandardInput.WriteLine("get_video_resolution");
                MediaPlayer.StandardInput.Flush();

                // This is to give the HandleMediaPlayerOutputDataReceived enought time to process and set the currentPosition.
                System.Threading.Thread.Sleep(100);
                return this._getfileinfofilename;
            }
            return "";
        }
        #endregion
        /// <summary>
        /// Seek a new postion.
        /// Seek to some place in the movie.
        /// Seek.Relative is a relative seek of +/- value seconds (default).
        /// Seek.Percentage is a seek to value % in the movie.
        /// Seek.Absolute is a seek to an absolute position of value seconds.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        

        public void SetSize(int width, int height)
        {
            if (this.CurrentStatus != MediaStatus.Playing)
            {
                return;
            }
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property width {0}", width));
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property height {0}", height));
            MediaPlayer.StandardInput.Flush();
        }
        public void aspect_ratio(string s)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("switch_ratio {0}", s));
            MediaPlayer.StandardInput.Flush();
        }
               /// <summary>
        /// Pause the current video.  If paused it will unpause.
        /// </summary>
        


        /// <summary>
        /// Retrieves the number of seconds of the current playing video.
        /// </summary>
       
        
        /// <summary>
        /// Get if the video is full is screen or not.  Set video to play in fullscreen.
        /// </summary>
        public bool FullScreen
        {
            get { return _fullscreen; }
            set 
            {
                _fullscreen = value;
                MediaPlayer.StandardInput.WriteLine(string.Format("set_property fullscreen {0}", Convert.ToInt32(_fullscreen)  ));
                MediaPlayer.StandardInput.Flush();
            }
        }

        /// <summary>
        /// Toggle Fullscreen.
        /// </summary>
        public void ToggleFullScreen()
        {
            if (this.MplayerRunning)
            {
                MediaPlayer.StandardInput.WriteLine("vo_fullscreen");
                MediaPlayer.StandardInput.Flush();
            }
        }

        #region volume
        /// <summary>
        /// Toggle Mute.  
        /// </summary>
        public void Mute()
        {
            MediaPlayer.StandardInput.WriteLine("mute");
            MediaPlayer.StandardInput.Flush();
        }


        /// <summary>
        /// Accepts a volume value of 0 - 100.
        /// </summary>
        /// <param name="volume"></param>
        public void Volume(int volume)
        {
            Debug.Assert(volume >= 0 && volume <= 100);
            try
            {

                MediaPlayer.StandardInput.WriteLine(string.Format("set_property volume {0}", volume));
                MediaPlayer.StandardInput.Flush();
                volumemain = volume;
            }
            catch
            {
            }
            
        }
        public void changeaudio(int v)
        {
            MediaPlayer.StandardInput.WriteLine(string.Format("set_property switch_audio {0}", v));
            MediaPlayer.StandardInput.Flush();
        }
        #endregion
        /// <summary>
        /// All mplayer standard output is read through this function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMediaPlayerOutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                
                string line = e.Data.ToString();
                
                this.consola(this, new MplayerEvent(line));
                if (line.StartsWith("EOF code:")) 
                {
                    this._finalfilecode = line.Substring("EOF code:".Length);
                    if (this._finalfilecode != null)
                    {
                        this.finalfile(this, new MplayerEvent(this._finalfilecode));
                    }
                }
                else if (line.Contains("Scanning file") || line.Contains("get_path"))
                {
                    this._scanning = line;
                    if (this._scanning != null)
                    {
                        this.scanfonts(this, new MplayerEvent(this._scanning));
                    }
                }
                else if (line.StartsWith("ID_FILE_SUB_FILENAME="))
                {
                    this._filesub = line.Substring("ID_FILE_SUB_FILENAME=".Length);
                    if (this._filesub != null)
                    {
                        this.filesub(this, new MplayerEvent(this._filesub));
                    }
                }
                else if (line.StartsWith("ID_AID_"))
                {
                    this._audiochannel = line.Substring("ID_AID_".Length);
                    if (this._audiochannel != null)
                    {
                        this.audiochannel(this, new MplayerEvent(this._audiochannel));
                    }
                }

                else if (line.StartsWith("ID_SID_"))
                {
                    this._setaudiolang = line.Substring("ID_SID_".Length);
                    if (this._setaudiolang != null)
                    {
                        this.setaudiolang(this, new MplayerEvent(this._setaudiolang));
                    }
                }

                else if (line.StartsWith("ANS_PERCENT_POSITION="))
                {
                    this._percentpos = line.Substring("ANS_PERCENT_POSITION=".Length);
                }
                else if (line.StartsWith("ANS_TIME_POSITION="))
                {
                    this._currentPosition = (int)float.Parse(line.Substring("ANS_TIME_POSITION=".Length));

                    if (this.CurrentPosition != null)
                    {

                        this.CurrentPosition(this, new MplayerEvent(this._currentPosition));
                    }
                }
                else if (line.StartsWith("ANS_length="))
                {
                    this._totalTime = (int)float.Parse(line.Substring("ANS_length=".Length));
                }
                else if (line.StartsWith("Exiting"))
                {
                    if (this.VideoExited != null)
                    {
                        this.VideoExited(this, new MplayerEvent("Exiting File"));
                    }
                }
                else if (line.StartsWith("Cache fill:"))
                {
                    this._cache = line.Substring("Cache fill:".Length);

                    if (this._cache != null)
                    {
                        this.cache(this, new MplayerEvent(this._cache));
                    }
                }
                else if (line.StartsWith("ANS_PERCENT_POSITION="))
                {
                    this._percentpos = line.Substring("ANS_PERCENT_POSITION=".Length);
                }
                else if (line.StartsWith("ANS_AUDIO_BITRATE=")) //audio bitrate
                {
                    this._getfileinfofilename = line.Substring("ANS_AUDIO_BITRATE=".Length);
                }
                else if (line.StartsWith("ANS_AUDIO_CODEC=")) //audio codec
                {
                    this._getfileinfofilename = line.Substring("ANS_AUDIO_CODEC=".Length);
                }
                else if (line.StartsWith("ANS_AUDIO_SAMPLES=")) //audio sample
                {
                    this._getfileinfofilename = line.Substring("ANS_AUDIO_SAMPLES=".Length);
                }
                else if (line.StartsWith("ANS_FILENAME=")) //audio filename
                {
                    this._getfileinfofilename = line.Substring("ANS_FILENAME=".Length);
                }
                else if (line.StartsWith("ANS_META_ALBUM=")) //album
                {
                    this._getfileinfofilename = line.Substring("ANS_META_ALBUM=".Length);
                }
                else if (line.StartsWith("ANS_META_ARTIST=")) //artista
                {
                    this._getfileinfofilename = line.Substring("ANS_META_ARTIST=".Length);
                }
                else if (line.StartsWith("ANS_META_COMMENT=")) //comentarios
                {
                    this._getfileinfofilename = line.Substring("ANS_META_COMMENT=".Length);
                }
                else if (line.StartsWith("ANS_META_GENRE=")) //genero
                {
                    this._getfileinfofilename = line.Substring("ANS_META_GENRE=".Length);
                }
                else if (line.StartsWith("ANS_META_TITLE=")) //titulo
                {
                    this._getinfotitle = line.Substring("ANS_META_TITLE=".Length);
                }
                else if (line.StartsWith("ANS_META_TRACK=")) //track
                {
                    this._getfileinfofilename = line.Substring("ANS_META_TRACK=".Length);
                }
                else if (line.StartsWith("ANS_META_YEAR=")) //ano
                {
                    this._getfileinfofilename = line.Substring("ANS_META_YEAR=".Length);
                }
                else if (line.StartsWith("ANS_VIDEO_BITRATE=")) //video bitrate
                {
                    this._getfileinfofilename = line.Substring("ANS_VIDEO_BITRATE=".Length);
                }
                else if (line.StartsWith("ANS_VIDEO_RESOLUTION=")) //video resoluçao 
                {
                    this._getfileinfofilename = line.Substring("ANS_VIDEO_RESOLUTION=".Length);
                }
                ////////            
            }
        }

        /// <summary>
        /// All mplayer error output is read through this function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMediaPlayerErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.consola(this, new MplayerEvent(e.Data.ToString()));
            }
        }

    }
}
