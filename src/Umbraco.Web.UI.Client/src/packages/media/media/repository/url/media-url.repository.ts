import type { UmbMediaUrlModel } from './types.js';
import { UMB_MEDIA_URL_STORE_CONTEXT } from './media-url.store.context-token.js';
import { UmbMediaUrlServerDataSource } from './media-url.server.data-source.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaUrlRepository extends UmbItemRepositoryBase<UmbMediaUrlModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaUrlServerDataSource, UMB_MEDIA_URL_STORE_CONTEXT);
	}
}

export { UmbMediaUrlRepository as api };

export default UmbMediaUrlRepository;
