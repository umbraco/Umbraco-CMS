import type { Editor } from '../../externals.js';
import { NodeSelection } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { getGuidFromUdi, splitStringToArray } from '@umbraco-cms/backoffice/utils';
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

	get #allowedMediaTypeIds(): Array<string> {
		return splitStringToArray(this.configuration?.getValueByAlias<string>('allowedMediaTypes'));
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
		let currentTarget = editor.getAttributes('image');
		let currentMediaUdi = this.#extractMediaUdi(currentTarget);
		let currentCaption: string | undefined;

		// If no image found directly, check if cursor is inside a figure (e.g. in figcaption)
		if (!currentMediaUdi) {
			const figureData = this.#extractFigureImageData(editor);
			if (figureData) {
				currentTarget = figureData.imageAttrs;
				currentMediaUdi = this.#extractMediaUdi(currentTarget);
				currentCaption = figureData.caption;
				// Select the figure so insertContent replaces it instead of inserting at cursor
				editor.commands.setNodeSelection(figureData.pos);
			}
		} else {
			currentCaption = this.#extractCaption(editor.state.selection);
		}

		// If editing existing image, use its UDI; otherwise open media picker
		const mediaGuid = currentMediaUdi ?? (await this.#openMediaPicker());
		if (!mediaGuid) return;

		const media = await this.#showMediaCaptionAltText(
			mediaGuid,
			currentTarget?.alt as string | undefined,
			currentCaption,
			currentTarget?.width ? parseInt(currentTarget.width as string, 10) : undefined,
			currentTarget?.height ? parseInt(currentTarget.height as string, 10) : undefined,
		);
		if (!media) return;

		this.#insertInEditor(editor, mediaGuid, media);
	}

	#extractMediaUdi(imageAttributes: Record<string, unknown>): string | undefined {
		return imageAttributes?.['data-udi'] ? getGuidFromUdi(imageAttributes['data-udi'] as string) : undefined;
	}

	#extractCaption(selection: unknown): string | undefined {
		if (!(selection instanceof NodeSelection)) return undefined;
		if (selection.node.type.name !== 'figure') return undefined;

		let caption: string | undefined;
		selection.node.descendants((child) => {
			if (child.type.name === 'figcaption') {
				caption = child.textContent || undefined;
				return false;
			}
			return true;
		});
		return caption;
	}

	#extractFigureImageData(
		editor: Editor,
	): { imageAttrs: Record<string, unknown>; caption?: string; pos: number } | undefined {
		const { $from } = editor.state.selection;

		for (let depth = $from.depth; depth >= 0; depth--) {
			const node = $from.node(depth);
			if (node.type.name === 'figure') {
				let imageAttrs: Record<string, unknown> = {};
				let caption: string | undefined;

				node.descendants((child) => {
					if (child.type.name === 'image') {
						imageAttrs = { ...child.attrs };
						return false;
					}
					if (child.type.name === 'figcaption') {
						caption = child.textContent || undefined;
						return false;
					}
					return true;
				});

				if (imageAttrs['data-udi']) {
					return { imageAttrs, caption, pos: $from.before(depth) };
				}
			}
		}

		return undefined;
	}

	async #openMediaPicker(currentMediaUdi?: string): Promise<string | undefined> {
		const allowedIds = this.#allowedMediaTypeIds;
		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_PICKER_MODAL, {
			data: {
				multiple: false,
				pickableFilter: allowedIds.length ? (item) => allowedIds.includes(item.mediaType.unique) : undefined,
			},
			value: {
				selection: currentMediaUdi ? [currentMediaUdi] : [],
			},
		});
		if (!modalHandler) return undefined;

		const { selection } = await modalHandler.onSubmit().catch(() => ({ selection: undefined }));
		return selection?.[0] ?? undefined;
	}

	async #showMediaCaptionAltText(
		mediaUnique: string,
		altText?: string,
		caption?: string,
		width?: number,
		height?: number,
	) {
		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_CAPTION_ALT_TEXT_MODAL, {
			data: { mediaUnique, maxImageSize: this.maxImageSize },
			value: { url: '', altText, caption, width, height },
			modal: { size: 'medium' }, // Override default sidebar size for better UX
		});
		return modalHandler?.onSubmit().catch(() => undefined);
	}

	async #insertInEditor(editor: Editor, mediaUnique: string, media: UmbMediaCaptionAltTextModalValue) {
		if (!media?.url) return;

		const width = media.width || this.maxImageSize;
		const height = media.height || this.maxImageSize;

		const { data } = await this.#imagingRepository.requestResizedItems([mediaUnique], {
			width,
			height,
			mode: ImageCropModeModel.MAX,
		});

		if (!data?.length || !data[0]?.url) {
			console.error('No data returned from imaging repository');
			return;
		}

		const img = {
			src: data[0].url,
			alt: media.altText,
			'data-udi': `umb://media/${mediaUnique.replace(/-/g, '')}`,
			width: width.toString(),
			height: height.toString(),
		};

		if (media.caption) {
			return editor.commands.insertContent({
				type: 'figure',
				content: [
					{ type: 'paragraph', content: [{ type: 'image', attrs: img }] },
					{ type: 'figcaption', content: [{ type: 'text', text: media.caption }] },
				],
			});
		}

		return editor.commands.setImage(img);
	}
}
