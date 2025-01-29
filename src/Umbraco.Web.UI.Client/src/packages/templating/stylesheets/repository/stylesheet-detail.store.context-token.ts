import type { UmbStylesheetDetailStore } from './stylesheet-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_STYLESHEET_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbStylesheetDetailStore>(
	'UmbStylesheetDetailStore',
);
