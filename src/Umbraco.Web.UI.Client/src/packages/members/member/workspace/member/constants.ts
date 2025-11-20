import type { UmbMemberVariantModel } from '../../types.js';

export const UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD: UmbMemberVariantModel = {
	culture: null,
	segment: null,
	name: '',
	createDate: null,
	updateDate: null,
	flags: [],
} as const;

export { UMB_MEMBER_WORKSPACE_CONTEXT } from './member-workspace.context-token.js';

export const UMB_MEMBER_WORKSPACE_ALIAS = 'Umb.Workspace.Member';
export const UMB_MEMBER_WORKSPACE_VIEW_CONTENT_ALIAS = 'Umb.WorkspaceView.Member.Content';
export const UMB_MEMBER_WORKSPACE_VIEW_MEMBER_ALIAS = 'Umb.WorkspaceView.Member.Member';
