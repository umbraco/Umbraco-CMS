import type { UmbDocumentDetailModel } from '../types.js';
import type { UmbDocumentCollectionFilterModel } from './types.js';
import { UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './views/index.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionContext extends UmbDefaultCollectionContext<
	UmbDocumentDetailModel,
	UmbDocumentCollectionFilterModel
> {
	constructor(host: UmbControllerHostElement) {
		super(host, { pageSize: 5, defaultViewAlias: UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS });

		this.selection.setSelectable(true);
	}
}
