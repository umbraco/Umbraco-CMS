import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbStylesheetCreateOptionsModalData {
	parent: UmbEntityModel;
}

export const UMB_STYLESHEET_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbStylesheetCreateOptionsModalData>(
	'Umb.Modal.Stylesheet.CreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
