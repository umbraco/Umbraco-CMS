import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from '../../repository/constants.js';
import type { UmbPartialViewDetailModel } from '../../types.js';
import { UmbRenamePartialViewServerDataSource } from './rename-partial-view.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRenameServerFileRepositoryBase } from '@umbraco-cms/backoffice/server-file-system';

export class UmbRenamePartialViewRepository extends UmbRenameServerFileRepositoryBase<UmbPartialViewDetailModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UmbRenamePartialViewServerDataSource, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT);
	}
}

export default UmbRenamePartialViewRepository;
