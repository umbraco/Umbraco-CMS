import { UmbDiscardChangesModalElement } from './discard-changes-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DiscardChanges',
		name: 'Discard Changes Modal',
		element: UmbDiscardChangesModalElement,
	},
];
