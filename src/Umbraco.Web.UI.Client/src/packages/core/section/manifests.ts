import { manifests as sectionUserPermissionConditionManifests } from './conditions/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		element: () => import('./section-picker-modal/section-picker-modal.element.js'),
	},
	...sectionUserPermissionConditionManifests,
	...repositoryManifests,
];
