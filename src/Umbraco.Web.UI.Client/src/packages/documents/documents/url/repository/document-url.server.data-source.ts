import type { UmbDocumentUrlsModel } from './types.js';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
		super(host, { getItems, mapper });
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => DocumentService.getDocumentUrls({ query: { id: uniques } });

const mapper = (item: DocumentUrlInfoResponseModel): UmbDocumentUrlsModel => ({ unique: item.id, urls: item.urlInfos });
