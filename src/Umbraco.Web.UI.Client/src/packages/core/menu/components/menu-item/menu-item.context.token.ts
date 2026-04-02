import type { UmbDefaultMenuItemContext } from './menu-item.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MENU_ITEM_CONTEXT = new UmbContextToken<UmbDefaultMenuItemContext>('UmbMenuItemContext');
