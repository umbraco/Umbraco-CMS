import { UmbBlockWorkspaceData } from '../../index.js';
import { UmbModalToken, UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockListWorkspaceData
	extends UmbBlockWorkspaceData<{
		index: number;
		parentId: string | null;
	}> {}

export const UMB_BLOCK_GRID_WORKSPACE_MODAL = new UmbModalToken<UmbBlockListWorkspaceData, UmbWorkspaceValue>(
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
