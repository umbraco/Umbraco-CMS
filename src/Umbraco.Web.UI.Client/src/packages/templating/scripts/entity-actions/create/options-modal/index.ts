import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbScriptCreateOptionsModalData {
	parentUnique: string | null;
}

export const UMB_SCRIPT_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbScriptCreateOptionsModalData>(
	'Umb.Modal.Script.CreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
