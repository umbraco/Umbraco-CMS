import { UmbStaticFileItemServerDataSource } from './static-file-item.server.data-source.js';
import { UMB_STATIC_FILE_ITEM_STORE_CONTEXT } from './static-file-item.store.js';
import type { UmbStaticFileItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbStaticFileItemRepository extends UmbItemRepositoryBase<UmbStaticFileItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStaticFileItemServerDataSource, UMB_STATIC_FILE_ITEM_STORE_CONTEXT);
	}
}

export default UmbStaticFileItemRepository;
