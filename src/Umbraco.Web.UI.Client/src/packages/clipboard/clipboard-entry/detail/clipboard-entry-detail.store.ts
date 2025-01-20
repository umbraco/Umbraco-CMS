import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UMB_CLIPBOARD_ENTRY_DETAIL_STORE_CONTEXT } from './clipboard-entry-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbClipboardEntryDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Clipboard Details
 */
export class UmbClipboardEntryDetailStore extends UmbDetailStoreBase<UmbClipboardEntryDetailModel> {
	/**
	 * Creates an instance of UmbClipboardEntryDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbClipboardEntryDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_ENTRY_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbClipboardEntryDetailStore;
