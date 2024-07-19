import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPartialViewCreateOptionsModalData {
	parent: UmbEntityModel;
}

export const UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbPartialViewCreateOptionsModalData>(
	'Umb.Modal.PartialView.CreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
