import type { UmbTemplateDetailStore } from './template-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TEMPLATE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbTemplateDetailStore>('UmbTemplateDetailStore');
