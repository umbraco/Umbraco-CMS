import { UmbSectionUserPermissionCondition } from './conditions/section-user-permission.condition.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'Section User Permission Condition',
		alias: 'Umb.Condition.SectionUserPermission',
		api: UmbSectionUserPermissionCondition,
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		element: () => import('./section-picker/section-picker-modal.element.js'),
	},
];
