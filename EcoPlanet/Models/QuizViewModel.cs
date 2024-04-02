namespace EcoPlanet.Models
{
    public class QuizViewModel
    {
        public List<Quiz> Questions { get; set; }
        public List<string> UserAnswers { get; set; }
        public int CurrentQuestionIndex { get; set; } = 0;
        public bool IsFinished { get; set; } = false;
    }
}
