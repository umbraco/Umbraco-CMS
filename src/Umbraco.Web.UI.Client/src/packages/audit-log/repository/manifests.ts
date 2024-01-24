import { UmbAuditLogRepository } from './audit-log.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_AUDIT_LOG_REPOSITORY_ALIAS = 'Umb.Repository.AuditLog';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_AUDIT_LOG_REPOSITORY_ALIAS,
	name: 'AuditLog Repository',
	api: UmbAuditLogRepository,
};

export const manifests = [repository];
