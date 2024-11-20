import type { UmbClipboardEntry } from '../types.js';
import { UmbClipboardEntryItemLocalStorageDataSource } from './clipboard-entry-item.local-storage.data-source.js';
import { UMB_CLIPBOARD_ENTRY_ITEM_STORE_CONTEXT } from './clipboard-entry-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbClipboardEntryItemRepository extends UmbItemRepositoryBase<UmbClipboardEntry> {
	constructor(host: UmbControllerHost) {
		super(host, UmbClipboardEntryItemLocalStorageDataSource, UMB_CLIPBOARD_ENTRY_ITEM_STORE_CONTEXT);
	}
}

export { UmbClipboardEntryItemRepository as api };
