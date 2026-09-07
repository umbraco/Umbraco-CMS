import { UmbSearchDetailStore } from './search-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SEARCH_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbSearchDetailStore>(
  'UmbSearchDetailStore',
);
