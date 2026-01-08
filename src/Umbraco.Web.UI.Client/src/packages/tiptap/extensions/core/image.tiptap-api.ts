import { UmbTiptapExtensionApiBase } from '../base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

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
