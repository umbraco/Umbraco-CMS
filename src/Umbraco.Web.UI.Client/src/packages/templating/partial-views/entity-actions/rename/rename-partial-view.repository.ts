import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from '../../repository/partial-view-detail.store.js';
import type { UmbPartialViewDetailModel } from '../../types.js';
import { UmbRenamePartialViewServerDataSource } from './rename-partial-view.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRenameRepositoryBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbRenamePartialViewRepository extends UmbRenameRepositoryBase<UmbPartialViewDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbRenamePartialViewServerDataSource, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT);
	}
}

export default UmbRenamePartialViewRepository;
