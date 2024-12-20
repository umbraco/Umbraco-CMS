import type { UmbClipboardEntryItemModel } from './item/types.js';

export type * from './translator/types.js';

/**
 * A Clipboard entry is a composed set of data representing one entry in the clipboard.
 * The entry has enough knowledge for the context of the clipboard to filter away unsupported entries.
 */
export interface UmbClipboardEntryDetailModel<Type extends string = string, MetaType = object, ValueType = unknown>
	extends UmbClipboardEntryItemModel<Type, MetaType> {
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
