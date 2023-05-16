//eslint-disable-next-line
import editorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';
//eslint-disable-next-line
import jsonWorker from 'monaco-editor/esm/vs/language/json/json.worker?worker';
//eslint-disable-next-line
import cssWorker from 'monaco-editor/esm/vs/language/css/css.worker?worker';
//eslint-disable-next-line
import htmlWorker from 'monaco-editor/esm/vs/language/html/html.worker?worker';
//eslint-disable-next-line
import tsWorker from 'monaco-editor/esm/vs/language/typescript/ts.worker?worker';

export const initializeWorkers = () => {
	self.MonacoEnvironment = {
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		getWorker(_: any, label: string) {
			if (label === 'json') {
				return new jsonWorker();
			}
			if (label === 'css' || label === 'scss' || label === 'less') {
				return new cssWorker();
			}
			if (label === 'html' || label === 'handlebars' || label === 'razor') {
				return new htmlWorker();
			}
			if (label === 'typescript' || label === 'javascript') {
				return new tsWorker();
			}
			return new editorWorker();
		},
	};
};

initializeWorkers();
