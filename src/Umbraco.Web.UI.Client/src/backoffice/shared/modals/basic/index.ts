import type { TemplateResult } from 'lit';
import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbBasicModalData {
	headline: string;
	content: TemplateResult | string;
}

export const UMB_BASIC_MODAL_TOKEN = new UmbModalToken<UmbBasicModalData>('Umb.Modal.Basic', {
	type: 'dialog',
});
