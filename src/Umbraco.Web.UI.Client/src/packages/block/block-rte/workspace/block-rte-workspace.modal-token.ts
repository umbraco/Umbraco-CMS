import type {
	UmbBlockLayoutBaseModel,
	UmbBlockViewPropsType,
	UmbBlockWorkspaceData,
} from '@umbraco-cms/backoffice/block';
import type { UmbWorkspaceModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockRTEWorkspaceData
	extends UmbBlockWorkspaceData<{
		index: number;
	}> {}

export type UmbBlockRTEWorkspaceValue = Array<UmbBlockViewPropsType<UmbBlockLayoutBaseModel>>;

export const UMB_BLOCK_RTE_WORKSPACE_MODAL = new UmbModalToken<UmbBlockRTEWorkspaceData, UmbBlockRTEWorkspaceValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		data: { entityType: 'block', preset: {}, originData: { index: -1 } },
		// Recast the type, so the entityType data prop is not required:
	},
) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbBlockRTEWorkspaceValue>;
