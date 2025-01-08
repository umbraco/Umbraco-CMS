import type { UmbDocumentVariantModel } from '../types.js';

export * from './conditions/constants.js';
export * from './document-workspace.context-token.js';

export const UMB_DOCUMENT_WORKSPACE_ALIAS = 'Umb.Workspace.Document';

export const UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD: UmbDocumentVariantModel = {
	culture: null,
	segment: null,
	state: null,
	name: '',
	publishDate: null,
	createDate: null,
	updateDate: null,
	scheduledPublishDate: null,
	scheduledUnpublishDate: null,
} as const;
