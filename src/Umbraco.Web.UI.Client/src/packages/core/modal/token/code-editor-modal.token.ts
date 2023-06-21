import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { CodeEditorLanguage } from '@umbraco-cms/backoffice/code-editor';

export interface UmbCodeEditorModalData {
	headline: string;
	content: string;
	language: CodeEditorLanguage;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

export interface UmbCodeEditorModalResult {
	content: string;
}

export const UMB_CODE_EDITOR_MODAL = new UmbModalToken<UmbCodeEditorModalData, UmbCodeEditorModalResult>(
	'Umb.Modal.CodeEditor',
	{
		type: 'sidebar',
		size: 'large',
	}
);
