import type { UmbClipboardEntryEntityType } from '../entity.js';

export interface UmbClipboardEntryItemModel<MetaType = object> {
	entityType: UmbClipboardEntryEntityType;

	/**
	 * A unique identifier, ensures that this clipboard entry will be replaced if it gets copied later.
	 */
	unique: string;
	/**
	 * The name of this clipboard entry.
	 */
	name: string | null;
	/**
	 * The icon of the clipboard entry.
	 */
	icon: string | null;
	/**
	 * The aliases of the content-types of these entries.
	 */
	meta: MetaType;

	/**
	 * The date the clipboard entry was created.
	 */
	createDate: string | null;

	/**
	 * The date the clipboard entry was last updated.
	 */
	updateDate: string | null;
}
