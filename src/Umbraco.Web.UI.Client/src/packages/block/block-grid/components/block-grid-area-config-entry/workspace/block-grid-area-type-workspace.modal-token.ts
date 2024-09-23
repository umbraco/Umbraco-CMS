import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockGridAreaTypeWorkspaceData extends UmbWorkspaceModalData {}

export const UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbBlockGridAreaTypeWorkspaceData,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: { entityType: 'block-grid-area-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbBlockGridAreaTypeWorkspaceData, 'entityType'>, UmbWorkspaceModalValue>;
