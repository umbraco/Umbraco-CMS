import type { UmbClipboardEntryEntityType } from './entity.js';

/**
 * A Clipboard entry is a composed set of data representing one entry in the clipboard.
 * The entry has enough knowledge for the context of the clipboard to filter away unsupported entries.
 */
export interface UmbClipboardEntryDetailModel<Type extends string = string, MetaType = object, ValueType = unknown> {
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
	name: string;
	/**
	 * The icons of the copied pieces for this clipboard entry.
	 */
	icons: Array<string>;
	/**
	 * The aliases of the content-types of these entries.
	 */
	meta: MetaType;
	/**
	 * The value of the clipboard entry.
	 */
	value: ValueType;
}

/**
 * @deprecated
 * @see UmbClipboardEntryDetailModel
 */
export interface UmbClipboardEntry<Type extends string = string, MetaType = object, DataType = unknown> {
	type: Type;
	unique: string;
	name: string;
	icons: Array<string>;
	meta: MetaType;
	data: Array<DataType>;
}
