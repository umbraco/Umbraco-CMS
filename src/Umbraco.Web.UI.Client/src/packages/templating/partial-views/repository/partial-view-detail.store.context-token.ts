import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbPartialViewDetailStore } from './partial-view-detail.store.js';

export const UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbPartialViewDetailStore>(
	'UmbPartialViewDetailStore',
);
