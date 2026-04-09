import { UmbModalToken } from '../../token/index.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';

export interface UmbInfoModalData {
	headline: string;
	content: TemplateResult | string;
}

export type UmbInfoModalValue = undefined;

export const UMB_INFO_MODAL = new UmbModalToken<UmbInfoModalData, UmbInfoModalValue>('Umb.Modal.Info', {
	modal: {
		type: 'dialog',
	},
});
