import { UMB_MEDIA_START_NODE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		alias: 'Umb.ValueSummary.Media.StartNode',
		name: 'Media Start Node Value Summary',
		forValueType: UMB_MEDIA_START_NODE_VALUE_TYPE,
		element: () => import('./media-start-node-value-summary.element.js'),
		resolver: () => import('./media-start-node-value-summary.resolver.js'),
	},
];
