import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaCaptionAltText',
		name: 'Media Caption Alt Text',
		js: () => import('./media-caption-alt-text/media-caption-alt-text-modal.element.js'),
	},
];

export const manifests = [...modals];
