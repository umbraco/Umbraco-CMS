import type { UmbExtensionTypeItemStore } from './item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_EXTENSION_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbExtensionTypeItemStore>(
	'UmbExtensionTypeItemStore',
);
