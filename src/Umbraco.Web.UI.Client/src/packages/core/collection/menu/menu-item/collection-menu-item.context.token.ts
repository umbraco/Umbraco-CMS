import type { UmbCollectionMenuItemContext } from './collection-menu-item-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_COLLECTION_MENU_ITEM_CONTEXT = new UmbContextToken<UmbCollectionMenuItemContext>(
	'UmbCollectionMenuItemContext',
);
