import type { UmbBlockWorkspaceData, UmbBlockWorkspaceOriginData } from '@umbraco-cms/backoffice/block';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockGridWorkspaceOriginData extends UmbBlockWorkspaceOriginData {
	index: number;
	parentUnique: string | null;
	areaKey?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockGridWorkspaceData extends UmbBlockWorkspaceData<UmbBlockGridWorkspaceOriginData> {}

export const UMB_BLOCK_GRID_WORKSPACE_MODAL = new UmbModalToken<UmbBlockGridWorkspaceData, UmbWorkspaceModalValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		data: {
			entityType: 'block',
			preset: {},
			originData: { index: -1, parentUnique: null },
			baseDataPath: undefined as unknown as string,
		},
	},
	// Recast the type, so the entityType data prop is not required:
) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbWorkspaceModalValue>;
