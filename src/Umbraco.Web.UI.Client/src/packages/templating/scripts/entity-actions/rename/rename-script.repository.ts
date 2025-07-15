import { UMB_SCRIPT_DETAIL_STORE_CONTEXT } from '../../repository/constants.js';
import type { UmbScriptDetailModel } from '../../types.js';
import { UmbRenameScriptServerDataSource } from './rename-script.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRenameServerFileRepositoryBase } from '@umbraco-cms/backoffice/server-file-system';

export class UmbRenameScriptRepository extends UmbRenameServerFileRepositoryBase<UmbScriptDetailModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UmbRenameScriptServerDataSource, UMB_SCRIPT_DETAIL_STORE_CONTEXT);
	}
}

export default UmbRenameScriptRepository;
