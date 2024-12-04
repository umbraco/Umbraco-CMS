import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS = 'Umb.Modal.DocumentSchedule';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS,
	name: 'Document Schedule Modal',
	element: () => import('./document-schedule-modal.element.js'),
};
