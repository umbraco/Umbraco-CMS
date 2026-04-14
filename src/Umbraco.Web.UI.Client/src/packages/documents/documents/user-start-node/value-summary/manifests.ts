import { UMB_DOCUMENT_USER_START_NODE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.Document.UserStartNode',
		name: 'Document User Start Node Value Summary',
		forValueType: UMB_DOCUMENT_USER_START_NODE_VALUE_TYPE,
		element: () => import('./document-user-start-node-value-summary.element.js'),
		valueResolver: () => import('./document-user-start-node-value-summary.resolver.js'),
	},
];
