'Code written by: Greg Chumney
'Using VS2010 IDE
'Encode class for encoding video files and generating thumbnails
'
'NOTES: This class seems to have a lot of redundant function calls, such as 
'calling the SourceExists function. It may not be needed, you may just delete 
'those lines if you wish. HOWEVER, I believe its better to be safe than sorry. 
'Everytime we try to get info, gen thumbnails, encode or anything like that we
'check JUST in case something happend to the source file so we can get out and 
'throw the exceptions.
'
'You can use this class for two scenarios. A: You have an app that has many files
'to be encoded, and you want to create an instance for each file. (ie, You may have 
'a custom control for each video file that displays a thumb and progress that you will
'queue.) or B: you have an app where you wish to only run ONE instance of the encoder,
'keep it open and just keep changing the source file property for each movie.
'
'Either way, A: OR B: it will keep track of how many thumbs it generates, and remove them
'upon you calling the dispose method.. 




''' <summary>
''' The encode class contains all the code to set up ffmpeg and start the encoding process
''' </summary>
Public Class Encoder
    Implements IDisposable

#Region "Events"
    ''' <summary>
    ''' Occurs when Progress changes.
    ''' </summary>
    Public Event Progress(ByVal ProgressPercent As Integer, ByVal TimeRemaining As String)
    ''' <summary>
    ''' Occurs when status changes.
    ''' </summary>
    Public Event Status(ByVal status As String)
    ''' <summary>
    ''' Occurs when StdError of ffmpeg changes. Usefull for debugging or to display the command line to user.
    ''' </summary>
    Public Event ConsoleOutput(ByVal StdOut As String)
    ''' <summary>
    ''' Misc info such as fps, qfactor, time, bitrate.. Basicly, anything that is in the Stdout of ffmpeg and estimated size.
    ''' </summary>
    Public Event MiscInfo(ByVal Fps As Integer, ByVal Qfactor As Integer, ByVal Time As Integer, ByVal Bitrate As Double _
                          , ByVal Frame As Integer, ByVal Size As String, ByVal EstimatedSize As String)
#End Region
#Region "Libx264 Presets"
    'TODO: Document all the presets.. for intellisense. Not very important right now
    'To use the x264 encoder, you will need to tell it which preset to use. if the user doesnt specify, the default will
    'be used. (see the example encoder for usage examples)
    'NOTE: Sorry, at this time i am unable to test the ipod presets. my ipod broke..But ill add them soon. 
    Public ReadOnly libx264_default As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method hex -subq 7 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 3 -directpred 1 -trellis 1 -flags2 +mixed_refs+wpred+dct8x8+fastpskip -wpredp 2"
    Public ReadOnly libx264_fast As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method hex -subq 6 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 2 -directpred 1 -trellis 1 -flags2 +bpyramid+mixed_refs+wpred+dct8x8+fastpskip -wpredp 2 -rc_lookahead 30"
    Public ReadOnly libx264_fast_firstpass As String = "-coder 1 -flags +loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partb8x8 -me_method dia -subq 2 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 1 -directpred 1 -trellis 0 -flags2 +bpyramid-mixed_refs+wpred-dct8x8+fastpskip -wpredp 2 -rc_lookahead 30"
    Public ReadOnly libx264_faster As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method hex -subq 4 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 2 -directpred 1 -trellis 1 -flags2 +bpyramid-mixed_refs+wpred+dct8x8+fastpskip -wpredp 1 -rc_lookahead 20"
    ' Public ReadOnly libx264_faster_firstpass As String
    Public ReadOnly libx264_fastfirstpass As String = "-coder 1 -flags +loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partp4x4-partb8x8 -me_method dia -subq 2 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 1 -directpred 3 -trellis 0 -flags2 -bpyramid-wpred-mixed_refs-dct8x8+fastpskip -wpredp 2"
    Public ReadOnly libx264_hq As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method umh -subq 8 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 2 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 4 -directpred 3 -trellis 1 -flags2 +wpred+mixed_refs+dct8x8+fastpskip -wpredp 2"
    Public ReadOnly libx264_max As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partp4x4+partb8x8 -me_method tesa -subq 10 -me_range 24 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 2 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 16 -directpred 3 -trellis 2 -flags2 +wpred+mixed_refs+dct8x8-fastpskip -wpredp 2"
    Public ReadOnly libx264_medium As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method hex -subq 7 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 3 -directpred 1 -trellis 1 -flags2 +bpyramid+mixed_refs+wpred+dct8x8+fastpskip -wpredp 2"
    Public ReadOnly libx264_medium_firstpass As String = "-coder 1 -flags +loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partb8x8 -me_method dia -subq 2 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 1 -directpred 1 -trellis 0 -flags2 +bpyramid-mixed_refs+wpred-dct8x8+fastpskip -wpredp 2"
    '  Public ReadOnly libx264_baseline As String
    Public ReadOnly libx264_normal As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method hex -subq 6 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 2 -directpred 3 -trellis 0 -flags2 +wpred+dct8x8+fastpskip -wpredp 2"
    Public ReadOnly libx264_slow As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partb8x8 -me_method umh -subq 8 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 2 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 3 -refs 5 -directpred 3 -trellis 1 -flags2 +bpyramid+mixed_refs+wpred+dct8x8+fastpskip -wpredp 2 -rc_lookahead 50"
    '  Public ReadOnly libx264_slow_firstpass As String
    '  Public ReadOnly libx264_slower As String
    ' Public ReadOnly libx264_slower_firstpass As String
    ' Public ReadOnly libx264_slowfirstpass As String
    ' Public ReadOnly libx264_superfast As String
    'Public ReadOnly libx264_superfast_firstpass As String
    Public ReadOnly libx264_ultrafast As String = "-coder 0 -flags -loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partb8x8 -me_method dia -subq 0 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 0 -i_qfactor 0.71 -b_strategy 0 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 0 -refs 1 -directpred 1 -trellis 0 -flags2 -bpyramid-mixed_refs-wpred-dct8x8+fastpskip-mbtree -wpredp 0 -aq_mode 0 -rc_lookahead 0"
    Public ReadOnly libx264_ultrafast_firstpass As String = "-coder 0 -flags -loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partb8x8 -me_method dia -subq 0 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 0 -i_qfactor 0.71 -b_strategy 0 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 0 -refs 1 -directpred 1 -trellis 0 -flags2 -bpyramid-mixed_refs-wpred-dct8x8+fastpskip-mbtree -wpredp 0 -aq_mode 0 -rc_lookahead 0"
    '  Public ReadOnly libx264_veryfast As String
    ' Public ReadOnly libx264_veryfast_firstpass As String
    ' Public ReadOnly libx264_veryslow As String
    ' Public ReadOnly libx264_veryslow_firstpass As String
    Public ReadOnly libx264_lossless_fast As String = "-coder 0 -flags +loop -cmp +chroma -partitions -parti8x8+parti4x4+partp8x8-partp4x4-partb8x8 -me_method hex -subq 3 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -directpred 1 -flags2 +fastpskip -cqp 0 -wpredp 0"
    Public ReadOnly libx264_lossless_max As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partp4x4-partb8x8 -me_method esa -subq 8 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -refs 16 -directpred 1 -flags2 +mixed_refs+dct8x8+fastpskip -cqp 0 -wpredp 2"
    Public ReadOnly libx264_lossless_medium As String = "-coder 1 -flags +loop -cmp +chroma -partitions -parti8x8+parti4x4+partp8x8+partp4x4-partb8x8 -me_method hex -subq 5 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -directpred 1 -flags2 +fastpskip -cqp 0 -wpredp 2"
    Public ReadOnly libx264_lossless_slow As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partp4x4-partb8x8 -me_method umh -subq 6 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -refs 2 -directpred 1 -flags2 +dct8x8+fastpskip -cqp 0 -wpredp 2"
    Public ReadOnly libx264_lossless_slower As String = "-coder 1 -flags +loop -cmp +chroma -partitions +parti8x8+parti4x4+partp8x8+partp4x4-partb8x8 -me_method umh -subq 8 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -refs 4 -directpred 1 -flags2 +mixed_refs+dct8x8+fastpskip -cqp 0 -wpredp 2"
    Public ReadOnly libx264_lossless_ultrafast As String = "-coder 0 -flags +loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partp4x4-partb8x8 -me_method dia -subq 0 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -b_strategy 1 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -directpred 1 -flags2 +fastpskip -cqp 0"
    Private _Libx264_custom As String = libx264_normal 'of course you can create your own :) but if you dont select anything, Normal is what you get!
    Private _Libx264_custom_pass2 As String = libx264_normal
    'TODO: finish off the presets. there is also the placebo presets and whatnot. 
#End Region
#Region "Acodecs"
    'TODO: Test and add more audio codecs
    Public ReadOnly AudioCodec_Copy As String = "copy"
    Public ReadOnly AudioCodec_NONE As String = "none"
    Public ReadOnly AudioCodec_mp3 As String = "libmp3lame"
    'AAC got removed from ffmpeg due to not being free or something. the other aac is experimental.. so that blows..
    'but, its the only AAC encoder that is in the new build. here are the trade offs:
    'old build april 2009 -- has the libfaac aac encoder.. But alot of other things have changed or were messed up.
    'so we loose a GREAT aac encoder, BUT we gain the ability to update to newer autobuilds of ffmpeg.. Groovy.
    Public ReadOnly AudioCodec_AAC As String = "aac -strict experimental" 'TODO: Note, if you upgrade ffmpeg, check to see 
    'if the included aac encoder is still "experimental".
#End Region
#Region "Vcodecs"
    'TODO: Test and add more video codecs
    Public ReadOnly Vcodec_Copy As String = "copy"
    Public ReadOnly Vcodec_h264 As String = "libx264"
    Public ReadOnly Vcodec_flv As String = "flv"
    Public ReadOnly Vcodec_Xvid As String = "libxvid"
    Public ReadOnly Vcodec_NONE As String = "none"
#End Region
#Region "Containers"
    'TODO: Test and add more containers

    'these containers are for when the user forces a container.. ie.. ffmpeg -i test.avi ~~~ -f flv Output.flv
    'NOTE: the encoder will "try" and determain the correct container based on the output file. SO, if the user
    'used output.flv as the outname, the encoder would "know" to use the FLV container format.
    'They arent manditory to use, but we will add them, because.. We are going to create guid named files without
    'an extension for the temp files. And thus, we will need to specify a container format..
    Public ReadOnly Format_MKV As String = "matroska" 'for .mkv extension
    Public ReadOnly Format_Matroska As String = "matroska" 'copy of above matroska.
    Public ReadOnly Format_FLV As String = "flv"
    Public ReadOnly Format_MOV As String = "mov"
    Public ReadOnly Format_MP4 As String = "mp4"
    Public ReadOnly Format_AVI As String = "avi"
    Public ReadOnly Format_MP3 As String = "mp3" 'Note: Audio only
    Public ReadOnly Format_AAC As String = "aac" 'NOTE: Audio only
    'THere is much to add. 3gp, ipod, ect. 
#End Region
#Region "Audio Options"
    Private _Aframes As Integer 'Set the number of audio frames to record. -aframes (int)
    Private _AFreq As String = "44100" 'sets the audio sampling frequency.. -ar 44100
    Private _AudioBitRate As String = "160k" '-ab 128k
    Private _AudioChannels As String = "2" '-ac 2 (default for 2 channel)
#End Region
#Region "video Options"
    Private _VideoBitRate As String = "1000k"
    Private _Vframes As String  'number of frames to encode.. -vframes (int)
    Private _Vrate As String  'set frame rate.. -r 23.98
    Private _size As String  'sets frame size.. -s 720x480
    Private _aspect As String  'sets aspect ratio.. -aspect 4:3 (16:9, 1.3333, 1.7777 will work too)
    'TODO: Add more video option variables as needed
    ' -metadata key=value -metadata title="my title"
    'TODO: Work on metadata keys.
    Public ReadOnly RateControl_ABR As String = "abr"
    Public ReadOnly RateControl_VBR As String = "vbr"
    Public ReadOnly RateControl_CRF As String = "crf"
#End Region
#Region "Messages and exceptions"
    'TODO: Create exceptions as needed. Make them as non-cryptic as possible as these will probably be
    'passed right to the end user. 
    Dim InvalidRateControlOption As String = "CRF is not a valid rate control for the selected codec, the default bitrate will be used."
    Dim SourceNotFound As String = "Source file " & _Source & " not found"

#End Region
#Region "variables" 'depreciated, see Audio/video options above :)
    Private _ffmpeg As String = "" 'Holds the Path of the ffmpeg executable. 
    Private _Threads As String = "0" 'Default, Lets FFMpeg decide the number of threads
    Private _ACODEC As String = "copy" 'Default will be to copy the audio stream.
    Private _VideoCodec As String = "copy"
    Private _RateControl As String = "abr" 'choices are: "abr" for 1pass average bitrate
    '"vbr" for 2 pass variable bitrate or "crf" for constant rate factor
    Private _Crf As Integer = 17 '-crf 17 sounded like a good number :) If your using -crf then you know what you want this set as.
    Private _Overwrite As Boolean = False
    Private _Container As String
    Private _TempName As String
    Private _Analyzed As Boolean = False
    Private _TotalFrames As Integer
    Private _AnalyzedFailed As Boolean = False


    Private _FFmpegProcessPropertys As New ProcessStartInfo 'TODO: give unique process id & name

    Private _seconds As Integer
    Private _thumb As String 'Will hold the path name of the thumbnail.
    Private _ThumbGenerated As Boolean = False
    Private _ThumbnailList As New ArrayList 'Will hold a list of all temperary thumbnails generated, so they can be deleted
    'when the garbage collector cleans up.


    'Note: Commmand line will go as follows, 
    '-i <infile> <Custom params ie. -map> <audio options> <video options> <libx264 pre (if needed)> <Thread count> <container> <output>
    Private _Source As String
    Private _AudioParams As String
    Private _VideoParamsPass1 As String
    Private _VideoParamsPass2 As String
    Private _CustomParams As String '-map 0:0 -map 0:1 for example
    Private _OutputDirectory As String 'ie. d:\encoded files\
    Private _FileName As String 'ie.. Encoded.mkv: if one isnt selected one will be created from input name
    Private _CommandPass1 As String
    Private _CommandPass2 As String
#End Region
#Region "File Types"
    Dim _AudioFileTypes As New ArrayList
    Private Sub AddAudioFiles()
        'TODO: ad any known "audio only" File types.. ie.. mp3, aac.. 
        'we will check this list, if the input file is audio only
        'then we can auto disable the video encoding. 
        _AudioFileTypes.Add(".mp3")
        _AudioFileTypes.Add(".wmv")
        _AudioFileTypes.Add(".flac")
        _AudioFileTypes.Add(".aac")
        _AudioFileTypes.Add(".ac3")
        _AudioFileTypes.Add(".aif")
        _AudioFileTypes.Add(".at3")
        _AudioFileTypes.Add(".ogg")
        _AudioFileTypes.Add(".wav")
        '_AudioFileTypes.Add(".cda") NOT SUPPORTED in ffmpeg. sorry, you'll have to rip your cd's some other way.
        'NOTE: ive not tested all of these extensions. also, if an input file is added with an extension
        'from the above list it will over-ride the video settings to -vn (no video encoding). 
        'This is to prevent any errors with ffmpeg. as of right now, there is no code to combine audio or video
        'or different sources. So if you want to put fourth the effort that is great. 
    End Sub
#End Region
#Region "Private Methods"
    Private Sub SetupAudio()
        If _ACODEC = "copy" Then
            _AudioParams = "-acodec copy"
        ElseIf _ACODEC = "none" Then
            _AudioParams = "-an"
        Else
            _AudioParams = String.Format("{0} {1} {2} {3} {4}", FAudiocodec, FAudioBitrate, FAudioFrequency, FAudioChannels, FAudioFrames)
        End If
    End Sub
    Private Sub SetupVideo()
        ValidRateControl() 'will test against codec, and make sure the rate control selected will work.
        Select Case _RateControl
            Case "abr"
                If _VideoCodec = "copy" Then
                    _VideoParamsPass1 = "-vcodec copy"
                ElseIf _VideoCodec = "none" Then
                    _VideoParamsPass1 = "-vn"
                ElseIf _VideoCodec = Vcodec_h264 Then
                    _VideoParamsPass1 = String.Format("{0}{1}{2}{3}{4}{5} {6}", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect, _Libx264_custom)
                Else
                    _VideoParamsPass1 = String.Format("{0}{1}{2}{3}{4}{5}", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect)
                End If
            Case "vbr"
                If _VideoCodec = "copy" Then
                    _VideoParamsPass1 = "-vcodec copy"
                    _VideoParamsPass2 = "-vcodec copy"
                Else
                    If _VideoCodec = Vcodec_h264 Then
                        _VideoParamsPass1 = String.Format("{0}{1}{2}{3}{4}{5}{6} {7}", "-pass 1 ", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect, _Libx264_custom)
                        _VideoParamsPass2 = String.Format("{0}{1}{2}{3}{4}{5}{6} {7}", "-pass 2 ", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect, _Libx264_custom_pass2)

                    Else
                        _VideoParamsPass1 = String.Format("{0}{1}{2}{3}{4}{5}{6}", "-pass 1 ", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect)
                        _VideoParamsPass2 = String.Format("{0}{1}{2}{3}{4}{5}{6}", "-pass 2 ", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect)
                    End If
                End If
            Case "crf" 'User should have chosen libx264!
                _VideoParamsPass1 = String.Format("{0}{1}{2}{3}{4}{5} {6}", FVideoCodec, FVideoBitrate, FVideoFrames, FVideoFrameRate, FVideoSize, FVideoAspect, _Libx264_custom)
        End Select
    End Sub
    Private Sub SetupCommandPass1()
        'command line should go something like: -i <source> <custom params> -pass 1 -an _videoparamspas1 -threads int NUL
        If _RateControl = RateControl_VBR Then
            '1st of 2 pass, NO audio, output to nul
            _CommandPass1 = String.Format("-i {0} {1} {2} {3} {4} {5}{6}", _Source, _CustomParams, "-an", _VideoParamsPass1, "-threads " & _Threads, "-y", "nul")

        Else
            'single pass only. 
            _CommandPass1 = String.Format("-i {0} {1} {2} {3} {4} {5}{6}", _Source, _CustomParams, _AudioParams, _VideoParamsPass1, "-threads " & _Threads, """" & _OutputDirectory, _FileName & """")
        End If
    End Sub
    Private Sub SetupCommandPass2()
        'second pass, WITH audio
        'command line should go something like: -i <source> <custom params> -pass 1 _audioparams2 _videoparamspas2 -threads int <out>
        _CommandPass2 = ""
    End Sub
    Private Function ValidRateControl() As Boolean
        'As far as i know (could be more but ive not yet tested) h264 is the only encoder
        'that uses -crf (int) flag. However, if another codec supports it, just add it below
        If _RateControl = "crf" Then
            Select Case _VideoCodec
                'TODO: Test other encoders for crf useability and add them below
                Case "libx264"
                    Return True
                Case Else
                    _RateControl = RateControl_ABR
                    Return False
            End Select

        Else
            Return True
        End If
    End Function
    'Private Sub RenameFile()
    '    Try
    '        My.Computer.FileSystem.RenameFile(_OutputDirectory & _TempName, _FileName)
    '    Catch ex As Exception

    '    End Try
    'End Sub
    Private Sub RunEncode(ByVal Argument As String)
        If TargetExists() = True Then
            If _Overwrite = True Then
                'delete file.. 
                Try
                    My.Computer.FileSystem.DeleteFile(_OutputDirectory & _FileName)
                Catch ex As Exception
                End Try
            Else
                Dim TargetAlreadyExistsException As New Exception("The Target file already exists")
                Throw TargetAlreadyExistsException
            End If
        End If


        _FFmpegProcessPropertys.Arguments = Argument
        _FFmpegProcessPropertys.UseShellExecute = False
        _FFmpegProcessPropertys.RedirectStandardOutput = True
        _FFmpegProcessPropertys.RedirectStandardError = True
        _FFmpegProcessPropertys.CreateNoWindow = True
        Dim FFmpegProcess = Process.Start(_FFmpegProcessPropertys)
        Dim readerStdOut As IO.StreamReader = FFmpegProcess.StandardError
        Dim sOutput As String
        While readerStdOut.EndOfStream = False
            sOutput = readerStdOut.ReadLine()
            RaiseEvent ConsoleOutput(sOutput)
            ' RaiseEvent Percent(GetPercent(sOutput, TotalFrames))
            Dim Percent As Integer = GetPercent(sOutput)
            RaiseEvent Progress(Percent, Timeleft(sOutput))

        End While
        FFmpegProcess.WaitForExit()
        FFmpegProcess.Close()


    End Sub
#End Region
#Region "Private Functions"
    Private Function FVideoCodec() As String
        Select Case _VideoCodec
            Case "copy"
                Return "-vcodec copy"
            Case "none"
                Return "-vn" 'should never return. But just in case..
            Case Else
                Return "-vcodec " & _VideoCodec
        End Select
    End Function
    Private Function FVideoBitrate() As String
        If _RateControl = "crf" Then
            Return " -crf " & _Crf
        Else
            Return " -b " & _VideoBitRate
        End If
    End Function
    Private Function FVideoFrames() As String
        If _Vframes = Nothing Then
            Return ""
        Else
            Return " -vframes " & _Vframes
        End If
    End Function
    Private Function FVideoFrameRate() As String
        If _Vrate = Nothing Then
            Return ""
        Else
            Return " -r " & _Vrate
        End If
    End Function
    Private Function FVideoSize() As String
        If _size = Nothing Then
            Return ""
        Else
            Return " -s " & _size
        End If
    End Function
    Private Function FVideoAspect() As String
        If _aspect = Nothing Then
            Return ""
        Else
            Return " -aspect " & _aspect
        End If
    End Function
    Private Function FAudioFrames() As String
        If _Aframes = Nothing Then
            Return ""
        Else
            Return "-aframes " & _Aframes
        End If
    End Function
    Private Function FAudioChannels() As String
        Return "-ac " & _AudioChannels
    End Function
    Private Function FAudioFrequency() As String
        Return "-ar " & _AFreq
    End Function
    Private Function FAudioBitrate() As String
        Return "-ab " & _AudioBitRate
    End Function
    Private Function FAudiocodec() As String
        Return "-acodec " & _ACODEC
    End Function
    'TODO: Add code to the Analyze functions to get stream information!
    Private Function QuickAnalyze() As Boolean
        Dim infoprocess As New ProcessStartInfo
        infoprocess.Arguments = "-i " & _Source '-an -vcodec copy -f rawvideo -y null"
        infoprocess.UseShellExecute = False
        infoprocess.RedirectStandardError = True
        infoprocess.CreateNoWindow = True
        infoprocess.FileName = _ffmpeg
        Dim FFmpegProcess = Process.Start(infoprocess)
        Dim readerStdOut As IO.StreamReader = FFmpegProcess.StandardError
        Dim sOutput As String
        Dim hours As Integer
        Dim minutes As Integer
        Dim seconds As Integer
        'So far, from what i can tell, ffmpeg will get a pretty good time IN SECONDS 
        'On vob files, mostly ones with broken headers will report the wrong time 
        'with the message max_analyze_duration reached. so, for now, we will assume
        'that if we dont get the MAD reached message we got good info. 
        While readerStdOut.EndOfStream = False
            sOutput = readerStdOut.ReadLine
            'check for MAD reached message
            If sOutput.Contains("max_analyze_duration") = True Then
                FFmpegProcess.Close()
                Return False 'ie, we werent able to perform a quick info analisys.
            Else
                If sOutput.Contains("Duration:") Then 'We want time to determain progress.
                    Dim split1 As String() = sOutput.Split(New [Char]() {" ", ","})
                    Dim String1 As String = split1(3)
                    Dim times As String() = String1.Split(New Char() {":"c})
                    Try
                        hours = times(0)
                        minutes = times(1)
                        seconds = times(2)
                        hours = hours * 60
                        minutes = minutes + hours
                        minutes = minutes * 60
                        _seconds = seconds + minutes
                    Catch ex As Exception
                        FFmpegProcess.Close()
                        Return False
                    End Try
                End If
                Dim Fps As String
                If sOutput.Contains(" fps,") Then 'We want fps, so we can determain total frames so we can get ETA of completion. 
                    Try
                        Dim split1 As String() = sOutput.Split(New [Char]() {" ", ","})
                        Dim s1 As String = split1(20)
                        Fps = s1
                    Catch ex As Exception
                        FFmpegProcess.Close()
                        Return False
                    End Try
                Else
                    FFmpegProcess.Close()
                    Return False
                End If
                'Encase in a try/catch, just in case FPS pulled from the console is something besides a number.. :)
                Try
                    _TotalFrames = seconds * Fps
                Catch ex As Exception
                    FFmpegProcess.Close()
                    Return False
                End Try
            End If
        End While
        FFmpegProcess.WaitForExit()
        FFmpegProcess.Close()
        Return True
    End Function
    Private Function SlowAnalyze() As Boolean
        Dim infoprocess As New ProcessStartInfo
        infoprocess.Arguments = "-i " & _Source & " -an -vcodec copy -f rawvideo -y nul"
        infoprocess.UseShellExecute = False
        infoprocess.RedirectStandardError = True
        infoprocess.CreateNoWindow = True
        infoprocess.FileName = _ffmpeg
        Dim FFmpegProcess = Process.Start(infoprocess)
        Dim readerStdOut As IO.StreamReader = FFmpegProcess.StandardError
        Dim sOutput As String
        Dim frame = ""
        Dim fps = ""
        Dim qfac = ""
        Dim size = ""
        Dim time = ""
        Dim br = ""
        While readerStdOut.EndOfStream = False
            sOutput = readerStdOut.ReadLine()
            RaiseEvent ConsoleOutput(sOutput)
            If sOutput.Contains("frame=") Then
                Try
                    Dim OutLine As String() = sOutput.Split(New Char() {"="})
                    Dim chunk1 As String() = OutLine(1).Split(New Char() {" "}) 'Frame
                    Dim chunk2 As String() = OutLine(2).Split(New Char() {" "}) 'fps (speed of encoding, NOT the actual frame rate)
                    Dim chunk3 As String() = OutLine(3).Split(New Char() {" "}) 'qfactor
                    Dim chunk4 As String() = OutLine(4).Split(New Char() {" "}) 'size
                    Dim chunk5 As String() = OutLine(5).Split(New Char() {" "}) 'time
                    Dim chunk6 As String() = OutLine(6).Split(New Char() {" "}) 'bitrate
                    'the above chunks had to be split. 
                    'the ones we care about are FRAME AND TIME: Now, because on short files
                    'there may be many spaces, ie frames=     32 is different than frames=  3343 <--note less spaces.
                    Dim TimeHit As Boolean = False
                    Dim FrameHit As Boolean = False
                    Dim fpshit As Boolean = False
                    Dim qfachit As Boolean = False
                    Dim sizehit As Boolean = False
                    Dim brhit As Boolean = False
                    For index = 0 To chunk1.Count - 1 'should account for enuff spaces!
                        If chunk1(index) <> "" And FrameHit = False Then
                            frame = chunk1(index)
                            FrameHit = True
                        End If
                    Next
                    For index = 0 To chunk5.Count - 1 'should account for enuff spaces!
                        If chunk5(index) <> "" And TimeHit = False Then
                            time = chunk5(index)
                            TimeHit = True
                        End If
                    Next
                    'If chunk5(index) <> "" And TimeHit = False Then
                    '    time = chunk5(index)
                    '    TimeHit = True
                    'End If
                    'frame = chunk1(0)
                    'fps = chunk2(0)
                    'qfac = chunk3(0)
                    'size = chunk4(0)
                    'time = chunk5(0)
                    'br = chunk6(0)
                Catch ex As Exception
                    'do nothing, prolly an audio file. Besides, all a failure here means is no progress, lol. 
                    'basicly, the progress will have to be intermanate. 
                    FFmpegProcess.Kill()
                    FFmpegProcess.Close()
                    Return False
                End Try
            End If
        End While
        If IsNumeric(time) = True Then
            _seconds = time
        End If
        If IsNumeric(frame) = True Then
            _TotalFrames = frame
        End If

        FFmpegProcess.WaitForExit()
        FFmpegProcess.Close()
        Return True
    End Function
    Private Function SourceExists() As Boolean
        If My.Computer.FileSystem.FileExists(_Source.Replace("""", "")) = True Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Function IsVideo() As Boolean
        'first, parse the file extension from the input file, check if its on the audio list..
        Dim FileExt As String = My.Computer.FileSystem.GetFileInfo(_Source.Trim("""", "")).Extension.ToLower
        If _AudioFileTypes.Contains(FileExt) = True Then

            Return False
        Else : Return True
        End If
    End Function
    Private Function GenerateThumb() As String
        If _ThumbGenerated = True Then
            Return _thumb
        Else
            _thumb = Nothing
            If SourceExists() = True Then
                AnalyzeFile()
                Dim Seconds As Integer = 1
                If _seconds > 45 Then
                    Seconds = 45
                Else
                    Seconds = _seconds
                End If
                Try
                    _thumb = System.IO.Path.GetTempPath & System.Guid.NewGuid().ToString & ".jpg" 'Create a new filename... 
                Catch ex As Exception
                    Return _thumb 'file creation failed for some reason. return nothin, get outta here.
                End Try
                _ThumbnailList.Add(_thumb)
                Dim Params As String = String.Format("-i {0} ""{1}"" -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo", _Source, _thumb, Seconds)
                _FFmpegProcessPropertys.Arguments = Params
                _FFmpegProcessPropertys.UseShellExecute = False
                _FFmpegProcessPropertys.RedirectStandardOutput = True
                _FFmpegProcessPropertys.RedirectStandardError = True
                _FFmpegProcessPropertys.CreateNoWindow = True
                Dim FFmpegProcess = Process.Start(_FFmpegProcessPropertys)
                Dim readerStdOut As IO.StreamReader = FFmpegProcess.StandardError
                Dim sOutput As String
                While readerStdOut.EndOfStream = False
                    sOutput = readerStdOut.ReadLine()
                    RaiseEvent ConsoleOutput(sOutput)
                    RaiseEvent Status(String.Format("Generating thumbnail for {0}", _FileName))
                End While
                FFmpegProcess.WaitForExit()
                FFmpegProcess.Close()
                _ThumbGenerated = True
                Return _thumb
            Else
                _ThumbGenerated = True
                Return _thumb
            End If
        End If
    End Function
    Private Function GetFileName() As String
        'NOTE: Even tho ffmpeg does this for you, we will do it ourselfs, we may remove it later. who knows.
        'This will run and check if user entered in a custom filename. ie, encoded.mkv
        'if not, then we will create on based on the container(if set) OR the codec
        Dim SourceExt As String = My.Computer.FileSystem.GetFileInfo(_Source.Trim("""")).Extension
        Dim SourceName As String = My.Computer.FileSystem.GetFileInfo(_Source.Trim("""")).Name.Replace(SourceExt, "")
        'We now have JUST the name of the source file, with No extension, now we need to add an extension based on
        'the codec, or preferably the container if it was set..
        If _FileName Is Nothing Then 'User didnt specify a custom filename, lets create one.
            'BTW, we are gonna do somethin i HATE doing, which is alotta nested directions. so lets get focused.
            If _Container Is Nothing Then
                'generate file based on codec!
                'Note, we must check for audio only! 
                If IsVideo() = True Then
                    'check the video containers.
                    Select Case _VideoCodec
                        Case Vcodec_Copy
                            'We shoulda checked this before. But the in and outfile can stay the same
                            'I hope the user CHANGED the output directory to something different! haha
                            Return SourceName & SourceExt
                        Case Vcodec_flv
                            Return SourceName & ".flv"
                        Case Vcodec_h264
                            Return SourceName & ".mp4"
                        Case Vcodec_Xvid
                            Return SourceName & ".mp4"
                            'TODO: if you add more codecs, and they would use another ext, like REAL media for example 
                            'add them. otherwise, just do a CASE ELSE, and add a default like avi or mp4. 
                    End Select
                Else
                    'check the audio containers.
                    Select Case _ACODEC
                        Case AudioCodec_AAC
                            Return SourceName & ".aac"
                        Case AudioCodec_Copy
                            Return SourceName & SourceExt
                        Case AudioCodec_mp3
                            Return SourceName & ".mp3"
                            'TODO: Do the same for audio formats as you did for video.
                    End Select
                End If
            Else
                'generate file based on container(which is preferable)
                Select Case _Container
                    Case Format_AAC
                        Return SourceName & ".aac"
                    Case Format_AVI
                        Return SourceName & ".avi"
                    Case Format_FLV
                        Return SourceName & ".flv"
                    Case Format_Matroska
                        Return SourceName & ".mkv"
                    Case Format_MOV
                        Return SourceName & ".mov"
                    Case Format_MP3
                        Return SourceName & ".mp3"
                    Case Format_MP4
                        Return SourceName & ".mp4"
                    Case Else
                End Select
            End If
            Return SourceName
        Else 'then return what the user entered in for the filename. 
            Return _FileName
        End If
    End Function
    Private Function TargetExists() As Boolean
        If My.Computer.FileSystem.FileExists(_OutputDirectory & _FileName) = True Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Function TempName() As String
        _TempName = System.Guid.NewGuid().ToString
        Return _TempName
    End Function
    Private Function GetPercent(ByVal Cout As String) As Integer
        Dim Frame As Integer = 0
        If Cout.Contains("frame=") Then
            Try
                Dim OutLine As String() = Cout.Split(New Char() {"="})
                Dim chunk1 As String() = OutLine(1).Split(New Char() {" "}) 'Frame
                Dim chunk2 As String() = OutLine(2).Split(New Char() {" "}) 'fps (speed of encoding, NOT the actual frame rate)
                Dim chunk3 As String() = OutLine(3).Split(New Char() {" "}) 'qfactor
                Dim chunk4 As String() = OutLine(4).Split(New Char() {" "}) 'size
                Dim chunk5 As String() = OutLine(5).Split(New Char() {" "}) 'time
                Dim chunk6 As String() = OutLine(6).Split(New Char() {" "}) 'bitrate
                'the above chunks had to be split. 
                'the ones we care about are FRAME AND TIME: Now, because on short files
                'there may be many spaces, ie frames=     32 is different than frames=  3343 <--note less spaces.
                Dim TimeHit As Boolean = False
                Dim FrameHit As Boolean = False
                Dim fpshit As Boolean = False
                Dim qfachit As Boolean = False
                Dim sizehit As Boolean = False
                Dim brhit As Boolean = False

                For index = 0 To chunk1.Count - 1 'should account for enuff spaces!
                    If chunk1(index) <> "" And FrameHit = False Then
                        Frame = chunk1(index)
                        FrameHit = True
                    End If
                Next

                'If chunk5(index) <> "" And TimeHit = False Then
                '    time = chunk5(index)
                '    TimeHit = True
                'End If
                'frame = chunk1(0)
                'fps = chunk2(0)
                'qfac = chunk3(0)
                'size = chunk4(0)
                'time = chunk5(0)
                'br = chunk6(0)
                Return Frame / _TotalFrames * 100
            Catch ex As Exception
            End Try
        End If
        Return Nothing
    End Function
    Private Function Timeleft(ByVal Cout) As String
        Return 0
    End Function
#End Region
#Region "constructors"
    ''' <summary>
    ''' Initializes a new instance of the <see cref="Encoder" /> class.
    ''' </summary>
    Sub New()
        _ffmpeg = My.Application.Info.DirectoryPath & "\ffmpeg\ffmpeg.exe"
        _FFmpegProcessPropertys.FileName = _ffmpeg
    End Sub
    ''' <summary>
    ''' Initializes a new instance of the <see cref="Encoder" /> class.
    ''' </summary>
    ''' <param name="FFmpeg">The FFmpeg executable location.</param>
    Sub New(ByVal FFmpeg As String)
        _ffmpeg = FFmpeg
        _FFmpegProcessPropertys.FileName = _ffmpeg
    End Sub
