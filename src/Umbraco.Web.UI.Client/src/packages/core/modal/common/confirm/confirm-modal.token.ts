import { UmbModalToken } from '../../token/index.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';

export interface UmbConfirmModalData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger' | 'warning';
	cancelLabel?: string;
	confirmLabel?: string;
}

export type UmbConfirmModalValue = undefined;

export const UMB_CONFIRM_MODAL = new UmbModalToken<UmbConfirmModalData, UmbConfirmModalValue>('Umb.Modal.Confirm', {
	modal: {
		type: 'dialog',
	},
});
