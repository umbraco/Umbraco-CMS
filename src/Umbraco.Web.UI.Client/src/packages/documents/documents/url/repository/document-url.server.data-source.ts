import type { UmbDocumentUrlsModel } from './types.js';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A server data source for Document URLs
 * @class UmbDocumentUrlServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentUrlServerDataSource extends UmbItemServerDataSourceBase<
	DocumentUrlInfoResponseModel,
	UmbDocumentUrlsModel
> {
	/**
	 * Creates an instance of UmbDocumentUrlServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentUrlServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, { mapper });
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => DocumentService.getDocumentUrls({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: DocumentUrlInfoResponseModel): UmbDocumentUrlsModel => ({ unique: item.id, urls: item.urlInfos });
