namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Published when a migration plan has been successfully executed.
    /// </summary>
    public class MigrationPlanExecuted : INotification
    {
        public MigrationPlanExecuted(string migrationPlanName, string initialState, string finalState)
        {
            MigrationPlanName = migrationPlanName;
            InitialState = initialState;
            FinalState = finalState;
        }

        public string MigrationPlanName { get; }
        public string InitialState { get; }
        public string FinalState { get; }
    }
}
