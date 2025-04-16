import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { umbEmbeddedMedia } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_EMBEDDED_MEDIA_MODAL } from '@umbraco-cms/backoffice/embedded-media';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarEmbeddedMediaExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		const data = {
			constrain: false,
			height: 240,
			width: 360,
			url: '',
		};

		const attrs = editor?.getAttributes(umbEmbeddedMedia.name);
		if (attrs) {
			data.constrain = attrs['data-embed-constrain'];
			data.height = attrs['data-embed-height'];
			data.width = attrs['data-embed-width'];
			data.url = attrs['data-embed-url'];
		}

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalHandler = modalManager.open(this, UMB_EMBEDDED_MEDIA_MODAL, { data });

		if (!modalHandler) return;

		const result = await modalHandler.onSubmit().catch(() => undefined);
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
