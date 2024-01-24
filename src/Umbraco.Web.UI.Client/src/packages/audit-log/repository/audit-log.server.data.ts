import { AuditLogResource, DirectionModel, AuditTypeModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbAuditLogServerDataSource
 */
export class UmbAuditLogServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbAuditLogServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbAuditLogServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbAuditLogServerDataSource
	 */
	async getAuditLog({
		orderDirection,
		sinceDate,
		skip,
		take,
	}: {
		orderDirection?: DirectionModel;
		sinceDate?: string;
		skip?: number;
		take?: number;
	}) {
		return await tryExecuteAndNotify(
			this.#host,
			AuditLogResource.getAuditLog({ orderDirection, sinceDate, skip, take }),
		);
	}

	async getAuditLogById({
		id,
		orderDirection,
		sinceDate,
		skip,
		take,
	}: {
		id: string;
		orderDirection?: DirectionModel;
		sinceDate?: string;
		skip?: number;
		take?: number;
	}) {
		return await tryExecuteAndNotify(
			this.#host,
			AuditLogResource.getAuditLogById({ id, orderDirection, sinceDate, skip, take }),
		);
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
		return await tryExecuteAndNotify(
			this.#host,
			AuditLogResource.getAuditLogTypeByLogType({ logType, sinceDate, skip, take }),
		);
	}
}
