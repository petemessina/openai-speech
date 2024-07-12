using Microsoft.CognitiveServices.Speech;

namespace ConsoleApp6.Services
{
    internal class SpeechService
    {
        private readonly SpeechRecognizer _speechRecognizer;

        public SpeechService(SpeechRecognizer speechRecognizer) 
        {
            _speechRecognizer = speechRecognizer;
        }
    }
}
