using System;
using System.Media;

namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// VoiceService.cs - Manages voice greetings and audio playback
    /// 
    /// Responsibilities:
    /// - Play audio files (greetings, alerts, notifications)
    /// - Handle audio errors gracefully
    /// - Provide voice feedback to user
    /// </summary>
    public class VoiceService
    {
        private SoundPlayer? _soundPlayer;
        private const string GREETING_FILE = "greeting.wav";

        public VoiceService()
        {
            _soundPlayer = new SoundPlayer();
        }

        /// <summary>
        /// Plays a voice greeting for the user
        /// </summary>
        /// <param name="audioPath">Path to the audio folder</param>
        /// <param name="userName">User's name for personalized greeting</param>
        public void PlayGreeting(string audioPath, string userName)
        {
            try
            {
                string greetingFile = Path.Combine(audioPath, GREETING_FILE);

                if (File.Exists(greetingFile))
                {
                    _soundPlayer = new SoundPlayer(greetingFile);
                    _soundPlayer.Play();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Greeting file not found: {greetingFile}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing greeting: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays a custom audio file
        /// </summary>
        /// <param name="audioFilePath">Full path to the audio file</param>
        public void PlaySound(string audioFilePath)
        {
            try
            {
                if (File.Exists(audioFilePath))
                {
                    _soundPlayer = new SoundPlayer(audioFilePath);
                    _soundPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays a beep notification
        /// </summary>
        public void PlayBeep()
        {
            try
            {
                Console.Beep(1000, 200);  // 1000 Hz, 200 ms
            }
            catch
            {
                // Silently fail if beep is not supported
            }
        }

        public void Dispose()
        {
            _soundPlayer?.Dispose();
        }
    }
}
