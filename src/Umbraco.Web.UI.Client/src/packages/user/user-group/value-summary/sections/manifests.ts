import { UMB_SECTION_ALIASES_VALUE_TYPE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.UserGroup.SectionAliases',
		name: 'Section Aliases Value Summary',
		forValueType: UMB_SECTION_ALIASES_VALUE_TYPE,
		element: () => import('./section-aliases-value-summary.element.js'),
	},
];
