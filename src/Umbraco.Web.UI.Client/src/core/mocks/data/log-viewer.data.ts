import { logs } from './logs.data';
import { UmbData } from './data';
import { LogMessageModel, LogTemplateModel, SavedLogSearchModel } from '@umbraco-cms/backend-api';

// Temp mocked database
class UmbLogviewerSearchesData extends UmbData<SavedLogSearchModel> {
	constructor(data: SavedLogSearchModel[]) {
		super(data);
	}

	// skip can be number or null
	getSavedSearches(skip = 0, take = this.data.length): Array<SavedLogSearchModel> {
		return this.data.slice(skip, take);
	}

	getByName(name: string) {
		return this.data.find((search) => search.name === name);
	}
}

class UmbLogviewerTemplatesData extends UmbData<LogTemplateModel> {
	constructor(data: LogTemplateModel[]) {
		super(data);
	}

	// skip can be number or null
	getTemplates(skip = 0, take = this.data.length): Array<LogTemplateModel> {
		return this.data.slice(skip, take);
	}
}

class UmbLogviewerMessagesData extends UmbData<LogMessageModel> {
	constructor(data: LogTemplateModel[]) {
		super(data);
	}

	// skip can be number or null
	getLogs(skip = 0, take = this.data.length): Array<LogMessageModel> {
		return this.data.slice(skip, take);
	}

	getLevelCount() {
		const levels = this.data.map((log) => log.level ?? 'unknown');
		const counts = {};
		levels.forEach((level: string) => {
			//eslint-disable-next-line @typescript-eslint/ban-ts-comment
			//@ts-ignore
			counts[level ?? 'unknown'] = (counts[level] || 0) + 1;
		});
		return counts;
	}
}

export const savedSearches: Array<SavedLogSearchModel> = [
	{
		name: 'Find all logs where the Level is NOT Verbose and NOT Debug',
		query: "Not(@Level='Verbose') and Not(@Level='Debug')",
	},
	{
		name: 'Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)',
		query: 'Has(@Exception)',
	},
	{
		name: "Find all logs that have the property 'Duration'",
		query: 'Has(Duration)',
	},
	{
		name: "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
		query: 'Has(Duration) and Duration > 1000',
	},
	{
		name: "Find all logs that are from the namespace 'Umbraco.Core'",
		query: "StartsWith(SourceContext, 'Umbraco.Core')",
	},
	{
		name: 'Find all logs that use a specific log message template',
		query: "@messageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'",
	},
	{
		name: 'Find logs where one of the items in the SortedComponentTypes property array is equal to',
		query: "SortedComponentTypes[?] = 'Umbraco.Web.Search.ExamineComponent'",
	},
	{
		name: 'Find logs where one of the items in the SortedComponentTypes property array contains',
		query: "Contains(SortedComponentTypes[?], 'DatabaseServer')",
	},
	{
		name: 'Find all logs that the message has localhost in it with SQL like',
		query: "@Message like '%localhost%'",
	},
	{
		name: "Find all logs that the message that starts with 'end' in it with SQL like",
		query: "@Message like 'end%'",
	},
	{
		name: 'bla',
		query: 'bla bla',
	},
];

