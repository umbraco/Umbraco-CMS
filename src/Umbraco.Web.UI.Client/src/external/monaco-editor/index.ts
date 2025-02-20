/* eslint-disable */
// @ts-ignore
import styles from 'monaco-editor/min/vs/editor/editor.main.css?inline';
/* eslint-enable */

const initializeWorkers = () => {
	self.MonacoEnvironment = {
		getWorker(workerId: string, label: string): Promise<Worker> | Worker {
			let url = '/umbraco/backoffice/monaco-editor/esm/vs/editor/editor.worker.js';
			if (label === 'json') {
				url = '/umbraco/backoffice/monaco-editor/esm/vs/language/json/json.worker.js';
			}
			if (label === 'css' || label === 'scss' || label === 'less') {
				url = '/umbraco/backoffice/monaco-editor/esm/vs/language/css/css.worker.js';
			}
			if (label === 'html' || label === 'handlebars' || label === 'razor') {
				url = '/umbraco/backoffice/monaco-editor/esm/vs/language/html/html.worker.js';
			}
			if (label === 'typescript' || label === 'javascript') {
				url = '/umbraco/backoffice/monaco-editor/esm/vs/language/typescript/ts.worker.js';
			}
			return new Worker(url, { name: workerId, type: 'module' });
		},
	};
};

initializeWorkers();

export * as monaco from 'monaco-editor';
export { styles };
