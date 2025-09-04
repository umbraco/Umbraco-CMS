import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { Code, CodeBlock } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapCodeBlockExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Code, CodeBlock];

	override getStyles = () => css`
		pre {
			background-color: var(--uui-color-surface-alt);
			padding: var(--uui-size-space-2) var(--uui-size-space-4);
			border-radius: calc(var(--uui-border-radius) * 2);
			overflow-x: auto;
		}

		code:not(pre > code) {
			background-color: var(--uui-color-surface-alt);
			padding: var(--uui-size-space-1) var(--uui-size-space-2);
			border-radius: calc(var(--uui-border-radius) * 2);
		}

		code {
			font-family: 'Roboto Mono', monospace;
			background: none;
			color: inherit;
			font-size: 0.8rem;
			padding: 0;
		}
	`;
}
