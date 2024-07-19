import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbScriptCreateOptionsModalData {
	parent: UmbEntityModel;
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
