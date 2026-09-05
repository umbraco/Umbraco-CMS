import { UMB_SECTION_ALIASES_VALUE_TYPE } from '../value-type/constants.js';
import { UmbSectionAliasesValueSummaryElement } from './section-aliases-value-summary.element.js';
import { UmbSectionAliasesValueSummaryResolver } from './section-aliases-value-summary.resolver.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.Section.Aliases',
		name: 'Section Aliases Value Summary',
		forValueType: UMB_SECTION_ALIASES_VALUE_TYPE,
		element: UmbSectionAliasesValueSummaryElement,
		valueResolver: UmbSectionAliasesValueSummaryResolver,
	},
];
