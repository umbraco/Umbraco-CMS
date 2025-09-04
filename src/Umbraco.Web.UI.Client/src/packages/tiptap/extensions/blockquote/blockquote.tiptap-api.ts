import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { Blockquote } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBlockquoteExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Blockquote];

	override getStyles = () => css`
		blockquote {
			border-left: var(--uui-size-1) solid var(--uui-color-border);
			margin-left: 0;
			padding-left: var(--uui-size-space-4);
		}
	`;
}
