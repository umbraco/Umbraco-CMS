export interface UmbContentVariantPickerData<VariantOptionModelType> {
	options: Array<VariantOptionModelType>;
	pickableFilter?: (variantOption: VariantOptionModelType) => boolean;
}

export interface UmbContentVariantPickerValue {
	selection: Array<string>;
}
