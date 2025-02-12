import type { UmbUploadableItem } from './types.js';

export class UmbDropzoneSubmittedEvent extends Event {
	public static readonly TYPE = 'submitted';

	/**
	 * An array of resolved uploadable items.
	 */
	public items;

	public constructor(items: Array<UmbUploadableItem>, args?: EventInit) {
		super(UmbDropzoneSubmittedEvent.TYPE, { bubbles: false, composed: false, cancelable: false, ...args });
		this.items = items;
	}
}
