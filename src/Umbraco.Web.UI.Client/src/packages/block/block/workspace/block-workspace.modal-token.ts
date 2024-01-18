import { UmbModalToken, UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';

export const UMB_BLOCK_WORKSPACE_MODAL = new UmbModalToken<UmbWorkspaceData, UmbWorkspaceValue>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
	data: { entityType: 'block', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceData, 'entityType'>, UmbWorkspaceValue>;
