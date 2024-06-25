import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockWorkspaceData<OriginDataType = unknown> extends UmbWorkspaceModalData {
	originData: OriginDataType;
}

export const UMB_BLOCK_WORKSPACE_MODAL = new UmbModalToken<UmbBlockWorkspaceData, UmbWorkspaceModalValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'large',
		},
		data: { entityType: 'block', preset: {}, originData: {} },
	},
	// Recast the type, so the entityType data prop is not required:
) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbWorkspaceModalValue>;
