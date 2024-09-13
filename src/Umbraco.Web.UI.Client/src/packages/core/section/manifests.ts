import { manifest as sectionUserPermissionConditionManifest } from './conditions/section-user-permission.condition.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		element: () => import('./section-picker-modal/section-picker-modal.element.js'),
	},
	sectionUserPermissionConditionManifest,
	...repositoryManifests,
];
