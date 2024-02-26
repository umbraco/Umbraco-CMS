import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbStylesheetCreateOptionsModalData {
	parentUnique: string | null;
	entityType: string;
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
