import type { UmbScriptItemModel } from '../../types.js';
import { UmbScriptItemServerDataSource } from './script-item.server.data-source.js';
import { UMB_SCRIPT_ITEM_STORE_CONTEXT } from './script-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbScriptItemRepository extends UmbItemRepositoryBase<UmbScriptItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptItemServerDataSource, UMB_SCRIPT_ITEM_STORE_CONTEXT);
	}
}

export default UmbScriptItemRepository;
