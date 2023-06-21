import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// TODO => investigate why exporting CodeEditorLanguage in code-editor barrel 
// causes the schema generation task to fail... For now, language property below
// duplicates the CodeEditorLanguage type
export interface UmbCodeEditorModalData {
	headline: string;
	content: string;
	language: 'razor' | 'typescript' | 'javascript' | 'css' | 'markdown' | 'json' | 'html';
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
