import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbTemplateItemStore } from './template-item.store.js';

export const UMB_TEMPLATE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbTemplateItemStore>('UmbTemplateItemStore');
