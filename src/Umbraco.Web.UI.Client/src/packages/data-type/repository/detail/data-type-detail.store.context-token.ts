import type { UmbDataTypeDetailStore } from './data-type-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DATA_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDataTypeDetailStore>('UmbDataTypeDetailStore');
