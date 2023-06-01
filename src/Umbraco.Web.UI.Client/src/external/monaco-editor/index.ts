import styles from 'monaco-editor/min/vs/editor/editor.main.css';
import { css, unsafeCSS } from '@umbraco-cms/backoffice/external/lit';

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

self.MonacoEnvironment = {
	getWorkerUrl: function (_moduleId, label) {
		if (label === 'json') {
			return './vs/language/json.worker.js';
		}
		if (label === 'css' || label === 'scss' || label === 'less') {
			return './vs/language/css.worker.js';
		}
		if (label === 'html' || label === 'handlebars' || label === 'razor') {
			return './vs/language/html.worker.js';
		}
		if (label === 'typescript' || label === 'javascript') {
			return './vs/language/ts.worker.js';
		}
		return './vs/language/editor.worker.js';
	},
};

export * as monaco from 'monaco-editor';
