import type { UmbDefaultMenuContext } from './menu.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MENU_CONTEXT = new UmbContextToken<UmbDefaultMenuContext>('UmbMenuContext');
