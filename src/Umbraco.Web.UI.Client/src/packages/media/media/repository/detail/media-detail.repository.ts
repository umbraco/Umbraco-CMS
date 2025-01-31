import type { UmbMediaDetailModel } from '../../types.js';
import { UmbMediaServerDataSource } from './media-detail.server.data-source.js';
import { UMB_MEDIA_DETAIL_STORE_CONTEXT } from './media-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaDetailRepository extends UmbDetailRepositoryBase<UmbMediaDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaServerDataSource, UMB_MEDIA_DETAIL_STORE_CONTEXT);
	}
}

export { UmbMediaDetailRepository as api };

export default UmbMediaDetailRepository;
