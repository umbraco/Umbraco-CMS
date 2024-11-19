import type { UmbClipboardDetailStore } from './clipboard-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CLIPBOARD_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbClipboardDetailStore>(
	'UmbLanguageDetailStore',
);
