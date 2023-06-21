export type { UmbCodeEditorElement } from './code-editor.element.js';
export type { UmbCodeEditorController } from './code-editor.controller.js';
export type { CodeEditorLanguage } from './code-editor.model.js';

export function loadCodeEditor() {
	return import('./code-editor.element.js');
}
