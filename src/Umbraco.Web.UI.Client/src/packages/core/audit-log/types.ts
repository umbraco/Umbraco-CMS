import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';

export interface UmbAuditLogModel<LogType = any> {
	user: UmbReferenceByUnique;
	timestamp: string;
	logType: LogType;
	comment?: string | null;
	parameters?: string | null;
}

export interface UmbAuditLogRequestArgs {
	unique: string;
	orderDirection?: UmbDirectionType;
	sinceDate?: string;
	skip?: number;
	take?: number;
}
