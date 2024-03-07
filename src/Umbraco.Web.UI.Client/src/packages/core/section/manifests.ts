import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests = [
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		js: () => import('./section-picker-modal/section-picker-modal.element.js'),
	},
	...repositoryManifests,
];
