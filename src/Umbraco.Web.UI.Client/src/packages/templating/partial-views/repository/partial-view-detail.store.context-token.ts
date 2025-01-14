import type { UmbPartialViewDetailStore } from './partial-view-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbPartialViewDetailStore>(
	'UmbPartialViewDetailStore',
);
