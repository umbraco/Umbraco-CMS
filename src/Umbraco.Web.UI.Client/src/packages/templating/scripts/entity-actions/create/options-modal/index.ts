import type { UmbScriptCreateOptionsModalData } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * @deprecated Use the `Umb.EntityAction.Script.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export const UMB_SCRIPT_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbScriptCreateOptionsModalData>(
	'Umb.Modal.Script.CreateOptions',
	{
		modal: {
			type: 'dialog',
		},
	},
);
