import type { UmbDataTypeDetailModel } from '../types.js';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DATATYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbWorkspaceModalData<UmbDataTypeDetailModel>,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
	data: { entityType: 'data-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceModalData<UmbDataTypeDetailModel>, 'entityType'>, UmbWorkspaceModalValue>;
