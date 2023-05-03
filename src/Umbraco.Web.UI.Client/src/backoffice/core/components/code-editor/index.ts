import * as initializeWorkers from './languageWorkers';
import { UmbCodeEditorElement } from './code-editor.element';
import { UmbCodeEditorController } from './code-editor.controller';
import { monacoEditorStyles } from './styles';

export default UmbCodeEditorElement;

export { initializeWorkers, UmbCodeEditorController as UmbCodeEditor, UmbCodeEditorElement, monacoEditorStyles };
