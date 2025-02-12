import { UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from '../../repository/index.js';
import type { UmbStylesheetDetailModel } from '../../types.js';
import { UmbRenameStylesheetServerDataSource } from './rename-stylesheet.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRenameServerFileRepositoryBase } from '@umbraco-cms/backoffice/server-file-system';

export class UmbRenameStylesheetRepository extends UmbRenameServerFileRepositoryBase<UmbStylesheetDetailModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UmbRenameStylesheetServerDataSource, UMB_STYLESHEET_DETAIL_STORE_CONTEXT);
	}
}

export default UmbRenameStylesheetRepository;
