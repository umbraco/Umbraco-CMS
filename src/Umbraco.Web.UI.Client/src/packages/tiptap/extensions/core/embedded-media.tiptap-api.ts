import { UmbTiptapExtensionApiBase } from '../base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { umbEmbeddedMedia } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapEmbeddedMediaExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [umbEmbeddedMedia.configure({ inline: true })];

	override getStyles = () => css`
		.umb-embed-holder {
			display: inline-block;
			position: relative;

			&::before {
				z-index: 1000;
				width: 100%;
				height: 100%;
				position: absolute;
				content: ' ';
			}

			&.ProseMirror-selectednode {
				outline: 3px solid var(--uui-color-focus);

				&::before {
					background: rgba(0, 0, 0, 0.1);
				}

				> * {
					user-select: none;
					pointer-events: none;
				}
			}
		}
	`;
}
