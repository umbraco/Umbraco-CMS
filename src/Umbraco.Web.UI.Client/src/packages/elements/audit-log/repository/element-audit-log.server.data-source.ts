import type { UmbElementAuditLogModel } from '../types.js';
//import type { UmbElementAuditLogType } from '../utils/index.js';
import type { UmbAuditLogDataSource, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
//import type { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
//import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
//import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for the element audit log
 * @class UmbAuditLogServerDataSource
 */
export class UmbElementAuditLogServerDataSource implements UmbAuditLogDataSource<UmbElementAuditLogModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbAuditLogServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbAuditLogServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the audit log for a element
	 * @param {UmbAuditLogRequestArgs} args The request arguments
	 * @returns {UmbDataSourceResponse<UmbPagedModel<UmbElementAuditLogModel>>} The data source response containing a paged model of audit log items
	 * @memberof UmbElementAuditLogServerDataSource
	 */
	async getAuditLog(args: UmbAuditLogRequestArgs) {
		console.error('Not implemented yet: UmbElementAuditLogServerDataSource.getAuditLog', [this.#host, args]);
		return { data: { items: [], total: -1 } };
		// const { data, error } = await tryExecute(
		// 	this.#host,
		// 	ElementService.getElementByIdAuditLog({
		// 		path: { id: args.unique },
		// 		query: {
		// 			orderDirection: args.orderDirection as DirectionModel, // TODO: Fix type cast
		// 			sinceDate: args.sinceDate,
		// 			skip: args.skip,
		// 			take: args.take,
		// 		},
		// 	}),
		// );

		// if (data) {
		// 	const mappedItems: Array<UmbElementAuditLogModel> = data.items.map((item) => {
		// 		return {
		// 			user: { unique: item.user.id },
		// 			timestamp: item.timestamp,
		// 			logType: item.logType as UmbElementAuditLogType, // TODO: Fix type cast
		// 			comment: item.comment,
		// 			parameters: item.parameters,
		// 		};
		// 	});

		// 	return { data: { items: mappedItems, total: data.total } };
		// }

		// return { error };
	}
}
