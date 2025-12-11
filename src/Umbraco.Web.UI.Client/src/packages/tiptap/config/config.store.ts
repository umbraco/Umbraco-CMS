import type { UmbTiptapServerConfigurationModel } from './types.js';
import { UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_CONTEXT } from './config.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreObjectBase } from '@umbraco-cms/backoffice/store';

export class UmbTiptapUmbracoPathConfigStore extends UmbStoreObjectBase<UmbTiptapServerConfigurationModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_CONTEXT.toString());
	}
}

export default UmbTiptapUmbracoPathConfigStore;
