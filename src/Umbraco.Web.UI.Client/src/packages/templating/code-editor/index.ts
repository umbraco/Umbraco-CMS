// import * as initializeWorkers from './languageWorkers.js';
export type { UmbCodeEditorElement } from './code-editor.element.js';
export type { UmbCodeEditorController } from './code-editor.controller.js';

export function loadCodeEditor() {
	return import('./code-editor.element.js');
}
