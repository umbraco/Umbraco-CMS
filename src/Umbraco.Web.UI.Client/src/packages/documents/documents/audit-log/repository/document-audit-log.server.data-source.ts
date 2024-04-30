import type { UmbAuditLogRequestArgs } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * Server data source for the document audit log
 * @export
 * @class UmbAuditLogServerDataSource
 */
export class UmbDocumentAuditLogServerDataSource {
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
	 * Get the audit log for a document
	 * @param {UmbAuditLogRequestArgs} args
	 * @return {*}
	 * @memberof UmbDocumentAuditLogServerDataSource
	 */
	async getAuditLog(args: UmbAuditLogRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentService.getDocumentByIdAuditLog({
				id: args.unique,
				orderDirection: args.orderDirection,
				sinceDate: args.sinceDate,
				skip: args.skip,
				take: args.take,
			}),
		);

		if (data) {
			const mappedItems = data.items.map((item) => {
				return {
					user: item.user ? { unique: item.user.id } : null,
					timestamp: item.timestamp,
					logType: item.logType,
					comment: item.comment,
					parameters: item.parameters,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
