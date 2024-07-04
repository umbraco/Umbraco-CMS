import { UmbMediaItemServerDataSource } from './media-item.server.data-source.js';
import { UMB_MEDIA_ITEM_STORE_CONTEXT } from './media-item.store.context-token.js';
import type { UmbMediaItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaItemRepository extends UmbItemRepositoryBase<UmbMediaItemModel> {
	#dataSource: UmbMediaItemServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host, UmbMediaItemServerDataSource, UMB_MEDIA_ITEM_STORE_CONTEXT);
		this.#dataSource = new UmbMediaItemServerDataSource(this);
	}

	async search({ query, skip, take }: { query: string; skip: number; take: number }) {
		return this.#dataSource.search({ query, skip, take });
	}
}

export default UmbMediaItemRepository;
