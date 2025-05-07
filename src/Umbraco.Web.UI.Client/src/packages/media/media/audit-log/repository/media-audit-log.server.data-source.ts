import type { UmbMediaAuditLogModel } from '../types.js';
import type { UmbMediaAuditLogType } from '../utils/index.js';
import type { UmbAuditLogDataSource, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for the Media audit log
 * @class UmbAuditLogServerDataSource
 */
export class UmbMediaAuditLogServerDataSource implements UmbAuditLogDataSource<UmbMediaAuditLogModel> {
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
	 * Get the audit log for a Media
	 * @param {UmbAuditLogRequestArgs} args
	 * @returns {*}
	 * @memberof UmbMediaAuditLogServerDataSource
	 */
	async getAuditLog(args: UmbAuditLogRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			MediaService.getMediaByIdAuditLog({
				path: { id: args.unique },
				query: {
					orderDirection: args.orderDirection as DirectionModel, // TODO: fix this type cast
					sinceDate: args.sinceDate,
					skip: args.skip,
					take: args.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbMediaAuditLogModel> = data.items.map((item) => {
				return {
					user: { unique: item.user.id },
					timestamp: item.timestamp,
					logType: item.logType as UmbMediaAuditLogType, // TODO: fix this type cast
					comment: item.comment,
					parameters: item.parameters,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
