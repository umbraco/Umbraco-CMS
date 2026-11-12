import type { UmbMemberTypeDetailModel } from '../types.js';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * Modal token that opens the member type workspace in a sidebar modal.
 * The modal is marked as a reference (`isReference`), so the workspace is presented as a
 * reference to the member type rather than as the primary editing context.
 * Only `preset` and other workspace data are required; the entity type is already set.
 */
export const UMB_MEMBER_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbWorkspaceModalData<UmbMemberTypeDetailModel>,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
		isReference: true,
	},
	data: { entityType: 'member-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceModalData<UmbMemberTypeDetailModel>, 'entityType'>, UmbWorkspaceModalValue>;
