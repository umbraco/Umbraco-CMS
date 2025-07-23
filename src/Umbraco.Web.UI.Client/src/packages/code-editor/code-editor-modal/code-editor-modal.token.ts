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
	formatOnLoad?: boolean;
}

export interface UmbCodeEditorModalValue {
	content: string;
}

export const UMB_CODE_EDITOR_MODAL = new UmbModalToken<UmbCodeEditorModalData, UmbCodeEditorModalValue>(
	'Umb.Modal.CodeEditor',
	{
		modal: {
			type: 'sidebar',
			size: 'large',
		},
	},
);
