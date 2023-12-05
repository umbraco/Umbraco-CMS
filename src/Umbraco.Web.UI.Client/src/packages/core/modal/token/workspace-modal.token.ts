import { CreateDataTypeRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbWorkspaceData {
	entityType: string;
	preset: Partial<CreateDataTypeRequestModel>;
}

export type UmbWorkspaceValue =
	| {
			id: string;
	  }
	| undefined;

export const UMB_WORKSPACE_MODAL = new UmbModalToken<UmbWorkspaceData, UmbWorkspaceValue>('Umb.Modal.Workspace', {
	type: 'sidebar',
	size: 'large',
});
