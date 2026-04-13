import { UMB_DOCUMENT_START_NODE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.Document.StartNode',
		name: 'Document Start Node Value Summary',
		forValueType: UMB_DOCUMENT_START_NODE_VALUE_TYPE,
		element: () => import('./document-start-node-value-summary.element.js'),
		resolver: () => import('./document-start-node-value-summary.resolver.js'),
	},
];
