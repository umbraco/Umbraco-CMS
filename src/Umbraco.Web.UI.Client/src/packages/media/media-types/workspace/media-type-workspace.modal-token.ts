import type { UmbMediaTypeDetailModel } from '../types.js';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

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
