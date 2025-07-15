import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockWorkspaceOriginData {}

export interface UmbBlockWorkspaceData<OriginDataType extends UmbBlockWorkspaceOriginData = UmbBlockWorkspaceOriginData>
	extends UmbWorkspaceModalData {
	originData: OriginDataType;
	baseDataPath: string;
}

export const UMB_BLOCK_WORKSPACE_MODAL = new UmbModalToken<UmbBlockWorkspaceData, UmbWorkspaceModalValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'large',
		},
		data: { entityType: 'block', preset: {}, originData: {}, baseDataPath: undefined as unknown as string },
	},
	// Recast the type, so the entityType data prop is not required:
) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbWorkspaceModalValue>;
