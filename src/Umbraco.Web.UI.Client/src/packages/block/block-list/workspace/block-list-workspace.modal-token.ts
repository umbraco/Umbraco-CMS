import type { UmbBlockWorkspaceData } from '../../index.js';
import type { UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockListWorkspaceData
	extends UmbBlockWorkspaceData<{
		index: number;
	}> {}

export const UMB_BLOCK_LIST_WORKSPACE_MODAL = new UmbModalToken<UmbBlockListWorkspaceData, UmbWorkspaceValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		data: { entityType: 'block', preset: {}, originData: { index: -1 } },
		// Recast the type, so the entityType data prop is not required:
	},
) as UmbModalToken<Omit<UmbWorkspaceData, 'entityType'>, UmbWorkspaceValue>;
