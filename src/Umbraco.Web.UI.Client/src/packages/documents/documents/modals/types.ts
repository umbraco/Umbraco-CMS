import type { UmbDocumentVariantOptionModel } from '../types.js';

export interface UmbDocumentVariantPickerData {
	options: Array<UmbDocumentVariantOptionModel>;
	pickableFilter?: (variantOption: UmbDocumentVariantOptionModel) => boolean;
}

export interface UmbDocumentVariantPickerValue {
	selection: Array<string>;
}
