import type { ReferenceByIdModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateBlueprintModalData {
	unique: string | null;
}

export interface UmbCreateBlueprintModalValue {
	name: string;
	parent: ReferenceByIdModel | null;
}

export const UMB_CREATE_BLUEPRINT_MODAL = new UmbModalToken<UmbCreateBlueprintModalData, UmbCreateBlueprintModalValue>(
	'Umb.Modal.CreateBlueprint',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
