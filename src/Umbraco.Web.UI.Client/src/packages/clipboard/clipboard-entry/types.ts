import type { UmbClipboardEntryItemModel } from './item/types.js';

export interface UmbClipboardEntryValueModel<ValueType = any> {
	type: string;
	value: ValueType;
}

export type UmbClipboardEntryValuesType = Array<UmbClipboardEntryValueModel>;

/**
 * A Clipboard entry is a composed set of data representing one entry in the clipboard.
 * The entry has enough knowledge for the context of the clipboard to filter away unsupported entries.
 */
export interface UmbClipboardEntryDetailModel<MetaType = object> extends UmbClipboardEntryItemModel<MetaType> {
	/**
	 * The values of the clipboard entry.
	 */
	values: UmbClipboardEntryValuesType;
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
