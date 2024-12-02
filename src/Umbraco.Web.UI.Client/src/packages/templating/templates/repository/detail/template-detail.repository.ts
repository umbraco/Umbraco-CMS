import type { UmbTemplateDetailModel } from '../../types.js';
import { UmbTemplateServerDataSource } from './template-detail.server.data-source.js';
import { UMB_TEMPLATE_DETAIL_STORE_CONTEXT } from './template-detail.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateDetailRepository extends UmbDetailRepositoryBase<UmbTemplateDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbTemplateServerDataSource, UMB_TEMPLATE_DETAIL_STORE_CONTEXT);
	}
}

export default UmbTemplateDetailRepository;
