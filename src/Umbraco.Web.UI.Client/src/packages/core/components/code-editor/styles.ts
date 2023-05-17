import { css, unsafeCSS } from 'lit';
import styles from 'monaco-editor/min/vs/editor/editor.main.css?inline';

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
