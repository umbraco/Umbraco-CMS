import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ROLLBACK_MODAL_ALIAS = 'Umb.Modal.Rollback';
export const UMB_DOCUMENT_SAVE_MODAL_ALIAS = 'Umb.Modal.DocumentSave';
export const UMB_DOCUMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentPublish';
export const UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentUnpublish';
export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';
export const UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS = 'Umb.Modal.DocumentPublishWithDescendants';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_ROLLBACK_MODAL_ALIAS,
		name: 'Rollback Modal',
		js: () => import('./rollback/rollback-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_SAVE_MODAL_ALIAS,
		name: 'Document Save Modal',
		js: () => import('./save-modal/document-save-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
		name: 'Document Publish Modal',
		js: () => import('./publish-modal/document-publish-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS,
		name: 'Document Unpublish Modal',
		js: () => import('./unpublish-modal/document-unpublish-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS,
		name: 'Document Schedule Modal',
		js: () => import('./schedule-modal/document-schedule-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS,
		name: 'Document Publish With Descendants Modal',
		js: () => import('./publish-with-descendants-modal/document-publish-with-descendants-modal.element.js'),
	},
];

export const manifests: Array<UmbExtensionManifest> = [...modals];
