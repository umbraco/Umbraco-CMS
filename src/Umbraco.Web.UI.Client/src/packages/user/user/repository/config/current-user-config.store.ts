import type { UmbCurrentUserConfigurationModel } from '../../types.js';
import { UMB_CURRENT_USER_CONFIG_STORE_CONTEXT } from './current-user-config.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreObjectBase } from '@umbraco-cms/backoffice/store';

export class UmbCurrentUserConfigStore extends UmbStoreObjectBase<UmbCurrentUserConfigurationModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONFIG_STORE_CONTEXT.toString());
	}
}

export default UmbCurrentUserConfigStore;
