import type { UmbPropertyEditorDataSourceItemStore } from './item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_CONTEXT =
	new UmbContextToken<UmbPropertyEditorDataSourceItemStore>('UmbPropertyEditorDataSourceItemStore');
