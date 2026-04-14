import { UMB_MEDIA_USER_START_NODE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.Media.UserStartNode',
		name: 'Media User Start Node Value Summary',
		forValueType: UMB_MEDIA_USER_START_NODE_VALUE_TYPE,
		element: () => import('./media-user-start-node-value-summary.element.js'),
		valueResolver: () => import('./media-user-start-node-value-summary.resolver.js'),
	},
];
