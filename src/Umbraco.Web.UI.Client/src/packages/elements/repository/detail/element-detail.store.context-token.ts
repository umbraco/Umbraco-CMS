import type { UmbElementDetailStore } from './element-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ELEMENT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbElementDetailStore>('UmbElementDetailStore');
