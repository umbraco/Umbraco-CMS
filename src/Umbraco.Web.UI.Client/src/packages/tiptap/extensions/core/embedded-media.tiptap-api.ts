import { UmbTiptapExtensionApiBase } from '../base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { umbEmbeddedMedia } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapEmbeddedMediaExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [umbEmbeddedMedia.configure({ inline: true })];

	override getStyles = () => css`
		.umb-embed-holder {
			display: inline-block;
			position: relative;
		}

		.umb-embed-holder > * {
			user-select: none;
			pointer-events: none;
		}

		.umb-embed-holder.ProseMirror-selectednode {
			outline: 2px solid var(--uui-palette-spanish-pink-light);
		}

		.umb-embed-holder::before {
			z-index: 1000;
			width: 100%;
			height: 100%;
			position: absolute;
			content: ' ';
		}

		.umb-embed-holder.ProseMirror-selectednode::before {
			background: rgba(0, 0, 0, 0.025);
		}
	`;
}
