import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { UmbImage } from './image.tiptap-extension.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

export default class UmbTiptapImageExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [UmbImage.configure({ inline: true })];

	override getStyles = () => css`
		figure {
			> p,
			img {
				pointer-events: none;
				margin: 0;
				padding: 0;
			}

			&.ProseMirror-selectednode {
				outline: 3px solid var(--uui-color-focus);
			}
		}

		img {
			&.ProseMirror-selectednode {
				outline: 3px solid var(--uui-color-focus);
			}
		}
	`;
}
