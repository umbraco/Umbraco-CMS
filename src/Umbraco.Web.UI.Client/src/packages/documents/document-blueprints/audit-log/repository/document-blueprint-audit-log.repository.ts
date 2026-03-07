import type { UmbDocumentAuditLogModel } from '../../../documents/audit-log/types.js';
import { UmbDocumentBlueprintAuditLogServerDataSource } from './document-blueprint-audit-log.server.data-source.js';
import type { UmbAuditLogRepository, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentBlueprintAuditLogRepository
	extends UmbRepositoryBase
	implements UmbAuditLogRepository<UmbDocumentAuditLogModel>
{
	#dataSource: UmbDocumentBlueprintAuditLogServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbDocumentBlueprintAuditLogServerDataSource(host);
	}

	async requestAuditLog(args: UmbAuditLogRequestArgs) {
		return this.#dataSource.getAuditLog(args);
	}
}
