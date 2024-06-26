import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbTemplateDetailStore } from './template-detail.store.js';

export const UMB_TEMPLATE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbTemplateDetailStore>('UmbTemplateDetailStore');
