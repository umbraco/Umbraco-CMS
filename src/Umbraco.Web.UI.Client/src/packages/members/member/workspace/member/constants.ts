import type { UmbMemberVariantModel } from '../../types.js';

export const UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD: UmbMemberVariantModel = {
	culture: null,
	segment: null,
	name: '',
	createDate: null,
	updateDate: null,
} as const;

export { UMB_MEMBER_WORKSPACE_CONTEXT } from './member-workspace.context-token.js';
export { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
