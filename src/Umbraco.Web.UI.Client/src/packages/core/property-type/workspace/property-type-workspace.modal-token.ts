import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPropertyTypeWorkspaceData extends UmbWorkspaceModalData {
	contentTypeUnique: string;
}

export type UmbPropertyTypeWorkspaceValue = never;

export const UMB_PROPERTY_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbPropertyTypeWorkspaceData,
	UmbPropertyTypeWorkspaceValue
>(
	'Umb.Modal.Workspace',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: { entityType: 'property-type', preset: {}, contentTypeUnique: undefined as unknown as string },
	},
	// Recast the type, so the entityType data prop is not required:
) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbWorkspaceModalValue>;
