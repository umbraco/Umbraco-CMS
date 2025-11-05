import type { Editor } from '../../externals.js';
import { NodeSelection } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { getGuidFromUdi, imageSize } from '@umbraco-cms/backoffice/utils';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { UMB_MEDIA_CAPTION_ALT_TEXT_MODAL, UMB_MEDIA_PICKER_MODAL } from '@umbraco-cms/backoffice/media';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMediaCaptionAltTextModalValue } from '@umbraco-cms/backoffice/media';

export default class UmbTiptapToolbarMediaPickerToolbarExtensionApi extends UmbTiptapToolbarElementApiBase {
	#imagingRepository = new UmbImagingRepository(this);

	#modalManager?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	/**
	 * @returns {number} The configured maximum allowed image size
	 */
	get maxImageSize(): number {
		const maxImageSize = parseInt(this.configuration?.getValueByAlias('maxImageSize') ?? '', 10);
		return isNaN(maxImageSize) ? 500 : maxImageSize;
	}

	/**
	 * @deprecated Use `maxImageSize` instead.
	 * @returns {number} The maximum width of uploaded images
	 */
	maxWidth = this.maxImageSize;

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

		let currentMediaUdi: string | undefined = undefined;
		if (currentTarget?.['data-udi']) {
			currentMediaUdi = getGuidFromUdi(currentTarget['data-udi']);
		}

		let currentAltText: string | undefined = undefined;
		if (currentTarget?.alt) {
			currentAltText = currentTarget.alt;
		}

		let currentCaption: string | undefined = undefined;
		// Find the figure node and extract the figcaption text
		const selection = editor.state.selection;

		// Check if the selection is a NodeSelection with a figure node
		if (selection instanceof NodeSelection) {
			const selectedNode = selection.node;

			if (selectedNode.type.name === 'figure') {
				// Extract the figcaption text from the figure node
				selectedNode.descendants((child) => {
					if (child.type.name === 'figcaption') {
						currentCaption = child.textContent || undefined;
						return false; // Stop searching
					}
					return true; // Continue searching
				});
			}
		}

		let mediaGuid: string;

		if (currentMediaUdi) {
			// Image already exists, go directly to edit alt text/caption
			mediaGuid = currentMediaUdi;
		} else {
			// No image selected, open media picker
			const selection = await this.#openMediaPicker();
			if (!selection?.length) return;

			const selectedGuid = selection[0];

			if (!selectedGuid) {
				throw new Error('No media selected');
			}

			mediaGuid = selectedGuid;
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

		const maxImageSize = this.maxImageSize;

		// Get the resized image URL
		const { data } = await this.#imagingRepository.requestResizedItems([mediaUnique], {
			width: maxImageSize,
			height: maxImageSize,
			mode: ImageCropModeModel.MAX,
		});

		if (!data?.length || !data[0]?.url) {
			console.error('No data returned from imaging repository');
			return;
		}

		// Set the media URL to the first item in the data array
		const src = data[0].url;

		// Fetch the actual image dimensions
		const { width, height } = await imageSize(src);

		const img = {
			src,
			alt: media.altText,
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
