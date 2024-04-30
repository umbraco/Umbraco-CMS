import { UmbModalToken } from './modal-token.js';
export interface UmbWorkspaceModalData<DataModelType = unknown> {
	entityType: string;
	preset: Partial<DataModelType>;
}

// TODO: It would be good with a WorkspaceValueBaseType, to avoid the  hardcoded type for unique here:
export type UmbWorkspaceModalValue =
	| {
			unique: string;
	  }
	| undefined;

export const UMB_WORKSPACE_MODAL = new UmbModalToken<UmbWorkspaceModalData, UmbWorkspaceModalValue>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'large',
		},
	},
);
