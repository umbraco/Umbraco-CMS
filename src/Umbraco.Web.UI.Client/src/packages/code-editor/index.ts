export * from './components/index.js';
export * from './models/index.js';
export * from './code-editor-modal/index.js';

export type { UmbCodeEditorController } from './code-editor.controller.js';

/**
 * @deprecated Use `import from '@umbraco-cms/backoffice/code-editor';` directly.
 * This function will be removed in Umbraco 15.
 */
export function loadCodeEditor() {
	return import('@umbraco-cms/backoffice/code-editor');
}
