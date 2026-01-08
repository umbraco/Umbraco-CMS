import type { UmbTemporaryFileConfigurationModel } from '../types.js';
import { UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT } from './config.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreObjectBase } from '@umbraco-cms/backoffice/store';

export class UmbTemporaryFileConfigStore extends UmbStoreObjectBase<UmbTemporaryFileConfigurationModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT.toString());
	}
}

export default UmbTemporaryFileConfigStore;
