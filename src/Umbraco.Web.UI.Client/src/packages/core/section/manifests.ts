import { UmbSectionUserPermissionCondition } from './conditions/section-user-permission.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Section User Permission Condition',
		alias: 'Umb.Condition.SectionUserPermission',
		api: UmbSectionUserPermissionCondition,
	},
];