export const messageTemplates: LogTemplateModel[] = [
	{
		messageTemplate: 'Create Foreign Key:\n {Sql}',
		count: 90,
	},
	{
		messageTemplate: 'Create Index:\n {Sql}',
		count: 86,
	},
	{
		messageTemplate: 'Create table:\n {Sql}',
		count: 82,
	},
	{
		messageTemplate: 'Create Primary Key:\n {Sql}',
		count: 78,
	},
	{
		messageTemplate: 'Creating data in {TableName}',
		count: 58,
	},
	{
		messageTemplate: 'Completed creating data in {TableName}',
		count: 58,
	},
	{
		messageTemplate: 'New table {TableName} was created',
		count: 58,
	},
	{
		messageTemplate: 'At {OrigState}',
		count: 18,
	},
	{
		messageTemplate: 'SQL [{ContextIndex}]: {Sql}',
		count: 15,
	},
	{
		messageTemplate: '{StartMessage} [Timing {TimingId}]',
		count: 14,
	},
	{
		messageTemplate: '{EndMessage} ({Duration}ms) [Timing {TimingId}]',
		count: 14,
	},
	{
		messageTemplate: 'Execute {MigrationType}',
		count: 13,
	},
	{
		messageTemplate: "Assigned Deploy permission letter '{Permission}' to user group '{UserGroupAlias}'",
		count: 6,
	},
	{
		messageTemplate: "Starting '{MigrationName}'...",
		count: 5,
	},
	{
		messageTemplate: 'Done (pending scope completion).',
		count: 5,
	},
	{
		messageTemplate:
			"Umbraco Forms scheduled record deletion task will not run as it has been not enabled via configuration. To enable, set the configuration value at 'Umbraco:Forms:Options:ScheduledRecordDeletion:Enabled' to true.",
		count: 5,
	},
	{
		messageTemplate:
			'Mapped Umbraco.Cloud.Deployment.SiteExtension.Messages.External.Git.ApplyChangesFromWwwRootToRepositoryCommand -> "siteext-input-queue"',
		count: 3,
	},
	{
		messageTemplate: 'Bus "Rebus 1" started',
		count: 3,
	},
	{
		messageTemplate: 'Acquiring MainDom.',
		count: 3,
	},
	{
		messageTemplate: 'Acquired MainDom.',
		count: 3,
	},
	{
		messageTemplate: 'Profiler is VoidProfiler, not profiling (must run debug mode to profile).',
		count: 3,
	},
	{
		messageTemplate:
			"Found single permission letter for '{LegacyPermission}' on user group '{UserGroupAlias}', assuming this is the 'Notifications' permission instead of the Deploy 'Queue for transfer' permission",
		count: 3,
	},
	{
		messageTemplate: 'Started :: Running {edition} edition',
		count: 3,
	},
	{
		messageTemplate:
			"File system watcher for deploy events started with filter 'deploy*' and notify filter 'FileName'.",
		count: 3,
	},
	{
		messageTemplate: 'Application started. Press Ctrl+C to shut down.',
		count: 3,
	},
	{
		messageTemplate: 'Hosting environment: {envName}',
		count: 3,
	},
	{
		messageTemplate: 'Content root path: {contentRoot}',
		count: 3,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFRecordDataLongString as a primary key already exists.',
		count: 2,
	},
	{
		messageTemplate:
			'No last synced Id found, this generally means this is a new server/install. A cold boot will be triggered.',
		count: 2,
	},
	{
		messageTemplate: 'Deploy permissions updated and saved',
		count: 2,
	},
	{
		messageTemplate: 'Starting :: Running on Umbraco Cloud',
		count: 2,
	},
	{
		messageTemplate:
			'Registered with MainDom, localContentDbExists? {LocalContentDbExists}, localMediaDbExists? {LocalMediaDbExists}',
		count: 2,
	},
	{
		messageTemplate: 'Creating the content store, localContentDbExists? {LocalContentDbExists}',
		count: 2,
	},
	{
		messageTemplate: 'Creating the media store, localMediaDbExists? {LocalMediaDbExists}',
		count: 2,
	},
	{
		messageTemplate: 'Stopping ({SignalSource})',
		count: 2,
	},
	{
		messageTemplate: 'Released ({SignalSource})',
		count: 2,
	},
	{
		messageTemplate: 'Application is shutting down...',
		count: 2,
	},
	{
		messageTemplate: 'Bus "Rebus 1" stopped',
		count: 2,
	},
	{
		messageTemplate: 'Queued Hosted Service is stopping.',
		count: 2,
	},
	{
		messageTemplate: 'User logged will be logged out due to timeout: {Username}, IP Address: {IPAddress}',
		count: 2,
	},
	{
		messageTemplate: 'Starting unattended install.',
		count: 1,
	},
	{
		messageTemplate: 'Unattended install completed.',
		count: 1,
	},
	{
		messageTemplate: 'Configured with Azure database.',
		count: 1,
	},
	{
		messageTemplate: 'Initialized the SqlServer database schema.',
		count: 1,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFRecordDataBit as a primary key already exists.',
		count: 1,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFRecordDataDateTime as a primary key already exists.',
		count: 1,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFRecordDataInteger as a primary key already exists.',
		count: 1,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFUserFormSecurity as a primary key already exists.',
		count: 1,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFUserGroupSecurity as a primary key already exists.',
		count: 1,
	},
	{
		messageTemplate:
			'Database migration step not completed: could not create primary key constraint on UFUserGroupFormSecurity as a primary key already exists.',
		count: 1,
	},
	{
		messageTemplate:
			'Database NuCache was serialized using {CurrentSerializer}. Currently configured NuCache serializer {Serializer}. Rebuilding Nucache',
		count: 1,
	},
	{
		messageTemplate: 'Starting :: Running locally',
		count: 1,
	},
	{
		messageTemplate: 'No XML encryptor configured. Key {KeyId:B} may be persisted to storage in unencrypted form.',
		count: 1,
	},
	{
		messageTemplate: 'Started :: Transitioned from azure init marker to deploy marker',
		count: 1,
	},
	{
		messageTemplate: 'Found {diskReadTrigger} or {diskReadOnStartTrigger} trigger file when starting, processing...',
		count: 1,
	},
	{
		messageTemplate: 'Beginning deployment {id}.',
		count: 1,
	},
	{
		messageTemplate: 'Suspend scheduled publishing.',
		count: 1,
	},
	{
		messageTemplate: 'Preparing',
		count: 1,
	},
	{
		messageTemplate: 'Reading state',
		count: 1,
	},
	{
		messageTemplate: 'No artifacts',
		count: 1,
	},
	{
		messageTemplate: 'Restore caches and indexes',
		count: 1,
	},
	{
		messageTemplate: 'Resume scheduled publishing.',
		count: 1,
	},
	{
		messageTemplate: 'Complete',
		count: 1,
	},
	{
		messageTemplate: 'Deployment {id} completed.',
		count: 1,
	},
	{
		messageTemplate: 'Work Status {WorkStatus}.',
		count: 1,
	},
	{
		messageTemplate: 'Released from MainDom',
		count: 1,
	},
	{
		messageTemplate: "Keep alive failed (at '{keepAlivePingUrl}').",
		count: 1,
	},
	{
		messageTemplate: 'Adding examine event handlers for {RegisteredIndexers} index providers.',
		count: 1,
	},
	{
		messageTemplate: 'Document {ContentName} (id={ContentId}) has been published.',
		count: 1,
	},
];

export const logLevels = {
	total: 2,
	items: [
		{
			name: 'Global',
			level: 'Information',
		},
		{
			name: 'UmbracoFile',
			level: 'Verbose',
		},
	],
};

export const umbLogviewerData = {
	searches: new UmbLogviewerSearchesData(savedSearches),
	templates: new UmbLogviewerTemplatesData(messageTemplates),
	logs: new UmbLogviewerMessagesData(logs),
	logLevels: logLevels,
};
