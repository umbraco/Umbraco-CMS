import type { UmbClipboardEntryItemStore } from './clipboard-entry-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CLIPBOARD_ENTRY_ITEM_STORE_CONTEXT = new UmbContextToken<UmbClipboardEntryItemStore>(
	'UmbClipboardEntryItemStore',
);
