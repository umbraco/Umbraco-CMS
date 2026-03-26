import type { Editor, ProseMirrorNode } from '../../externals.js';
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
		const currentMediaUdi = this.#extractMediaUdi(currentTarget);
		const currentAltText = currentTarget?.alt;
		const currentCaption = this.#extractCaption(editor.state.selection);
		const currentWidth = currentTarget?.width ? parseInt(currentTarget.width as string, 10) : undefined;
		const currentHeight = currentTarget?.height ? parseInt(currentTarget.height as string, 10) : undefined;

		await this.#updateImageWithMetadata(editor, currentMediaUdi, currentAltText, currentCaption, currentWidth, currentHeight);
	}

	async #updateImageWithMetadata(
		editor: Editor,
		currentMediaUdi: string | undefined,
		currentAltText: string | undefined,
		currentCaption: string | undefined,
		currentWidth: number | undefined,
		currentHeight: number | undefined,
	) {
		const mediaGuid = await this.#getMediaGuid(currentMediaUdi);
		if (!mediaGuid) return;

		const media = await this.#showMediaCaptionAltText(mediaGuid, currentAltText, currentCaption, currentWidth, currentHeight);
		if (!media) return;

		this.#insertInEditor(editor, mediaGuid, media);
	}

	#extractMediaUdi(imageAttributes: Record<string, unknown>): string | undefined {
		return imageAttributes?.['data-udi'] ? getGuidFromUdi(imageAttributes['data-udi'] as string) : undefined;
	}

	#extractCaption(selection: unknown): string | undefined {
		if (!(selection instanceof NodeSelection)) return undefined;
		if (selection.node.type.name !== 'figure') return undefined;

		return this.#findFigcaptionText(selection.node);
	}

	#findFigcaptionText(figureNode: ProseMirrorNode): string | undefined {
		let caption: string | undefined;
		figureNode.descendants((child) => {
			if (child.type.name === 'figcaption') {
				caption = child.textContent || undefined;
				return false; // Stop searching
			}
			return true; // Continue searching
		});
		return caption;
	}

	async #getMediaGuid(currentMediaUdi?: string): Promise<string | undefined> {
		if (currentMediaUdi) {
			// Image already exists, go directly to edit alt text/caption
			return currentMediaUdi;
		}

		// No image selected, open media picker
		const selection = await this.#openMediaPicker();
		if (!selection?.length) return undefined;

		const selectedGuid = selection[0];
		if (!selectedGuid) {
			throw new Error('No media selected');
		}

		return selectedGuid;
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

	async #showMediaCaptionAltText(mediaUnique: string, altText?: string, caption?: string, width?: number, height?: number) {
		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_CAPTION_ALT_TEXT_MODAL, {
			data: { mediaUnique },
			value: {
				url: '',
				altText,
				caption,
				width,
				height,
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

		// Use user-specified dimensions if provided, otherwise fall back to auto-detected
		let finalWidth: number;
		let finalHeight: number;

		if (media.width && media.height) {
			finalWidth = media.width;
			finalHeight = media.height;
		} else {
			const dims = await imageSize(src);
			finalWidth = dims.width;
			finalHeight = dims.height;
		}

		const img = {
			src,
			alt: media.altText,
			'data-udi': `umb://media/${mediaUnique.replace(/-/g, '')}`,
			width: finalWidth.toString(),
			height: finalHeight.toString(),
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
