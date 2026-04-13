import { UMB_SECTION_ALIASES_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.Section.Aliases',
		name: 'Section Aliases Value Summary',
		forValueType: UMB_SECTION_ALIASES_VALUE_TYPE,
		element: () => import('./section-aliases-value-summary.element.js'),
		resolver: () => import('./section-aliases-value-summary.resolver.js'),
	},
];
