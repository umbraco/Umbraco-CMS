import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * @deprecated Use the `Umb.EntityAction.PartialView.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export interface UmbPartialViewCreateOptionsModalData {
	parent: UmbEntityModel;
}

/**
 * @deprecated Use the `Umb.EntityAction.PartialView.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export const UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbPartialViewCreateOptionsModalData>(
	'Umb.Modal.PartialView.CreateOptions',
	{
		modal: {
			type: 'dialog',
		},
	},
);
