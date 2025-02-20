import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbSectionAliasCondition } from './section-alias.condition.js';
import { UmbSectionUserPermissionCondition } from './section-user-permission.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Section User Permission Condition',
		alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
		api: UmbSectionUserPermissionCondition,
	},
	{
		type: 'condition',
		name: 'Section Alias Condition',
		alias: 'Umb.Condition.SectionAlias',
		api: UmbSectionAliasCondition,
	},
];
