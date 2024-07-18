export { UmbCodeEditorElement } from './code-editor.element.js';
export { UmbCodeEditorLoadedEvent } from './code-editor-loaded.event.js';
export type { UmbCodeEditorController } from './code-editor.controller.js';
export type * from './code-editor.model.js';

/**
 * @deprecated Use `import from '@umbraco-cms/backoffice/code-editor';` directly.
 * This function will be removed in Umbraco 15.
 */
export function loadCodeEditor() {
	return import('@umbraco-cms/backoffice/code-editor');
}
