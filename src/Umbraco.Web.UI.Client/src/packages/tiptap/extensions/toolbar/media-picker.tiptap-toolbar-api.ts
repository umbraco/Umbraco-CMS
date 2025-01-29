import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { getGuidFromUdi, getProcessedImageUrl, imageSize } from '@umbraco-cms/backoffice/utils';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_MEDIA_CAPTION_ALT_TEXT_MODAL, UMB_MEDIA_PICKER_MODAL } from '@umbraco-cms/backoffice/media';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMediaCaptionAltTextModalValue } from '@umbraco-cms/backoffice/media';

export default class UmbTiptapToolbarMediaPickerToolbarExtensionApi extends UmbTiptapToolbarElementApiBase {
	#modalManager?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	/**
	 * @returns {number} The maximum width of uploaded images
	 */
	get maxWidth(): number {
		const maxImageSize = parseInt(this.configuration?.getValueByAlias('maxImageSize') ?? '', 10);
		return isNaN(maxImageSize) ? 500 : maxImageSize;
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManager = instance;
		});
	}

	override isActive(editor?: Editor) {
		return editor?.isActive('image') === true || editor?.isActive('figure') === true;
	}

	override async execute(editor: Editor) {
		const currentTarget = editor.getAttributes('image');
		const figure = editor.getAttributes('figure');

		let currentMediaUdi: string | undefined = undefined;
		if (currentTarget?.['data-udi']) {
			currentMediaUdi = getGuidFromUdi(currentTarget['data-udi']);
		}

		let currentAltText: string | undefined = undefined;
		if (currentTarget?.alt) {
			currentAltText = currentTarget.alt;
		}

		let currentCaption: string | undefined = undefined;
		if (figure?.figcaption) {
			currentCaption = figure.figcaption;
		}

		const selection = await this.#openMediaPicker(currentMediaUdi);
		if (!selection?.length) return;

		const mediaGuid = selection[0];

		if (!mediaGuid) {
			throw new Error('No media selected');
		}

		const media = await this.#showMediaCaptionAltText(mediaGuid, currentAltText, currentCaption);
		if (!media) return;

		this.#insertInEditor(editor, mediaGuid, media);
	}

	async #openMediaPicker(currentMediaUdi?: string) {
		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_PICKER_MODAL, {
			data: {
				multiple: false,
				//startNodeIsVirtual,
			},
			value: {
				selection: currentMediaUdi ? [currentMediaUdi] : [],
			},
		});

		if (!modalHandler) return;

		const { selection } = await modalHandler.onSubmit().catch(() => ({ selection: undefined }));

		return selection;
	}

	async #showMediaCaptionAltText(mediaUnique: string, altText?: string, caption?: string) {
		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_CAPTION_ALT_TEXT_MODAL, {
			data: { mediaUnique },
			value: {
				url: '',
				altText,
				caption,
			},
		});
		const mediaData = await modalHandler?.onSubmit().catch(() => null);
		return mediaData;
	}

	async #insertInEditor(editor: Editor, mediaUnique: string, media: UmbMediaCaptionAltTextModalValue) {
		if (!media?.url) return;

		const { width, height } = await imageSize(media.url, { maxWidth: this.maxWidth });
		const src = await getProcessedImageUrl(media.url, { width, height, mode: ImageCropModeModel.MAX });

		const img = {
			alt: media.altText,
			src,
			'data-udi': `umb://media/${mediaUnique.replace(/-/g, '')}`,
			width: width.toString(),
			height: height.toString(),
		};

		if (media.caption) {
			return editor.commands.insertContent({
				type: 'figure',
				content: [
					{
						type: 'paragraph',
						content: [
							{
								type: 'image',
								attrs: img,
							},
						],
					},
					{
						type: 'figcaption',
						content: [
							{
								type: 'text',
								text: media.caption,
							},
						],
					},
				],
			});
		}

		return editor.commands.setImage(img);
	}
}
