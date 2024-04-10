import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentBlueprintCreateOptions',
		name: 'Document Blueprint Create Options Modal',
		element: () => import('./modal/document-blueprint-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions];
