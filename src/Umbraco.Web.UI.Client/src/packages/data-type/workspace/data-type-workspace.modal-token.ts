import type { UmbDataTypeDetailModel } from '../types.js';
import type { UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DATATYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbWorkspaceData<UmbDataTypeDetailModel>,
	UmbWorkspaceValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
	data: { entityType: 'data-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceData<UmbDataTypeDetailModel>, 'entityType'>, UmbWorkspaceValue>;
