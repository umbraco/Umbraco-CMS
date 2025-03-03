import type { UmbMediaVariantModel } from '../types.js';

export * from './media-workspace.context-token.js';

export const UMB_MEDIA_WORKSPACE_ALIAS = 'Umb.Workspace.Media';

export const UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD: UmbMediaVariantModel = {
	culture: null,
	segment: null,
	name: '',
	createDate: null,
	updateDate: null,
} as const;