#End Region
#Region "Public Methods"
    Public Sub AnalyzeFile()
        'this may be called explicitly, or it should run automaticly before the encoder is ran
        'It can be be called so if the user wanted to analyze files as added ect ect..
        'Also, we need to check to see if its a video file, because we wont analyze audio files. no point.
        If IsVideo() = True Then
            If _Analyzed = False Then
                RaiseEvent Status("Analayzing file")
                If QuickAnalyze() = False Then 'Meaning Couldn't get the info by doing a quick analyze
                    If SlowAnalyze() = False Then
                        _AnalyzedFailed = True
                    End If
                End If
                _Analyzed = True
            End If
        End If

    End Sub
    Public Sub Encode()
        'Time to get to work, and encode a file. 
        If SourceExists() = True Then 'else do nothing, will error out anyways. ANd again, we will check when we analyze
            RaiseEvent Status("Setting up encoder")
            AnalyzeFile()
            _FileName = GetFileName()
            SetupAudio()
            SetupVideo()
            If _RateControl = RateControl_VBR Then
                SetupCommandPass1()
                SetupCommandPass2()
                RaiseEvent Status("Running Pass 1")
                RunEncode(_CommandPass1)
                RaiseEvent Status("Running Pass 2")
                RunEncode(_CommandPass2)
            Else
                RaiseEvent Status("Encoding")
                SetupCommandPass1()
                RunEncode(_CommandPass1)
            End If
            'RenameFile()
        End If
        RaiseEvent Status("Encoding Finished")
    End Sub
