import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { Anchor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapAnchorExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
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
