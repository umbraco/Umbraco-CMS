import { UMB_DOCUMENT_VARIANT_STATE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.Document.VariantState',
		name: 'Document Variant State Value Summary',
		forValueType: UMB_DOCUMENT_VARIANT_STATE_VALUE_TYPE,
		element: () => import('./document-variant-state-value-summary.element.js'),
	},
];
