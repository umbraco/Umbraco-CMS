import { UMB_CLIPBOARD_ENTRY_ITEM_STORE_CONTEXT } from './clipboard-entry-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbClipboardEntry } from '../../types.js';

/**
 * @class UmbClipboardEntryItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Clipboard Entry items
 */

export class UmbClipboardEntryItemStore extends UmbItemStoreBase<UmbClipboardEntry> {
	/**
	 * Creates an instance of UmbClipboardEntryItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbClipboardEntryItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_ENTRY_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbClipboardEntryItemStore as api };
