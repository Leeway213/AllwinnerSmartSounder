using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace SmartSounder.Tools.SpeechRecognizers
{
    public class SpeechRecognizerBase
    {
        protected AutoResetEvent _resetEvent;
        protected SpeechRecognizer _recognizer;
        public ResourceMap ResourceMap;
        public ResourceContext ResourceContext;

        public Language CurrentLanguage
        {
            get
            {
                return new Language(AppSettingsConstants.CurrentLanguage);
            }
        }

        public SpeechRecognizerState State
        {
            get
            {
                if (_recognizer != null)
                {
                    return _recognizer.State;
                }
                return SpeechRecognizerState.Idle;
            }
        }

        protected SpeechRecognizerBase(string speechResource)
        {
            _resetEvent = new AutoResetEvent(false);
            _recognizer = new SpeechRecognizer(CurrentLanguage);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree(speechResource);
                ResourceContext = ResourceContext.GetForCurrentView();
                ResourceContext.Languages = new string[] { _recognizer.CurrentLanguage.LanguageTag };

                _resetEvent.Set();
            });
        }

        public void BaseDispose()
        {
            _resetEvent?.Dispose();
            _recognizer?.Dispose();
        }

        protected SpeechRecognizerBase()
        {
            _resetEvent = new AutoResetEvent(false);
            _recognizer = new SpeechRecognizer();
        }

        public event TypedEventHandler<SpeechRecognizer, SpeechRecognizerStateChangedEventArgs> StateChanged
        {
            add { _recognizer.StateChanged += value; }
            remove { _recognizer.StateChanged -= value; }
        }

        public event TypedEventHandler<SpeechRecognizer, SpeechRecognitionHypothesisGeneratedEventArgs> HypothesisGenerated
        {
            add { _recognizer.HypothesisGenerated += value; }
            remove { _recognizer.HypothesisGenerated -= value; }
        }

        public event TypedEventHandler<SpeechContinuousRecognitionSession, SpeechContinuousRecognitionCompletedEventArgs> ContinuousRecognitionSessionCompleted
        {
            add { _recognizer.ContinuousRecognitionSession.Completed += value; }
            remove { _recognizer.ContinuousRecognitionSession.Completed -= value; }
        }

        public event TypedEventHandler<SpeechContinuousRecognitionSession, SpeechContinuousRecognitionResultGeneratedEventArgs> ContinuousRecognitionSessionResultGenerated
        {
            add { _recognizer.ContinuousRecognitionSession.ResultGenerated += value; }
            remove { _recognizer.ContinuousRecognitionSession.ResultGenerated -= value; }
        }
    }
}
