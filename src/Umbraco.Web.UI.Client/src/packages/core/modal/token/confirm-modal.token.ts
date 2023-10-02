import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbConfirmModalData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

export type UmbConfirmModalValue = undefined;

export const UMB_CONFIRM_MODAL = new UmbModalToken<UmbConfirmModalData, UmbConfirmModalValue>('Umb.Modal.Confirm', {
	type: 'dialog',
});
