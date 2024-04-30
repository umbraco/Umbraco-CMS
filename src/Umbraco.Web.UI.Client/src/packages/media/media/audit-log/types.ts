import type { AuditTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';

export interface UmbAuditLogRequestArgs {
	unique: string;
	orderDirection?: UmbDirectionType;
	sinceDate?: string;
	skip?: number;
	take?: number;
}

export interface UmbMediaAuditLogModel {
	user: UmbReferenceByUnique;
	timestamp: string;
	logType: AuditTypeModel;
	comment?: string | null;
	parameters?: string | null;
}
