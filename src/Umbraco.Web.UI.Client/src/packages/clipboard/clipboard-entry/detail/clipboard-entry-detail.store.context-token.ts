import type { UmbClipboardEntryDetailStore } from './clipboard-entry-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CLIPBOARD_ENTRY_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbClipboardEntryDetailStore>(
	'UmbClipboardEntryDetailStore',
);
