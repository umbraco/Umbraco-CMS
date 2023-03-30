import { CodeEditorLanguage } from "../../../src/backoffice/shared/components/code-editor/code-editor.model";
import { UmbModalToken } from "./modal-token";

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
