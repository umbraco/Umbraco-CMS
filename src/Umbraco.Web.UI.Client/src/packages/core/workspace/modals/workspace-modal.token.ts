import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
export interface UmbWorkspaceModalData<DataModelType = unknown> {
	entityType: string;
	preset: Partial<DataModelType>;
	baseDataPath?: string;
	inheritValidationLook?: boolean;
}

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
