import type { TemplateResult } from 'lit';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbConfirmModalData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

export type UmbConfirmModalResult = undefined;

export const UMB_CONFIRM_MODAL = new UmbModalToken<UmbConfirmModalData, UmbConfirmModalResult>('Umb.Modal.Confirm', {
	type: 'dialog',
});
