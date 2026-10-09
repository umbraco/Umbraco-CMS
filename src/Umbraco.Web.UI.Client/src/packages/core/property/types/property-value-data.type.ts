export interface UmbPropertyValueData<ValueType = unknown> {
	alias: string;
	value?: ValueType;
}

export interface UmbPropertyValueDataPotentiallyWithEditorAlias<
	ValueType = unknown,
> extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
}

export interface UmbPropertyValueDataWithVariant<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	culture: string | null;
	segment: string | null;
}
