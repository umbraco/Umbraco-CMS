import type { UmbDocumentAuditLogModel } from '../types.js';
import type { UmbDocumentAuditLogType } from '../utils/index.js';
import type { UmbAuditLogDataSource, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for the document audit log
 * @class UmbAuditLogServerDataSource
 */
export class UmbDocumentAuditLogServerDataSource implements UmbAuditLogDataSource<UmbDocumentAuditLogModel> {
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
	 * Get the audit log for a document
	 * @param {UmbAuditLogRequestArgs} args
	 * @returns {*}
	 * @memberof UmbDocumentAuditLogServerDataSource
	 */
	async getAuditLog(args: UmbAuditLogRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getDocumentByIdAuditLog({
				path: { id: args.unique },
				query: {
					orderDirection: args.orderDirection as DirectionModel, // TODO: Fix type cast
					sinceDate: args.sinceDate,
					skip: args.skip,
					take: args.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDocumentAuditLogModel> = data.items.map((item) => {
				return {
					user: { unique: item.user.id },
					timestamp: item.timestamp,
					logType: item.logType as UmbDocumentAuditLogType, // TODO: Fix type cast
					comment: item.comment,
					parameters: item.parameters,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
