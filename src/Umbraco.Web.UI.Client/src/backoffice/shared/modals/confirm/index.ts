import type { TemplateResult } from 'lit';
import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbConfirmModalData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

export type UmbConfirmModalResult = undefined;

export const UMB_CONFIRM_MODAL_TOKEN = new UmbModalToken<UmbConfirmModalData, UmbConfirmModalResult>(
	'Umb.Modal.Confirm',
	{
		type: 'dialog',
	}
);
