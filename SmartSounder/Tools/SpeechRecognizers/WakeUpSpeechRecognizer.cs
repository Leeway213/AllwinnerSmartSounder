using GalaSoft.MvvmLight.Threading;
using SmartSounder.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.SpeechRecognition;

namespace SmartSounder.Tools.SpeechRecognizers
{
    public class WakeUpSpeechRecognizer : SpeechRecognizerBase
    {
        public const string SPEECH_RESOURCE = "WakeUpSpeechResources";

        public string WakeUpString { get; private set; }

        private static WakeUpSpeechRecognizer _wakeUpSpeech;
        public static WakeUpSpeechRecognizer Instance
        {
            get
            {
                if (_wakeUpSpeech == null)
                {
                    _wakeUpSpeech = new WakeUpSpeechRecognizer();
                }
                return _wakeUpSpeech;
            }
            private set
            {
                _wakeUpSpeech = value;
            }
        }


        private WakeUpSpeechRecognizer() : base(SPEECH_RESOURCE)
        {
            InitializeRecognizer();
        }

        private async void InitializeRecognizer()
        {
            _resetEvent.WaitOne(3000);
            string wakeupStr = ResourceMap.GetValue("HeyCortana", ResourceContext).ValueAsString;
            _recognizer.Constraints.Add(
                new SpeechRecognitionListConstraint(
                    new List<string>()
                    {
                        wakeupStr
                    }, "Wakeup"));

            List<string> wrongWakeStrings = new List<string>();
            foreach (var item in ResourceMap)
            {
                //string str = item.Value.Candidates.First().ValueAsString;
                string str = ResourceMap.GetValue(item.Key, ResourceContext).ValueAsString;
                if (str != wakeupStr)
                {
                    wrongWakeStrings.Add(str);
                }
            }
            _recognizer.Constraints.Add(
                new SpeechRecognitionListConstraint(wrongWakeStrings, "Wrong"));
            var result = await _recognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                throw new Exception();
            }

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                WakeUpString = ResourceMap.GetValue("HeyCortana", ResourceContext).ValueAsString;
            });
        }


        public static async Task StartAsync()
        {
            try
            {
                await Instance._recognizer.ContinuousRecognitionSession.StartAsync(SpeechContinuousRecognitionMode.Default);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task StopAsync()
        {
            try
            {
                if (Instance._recognizer.State != SpeechRecognizerState.Idle)
                {
                    await Instance._recognizer.ContinuousRecognitionSession.CancelAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Dispose()
        {
            Instance.BaseDispose();
            Instance = null;
        }

    }
}
