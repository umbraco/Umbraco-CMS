import type { UmbMediaTypeDetailModel } from '../types.js';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * Modal token that opens the media type workspace in a sidebar modal.
 * The modal is marked as a reference (`isReference`), so the workspace is presented as a
 * reference to the media type rather than as the primary editing context.
 * Only `preset` and other workspace data are required; the entity type is already set.
 */
export const UMB_MEDIA_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbWorkspaceModalData<UmbMediaTypeDetailModel>,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
		isReference: true,
	},
	data: { entityType: 'media-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceModalData<UmbMediaTypeDetailModel>, 'entityType'>, UmbWorkspaceModalValue>;
