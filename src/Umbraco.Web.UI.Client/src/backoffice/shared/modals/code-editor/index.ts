import type { TemplateResult } from 'lit';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCodeEditorModalData {
	headline: string;
	content?: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

export interface UmbCodeEditorModalResult {
	content?: TemplateResult | string;
}

export const UMB_CODE_EDITOR_MODAL_TOKEN = new UmbModalToken<UmbCodeEditorModalData, UmbCodeEditorModalResult>(
	'Umb.Modal.CodeEditor',
	{
		type: 'sidebar',
		size: 'large',
	}
);
