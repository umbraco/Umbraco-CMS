import type { UmbUploadableItem } from './types.js';

export class UmbDropzoneChangeEvent extends Event {
	public static readonly TYPE = 'change';

	/**
	 * An array of resolved uploadable items.
	 */
	public items;

	public constructor(items: Array<UmbUploadableItem>, args?: EventInit) {
		super(UmbDropzoneChangeEvent.TYPE, { bubbles: false, composed: false, cancelable: false, ...args });
		this.items = items;
	}
}
