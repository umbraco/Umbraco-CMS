import * as initializeWorkers from './languageWorkers.js';
import { UmbCodeEditorElement } from './code-editor.element.js';
import { UmbCodeEditorController } from './code-editor.controller.js';
import { monacoEditorStyles } from './styles.js';

export default UmbCodeEditorElement;

export { initializeWorkers, UmbCodeEditorController as UmbCodeEditor, UmbCodeEditorElement, monacoEditorStyles };
