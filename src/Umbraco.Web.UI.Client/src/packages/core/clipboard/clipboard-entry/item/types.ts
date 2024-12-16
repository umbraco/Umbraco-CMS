import type { UmbClipboardEntryEntityType } from '../entity.js';

export interface UmbClipboardEntryItemModel<Type extends string = string, MetaType = object> {
	entityType: UmbClipboardEntryEntityType;
	/**
	 * The type of clipboard entry, this determines the data type of the entry. Making the entry as general as possible.
	 * Example a entry from a Block Editor, gets a generic type called 'block'. Making it able to copy/paste between different Block Editors.
	 */
	type: Type;
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
}
