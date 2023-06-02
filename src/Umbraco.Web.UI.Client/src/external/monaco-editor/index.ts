import styles from 'monaco-editor/min/vs/editor/editor.main.css';
import { css, unsafeCSS } from '@umbraco-cms/backoffice/external/lit';
import editorWorker from 'web-worker:monaco-editor/esm/vs/editor/editor.worker.js';
import cssWorker from 'web-worker:monaco-editor/esm/vs/language/css/css.worker.js';
import htmlWorker from 'web-worker:monaco-editor/esm/vs/language/html/html.worker.js';
import jsonWorker from 'web-worker:monaco-editor/esm/vs/language/json/json.worker.js';
import tsWorker from 'web-worker:monaco-editor/esm/vs/language/typescript/ts.worker.js';

export const monacoEditorStyles = css`
	${unsafeCSS(styles)}
`;

export const monacoJumpingCursorHack = css`
	/* a hacky workaround this issue: https://github.com/microsoft/monaco-editor/issues/3217
	should probably be removed when the issue is fixed */
	.view-lines {
		font-feature-settings: revert !important;
	}
`;

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
		}
	};
};

initializeWorkers();

export * as monaco from 'monaco-editor';
