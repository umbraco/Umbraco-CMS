import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
export interface UmbWorkspaceData<DataModelType = unknown> {
	entityType: string;
	preset: Partial<DataModelType>;
}

// TODO: It would be good with a WorkspaceValueBaseType, to avoid the  hardcoded type for unique here:
export type UmbWorkspaceValue =
	| {
			unique: string;
	  }
	| undefined;

export const UMB_WORKSPACE_MODAL = new UmbModalToken<UmbWorkspaceData, UmbWorkspaceValue>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
});
