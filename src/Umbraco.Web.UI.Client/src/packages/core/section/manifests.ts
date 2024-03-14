import { UmbSectionUserPermissionCondition } from './conditions/section-user-permission.condition.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		element: () => import('./section-picker-modal/section-picker-modal.element.js'),
	},
	{
		type: 'condition',
		name: 'Section User Permission Condition',
		alias: 'Umb.Condition.SectionUserPermission',
		api: UmbSectionUserPermissionCondition,
	},
	...repositoryManifests,
];
