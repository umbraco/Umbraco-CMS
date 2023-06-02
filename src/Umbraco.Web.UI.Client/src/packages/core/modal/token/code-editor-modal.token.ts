import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCodeEditorModalData {
	headline: string;
	content: string;
	language: string; // TODO => should used CodeEditorLanguage, but model is not part of libs (yet)
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
