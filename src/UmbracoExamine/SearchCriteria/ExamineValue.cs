using Examine.SearchCriteria;

namespace UmbracoExamine.SearchCriteria
{
    internal class ExamineValue : IExamineValue
    {
        public ExamineValue(Examineness vagueness, string value) : this(vagueness, value, 1)
        {
        }

        public ExamineValue(Examineness vagueness, string value, float level)
        {
            this.Examineness = vagueness;
            this.Value = value;
            this.Level = level;
        }

        public Examineness Examineness
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }

        public float Level
        {
            get;
            private set;
        }

    }
}
