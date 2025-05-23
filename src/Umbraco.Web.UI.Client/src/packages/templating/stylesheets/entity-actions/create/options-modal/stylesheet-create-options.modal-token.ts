import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/** @deprecated No longer used internally. This will be removed in Umbraco 18. [LK] */
export interface UmbStylesheetCreateOptionsModalData {
	parent: UmbEntityModel;
}

/** @deprecated No longer used internally. This will be removed in Umbraco 18. [LK] */
export const UMB_STYLESHEET_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbStylesheetCreateOptionsModalData>(
	'Umb.Modal.Stylesheet.CreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
