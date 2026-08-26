import type { UmbDocumentVariantState } from '../../variant-state.js';

export interface UmbDocumentVariantStateValueModel {
	culture: string | null;
	segment?: string | null;
	state: UmbDocumentVariantState | null;
}

export const UMB_DOCUMENT_VARIANT_STATE_VALUE_TYPE = 'Umb.ValueType.Document.VariantState' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DOCUMENT_VARIANT_STATE_VALUE_TYPE]: Array<UmbDocumentVariantStateValueModel>;
	}
}
