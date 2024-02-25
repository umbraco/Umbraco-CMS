import { UMB_SCRIPT_DETAIL_STORE_CONTEXT } from '../../repository/index.js';
import type { UmbScriptDetailModel } from '../../types.js';
import { UmbRenameScriptServerDataSource } from './rename-script.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRenameRepositoryBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbRenameScriptRepository extends UmbRenameRepositoryBase<UmbScriptDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbRenameScriptServerDataSource, UMB_SCRIPT_DETAIL_STORE_CONTEXT);
	}
}

export default UmbRenameScriptRepository;
