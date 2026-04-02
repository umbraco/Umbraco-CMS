import type { UmbDefaultCollectionMenuContext } from './default-collection-menu.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_COLLECTION_MENU_CONTEXT = new UmbContextToken<UmbDefaultCollectionMenuContext>(
	'UmbCollectionMenuContext',
);
