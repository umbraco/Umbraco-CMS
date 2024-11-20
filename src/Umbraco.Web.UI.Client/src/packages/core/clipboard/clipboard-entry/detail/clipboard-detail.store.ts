import { UMB_CLIPBOARD_DETAIL_STORE_CONTEXT } from './clipboard-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbClipboardEntry } from './types.js';

/**
 * @class UmbClipboardDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Clipboard Details
 */
export class UmbClipboardDetailStore extends UmbDetailStoreBase<UmbClipboardEntry> {
	/**
	 * Creates an instance of UmbClipboardDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbClipboardDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbClipboardDetailStore;
