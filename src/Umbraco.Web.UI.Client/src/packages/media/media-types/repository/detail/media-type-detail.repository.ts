import type { UmbMediaTypeDetailModel } from '../../types.js';
import { UmbMediaTypeServerDataSource } from './media-type-detail.server.data-source.js';
import { UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT } from './media-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbMediaTypeDetailRepository extends UmbDetailRepositoryBase<UmbMediaTypeDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeServerDataSource, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT);
	}
}

export default UmbMediaTypeDetailRepository;
