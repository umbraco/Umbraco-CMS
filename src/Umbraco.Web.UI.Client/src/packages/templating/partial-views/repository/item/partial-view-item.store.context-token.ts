import type { UmbPartialViewItemStore } from './partial-view-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT = new UmbContextToken<UmbPartialViewItemStore>(
	'UmbPartialViewItemStore',
);
