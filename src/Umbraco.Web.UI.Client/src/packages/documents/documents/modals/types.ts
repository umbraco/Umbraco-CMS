import type { UmbDocumentVariantOptionModel } from '../types.js';

export interface UmbDocumentVariantPickerData {
	options: Array<UmbDocumentVariantOptionModel>;
}

export interface UmbDocumentVariantPickerValue {
	selection: Array<string>;
}
