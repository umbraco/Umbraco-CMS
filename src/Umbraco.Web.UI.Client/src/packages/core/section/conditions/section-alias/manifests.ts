import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from './constants.js';
import { UmbSectionAliasCondition } from './section-alias.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Section Alias Condition',
		alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
		api: UmbSectionAliasCondition,
	},
];
