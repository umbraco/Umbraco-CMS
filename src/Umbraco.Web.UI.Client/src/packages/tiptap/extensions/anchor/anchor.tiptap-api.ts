import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Anchor } from './anchor.tiptap-extension.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

export default class UmbTiptapAnchorExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Anchor];

	override getStyles = () => css`
		span[data-umb-anchor] {
			&.ProseMirror-selectednode {
				border-radius: var(--uui-border-radius);
				outline: 2px solid var(--uui-color-selected);
			}

			uui-icon {
				height: 1rem;
				width: 1rem;
				vertical-align: text-bottom;
			}
		}
	`;
}
