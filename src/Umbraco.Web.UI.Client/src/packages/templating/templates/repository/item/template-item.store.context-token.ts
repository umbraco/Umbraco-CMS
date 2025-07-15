import type { UmbTemplateItemStore } from './template-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TEMPLATE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbTemplateItemStore>('UmbTemplateItemStore');
