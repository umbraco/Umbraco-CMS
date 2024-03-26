import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbStylesheetCreateOptionsModalData {
	parent: {
		unique: string | null;
		entityType: string;
	};
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
