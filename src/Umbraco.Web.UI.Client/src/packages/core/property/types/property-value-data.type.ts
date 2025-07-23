export interface UmbPropertyValueData<ValueType = unknown> {
	alias: string;
	value?: ValueType;
}

export interface UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType = unknown>
	extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
}
