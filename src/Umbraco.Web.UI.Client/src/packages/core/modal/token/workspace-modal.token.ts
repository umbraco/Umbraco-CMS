import { CreateDataTypeRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// TODO: Change model:
export interface UmbWorkspaceData {
	entityType: string;
	preset: Partial<CreateDataTypeRequestModel>;
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
