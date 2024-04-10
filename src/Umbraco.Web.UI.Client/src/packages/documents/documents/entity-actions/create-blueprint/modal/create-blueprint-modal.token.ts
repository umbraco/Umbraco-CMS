import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateBlueprintModalData {
	unique: string | null;
}

export const UMB_CREATE_BLUEPRINT_MODAL = new UmbModalToken<UmbCreateBlueprintModalData, never>(
	'Umb.Modal.CreateBlueprint',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
