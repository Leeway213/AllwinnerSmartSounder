using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace SmartSounder.Tools.SpeechRecognizers
{
    public class CommandSpeechRecognizer : SpeechRecognizerBase
    {
        private static CommandSpeechRecognizer _instance;
        public static CommandSpeechRecognizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CommandSpeechRecognizer();
                }
                return _instance;
            }
        }

        private CommandSpeechRecognizer() : base()
        {
            InitializeRecognizer();
        }

        private async void InitializeRecognizer()
        {
            var grammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "SmartSounder");
            _recognizer.Constraints.Add(grammar);
            var result = await _recognizer.CompileConstraintsAsync();
            _recognizer.ContinuousRecognitionSession.AutoStopSilenceTimeout = new TimeSpan(0, 0, 2);
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                throw new Exception();
            }
        }

        public static async Task StartAsync()
        {
            try
            {
                await Instance._recognizer.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task StopAsync()
        {
            try
            {
                await Instance._recognizer.ContinuousRecognitionSession.CancelAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
