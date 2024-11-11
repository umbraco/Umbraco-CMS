/* eslint-disable */
// @ts-ignore
import styles from 'monaco-editor/min/vs/editor/editor.main.css?inline';
// @ts-ignore
import editorWorker from 'monaco-editor/esm/vs/editor/editor.worker.js?worker';
// @ts-ignore
import cssWorker from 'monaco-editor/esm/vs/language/css/css.worker.js?worker';
// @ts-ignore
import htmlWorker from 'monaco-editor/esm/vs/language/html/html.worker.js?worker';
// @ts-ignore
import jsonWorker from 'monaco-editor/esm/vs/language/json/json.worker.js?worker';
// @ts-ignore
import tsWorker from 'monaco-editor/esm/vs/language/typescript/ts.worker.js?worker';
/* eslint-enable */

const initializeWorkers = () => {
	self.MonacoEnvironment = {
		getWorker(workerId: string, label: string): Promise<Worker> | Worker {
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

export * as monaco from 'monaco-editor';
export { styles };
