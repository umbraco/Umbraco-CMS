import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentPublish';
export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
		name: 'Document Publish Modal',
		js: () => import('./publish-modal/document-publish-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS,
		name: 'Document Schedule Modal',
		js: () => import('./schedule-modal/document-schedule-modal.element.js'),
	},
];

export const manifests = [...modals];
