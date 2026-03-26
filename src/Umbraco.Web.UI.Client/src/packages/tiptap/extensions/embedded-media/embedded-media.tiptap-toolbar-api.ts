import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { umbEmbeddedMedia } from './embedded-media.tiptap-extension.js';
import { UMB_EMBEDDED_MEDIA_MODAL } from '@umbraco-cms/backoffice/embedded-media';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export default class UmbTiptapToolbarEmbeddedMediaExtensionApi extends UmbTiptapToolbarElementApiBase {
	get maxImageSize(): number {
		const maxImageSize = parseInt(this.configuration?.getValueByAlias('maxImageSize') ?? '', 10);
		return isNaN(maxImageSize) ? 500 : maxImageSize;
	}

	override async execute(editor?: Editor) {
		const width = this.maxImageSize;
		const data = {
			constrain: false,
			height: Math.round(width / (16 / 9)),
			width,
			url: '',
		};

		const attrs = editor?.getAttributes(umbEmbeddedMedia.name);
		if (attrs) {
			data.constrain = attrs['data-embed-constrain'];
			data.height = attrs['data-embed-height'];
			data.width = attrs['data-embed-width'];
			data.url = attrs['data-embed-url'];
		}

		const result = await umbOpenModal(this, UMB_EMBEDDED_MEDIA_MODAL, { data }).catch(() => undefined);

		if (!result) return;

		editor?.commands.setEmbeddedMedia({
			markup: result.markup,
			url: result.url,
			constrain: result.constrain,
			height: result.height?.toString(),
			width: result.width?.toString(),
		});
	}
}