#End Region
#Region "Propertys"
    Public Property SourceFile As String
        Get
            Return _Source
        End Get
        Set(ByVal Source As String)
            _Source = """" & Source & """"
            _ThumbGenerated = False
            _Analyzed = False
            _AnalyzedFailed = False
            _FileName = Nothing
        End Set
    End Property
    Public Property Format As String
        Get
            Return _Container
        End Get
        Set(ByVal value As String)
            _Container = value
        End Set
    End Property
    Public Property AudioCodec As String
        Get
            Return _ACODEC
        End Get
        Set(ByVal value As String)
            _ACODEC = value
        End Set
    End Property
    Public Property Video_Codec As String
        Get
            Return _VideoCodec
        End Get
        Set(ByVal value As String)
            _VideoCodec = value
        End Set
    End Property
    Public Property AudioBitrate As String
        Get
            Return _AudioBitRate
        End Get
        Set(ByVal value As String)
            _AudioBitRate = value
        End Set
    End Property
    Public Property VideoBitrate As String
        Get
            Return _VideoBitRate
        End Get
        Set(ByVal value As String)
            _VideoBitRate = value
        End Set
    End Property
    Public Property Threads As String
        Get
            Return _Threads
        End Get
        Set(ByVal value As String)
            _Threads = value
        End Set
    End Property
    Public Property OverWrite As Boolean
        Get
            Return _Overwrite
        End Get
        Set(ByVal value As Boolean)
            _Overwrite = value
        End Set
    End Property
    Public Property ConstantRateFactor As Integer
        Get
            Return _Crf
        End Get
        Set(ByVal value As Integer)
            _Crf = value
        End Set
    End Property
    Public Property RateControl As String
        Get
            Return _RateControl
        End Get
        Set(ByVal value As String)
            _RateControl = value
        End Set
    End Property
    Public Property Libx264_Preset_pass1 As String
        Get
            Return _Libx264_custom
        End Get
        Set(ByVal value As String)
            _Libx264_custom = value
        End Set
    End Property
    Public Property Libx264_Preset_pass2 As String
        Get
            Return _Libx264_custom_pass2
        End Get
        Set(ByVal value As String)
            _Libx264_custom_pass2 = value
        End Set
    End Property
    Public Property OutputPath As String
        Get
            Return _OutputDirectory
        End Get
        Set(ByVal value As String)
            If value.EndsWith("\") Then
                If My.Computer.FileSystem.DirectoryExists(value) = False Then
                    My.Computer.FileSystem.CreateDirectory(value)
                End If
                _OutputDirectory = value
            Else
                If My.Computer.FileSystem.DirectoryExists(value & "\") = False Then
                    My.Computer.FileSystem.CreateDirectory(value & "\")
                End If
                _OutputDirectory = value & "\"
            End If

        End Set
    End Property
    Public ReadOnly Property ThumbNail As String
        Get
            Return GenerateThumb()
        End Get
    End Property
    Public Property Filename As String
        Get
            Return _FileName
        End Get
        Set(ByVal value As String)
            _FileName = value
        End Set
    End Property
    Public Property CustomParams As String
        Get
            Return _CustomParams
        End Get
        Set(ByVal value As String)
            _CustomParams = value
        End Set
    End Property
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.

            'When the garbage collector tears down the encoder, It will delete any thumbnail files
            'created in the temp directory.
            For Each Thumbs In _ThumbnailList
                If My.Computer.FileSystem.FileExists(Thumbs) = True Then
                    Try
                        RaiseEvent Status(String.Format("Removing generated thumbnail from system: {0}", Thumbs))
                        My.Computer.FileSystem.DeleteFile(Thumbs)

                    Catch ex As Exception
                        'If for some reason it cannot access the file, then do nothing.
                    End Try
                End If
            Next
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    Protected Overrides Sub Finalize()
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
