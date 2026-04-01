import type { UmbDocumentBlueprintAuditLogModel, UmbDocumentBlueprintAuditLogType } from './types.js';
import type { UmbAuditLogDataSource, UmbAuditLogRequestArgs } from '@umbraco-cms/backoffice/audit-log';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentBlueprintAuditLogServerDataSource
	implements UmbAuditLogDataSource<UmbDocumentBlueprintAuditLogModel>
{
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getAuditLog(args: UmbAuditLogRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.getDocumentBlueprintByIdAuditLog({
				path: { id: args.unique },
				query: {
					orderDirection: args.orderDirection as DirectionModel,
					sinceDate: args.sinceDate,
					skip: args.skip,
					take: args.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbDocumentBlueprintAuditLogModel> = (data.items ?? []).map((item) => {
				return {
					user: { unique: item.user.id },
					timestamp: item.timestamp,
					logType: item.logType as UmbDocumentBlueprintAuditLogType,
					comment: item.comment,
					parameters: item.parameters,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
