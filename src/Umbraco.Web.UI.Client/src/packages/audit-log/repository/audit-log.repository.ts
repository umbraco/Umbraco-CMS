import { UmbAuditLogServerDataSource } from './audit-log.server.data.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { AuditTypeModel, DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbAuditLogRepository extends UmbControllerBase {
	#dataSource: UmbAuditLogServerDataSource;
	#notificationService?: UmbNotificationContext;
	#init;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.#dataSource = new UmbAuditLogServerDataSource(host);

		this.#init = new UmbContextConsumerController(host, UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationService = instance;
		}).asPromise();
	}

	async getLog({
		orderDirection,
		sinceDate,
		skip = 0,
		take = 100,
	}: {
		orderDirection?: DirectionModel;
		sinceDate?: string;
		skip?: number;
		take?: number;
	}) {
		await this.#init;

		return this.#dataSource.getAuditLog({ orderDirection, sinceDate, skip, take });
	}

	async getAuditLogByUnique({
		id,
		orderDirection,
		skip = 0,
		take = 100,
	}: {
		id: string;
		orderDirection?: DirectionModel;
		skip?: number;
		take?: number;
	}) {
		await this.#init;
		return this.#dataSource.getAuditLogById({ id, orderDirection, skip, take });
	}

	async getAuditLogTypeByLogType({
		logType,
		sinceDate,
		skip,
		take,
	}: {
		logType: AuditTypeModel;
		sinceDate?: string;
		skip?: number;
		take?: number;
	}) {
		await this.#init;
		return this.#dataSource.getAuditLogTypeByLogType({ logType, sinceDate, skip, take });
	}
}
