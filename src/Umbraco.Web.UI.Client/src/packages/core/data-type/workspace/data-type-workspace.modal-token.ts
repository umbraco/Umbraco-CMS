import { CreateDataTypeRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbWorkspaceData, UmbWorkspaceValue } from '@umbraco-cms/backoffice/modal';

export const UMB_DATATYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbWorkspaceData<CreateDataTypeRequestModel>,
	UmbWorkspaceValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
	data: { entityType: 'data-type', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceData<CreateDataTypeRequestModel>, 'entityType'>, UmbWorkspaceValue>;
