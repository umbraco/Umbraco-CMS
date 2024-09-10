import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_SYSINFO_MODAL_ALIAS } from '../manifests.js';

export const UMB_SYSINFO_MODAL = new UmbModalToken(UMB_SYSINFO_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
		size: 'medium',
	},
});
