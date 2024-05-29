import type { UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockRteWorkspaceData extends UmbBlockWorkspaceData<object> {}

export const UMB_BLOCK_RTE_WORKSPACE_MODAL = new UmbModalToken<UmbBlockRteWorkspaceData, UmbWorkspaceModalValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		data: { entityType: 'block', preset: {}, originData: {} },
		// Recast the type, so the entityType data prop is not required:
	},
) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbWorkspaceModalValue>;
