import type { UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBlockGridAreaTypeWorkspaceData extends UmbWorkspaceData {}

export const UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbBlockGridAreaTypeWorkspaceData,
	UmbWorkspaceValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: { entityType: 'block-grid-area-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbBlockGridAreaTypeWorkspaceData, 'entityType'>, UmbWorkspaceValue>;
