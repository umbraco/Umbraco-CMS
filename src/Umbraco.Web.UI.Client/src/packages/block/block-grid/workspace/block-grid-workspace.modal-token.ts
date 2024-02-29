import type { UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import type { UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockGridWorkspaceData
	extends UmbBlockWorkspaceData<{
		index: number;
		parentId: string | null;
	}> {}

export const UMB_BLOCK_GRID_WORKSPACE_MODAL = new UmbModalToken<UmbBlockGridWorkspaceData, UmbWorkspaceValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		data: { entityType: 'block', preset: {}, originData: { index: -1, parentId: null } },
		// Recast the type, so the entityType data prop is not required:
	},
) as UmbModalToken<Omit<UmbWorkspaceData, 'entityType'>, UmbWorkspaceValue>;
