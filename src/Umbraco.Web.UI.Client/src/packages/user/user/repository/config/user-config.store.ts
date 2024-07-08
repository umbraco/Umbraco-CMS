import type { UmbUserConfigurationModel } from '../../types.js';
import { UMB_USER_CONFIG_STORE_CONTEXT } from './user-config.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreObjectBase } from '@umbraco-cms/backoffice/store';

export class UmbUserConfigStore extends UmbStoreObjectBase<UmbUserConfigurationModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_CONFIG_STORE_CONTEXT.toString());
	}
}

export default UmbUserConfigStore;
