import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '../components/input-tiny-mce/tiny-mce-plugin.js';
import { getGuidFromUdi } from '@umbraco-cms/backoffice/utils';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { RawEditorOptions } from '@umbraco-cms/backoffice/external/tinymce';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	sizeImageInEditor,
	uploadBlobImages,
	UMB_MEDIA_PICKER_MODAL,
	UMB_MEDIA_CAPTION_ALT_TEXT_MODAL,
} from '@umbraco-cms/backoffice/media';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

interface MediaPickerTargetData {
	altText?: string;
	url?: string;
	caption?: string;
	udi?: string;
	id?: string;
	tmpimg?: string;
}

interface MediaPickerResultData {
	id?: string;
	src?: string;
	alt?: string;
	'data-udi'?: string;
	'data-caption'?: string;
}

export default class UmbTinyMceMediaPickerPlugin extends UmbTinyMcePluginBase {
	#modalManager?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;
	readonly #temporaryFileRepository;

	constructor(args: TinyMcePluginArguments) {
		super(args);
		const localize = new UmbLocalizationController(args.host);

		this.#temporaryFileRepository = new UmbTemporaryFileRepository(args.host);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManager = instance;
		});

		this.editor.ui.registry.addToggleButton('umbmediapicker', {
			icon: 'image',
			tooltip: localize.term('general_mediaPicker'),
			onAction: () => this.#onAction(),
			onSetup: (api) => {
				const changed = this.editor.selection.selectorChangedWithUnbind('img[data-udi]', (state) =>
					api.setActive(state),
				);
				return () => changed.unbind();
			},
		});

		// Register global options for the editor
		this.editor.options.register('maxImageSize', { processor: 'number', default: 500 });

		// Adjust Editor settings to allow pasting images
		// but only if the umbmediapicker button is present
		const toolbar = this.configuration?.getValueByAlias<string[]>('toolbar');
		if (toolbar?.includes('umbmediapicker')) {
			this.editor.options.set('paste_data_images', true);
			this.editor.options.set('automatic_uploads', false);
			this.editor.options.set('images_upload_handler', this.#uploadImageHandler);
			// This allows images to be pasted in & stored as Base64 until they get uploaded to server
			this.editor.options.set('images_replace_blob_uris', true);

			// Listen for SetContent to update images
			this.editor.on('SetContent', async (e) => {
				const content = e.content;

				// Handle images that are pasted in
				uploadBlobImages(this.editor, content);
			});
		}
	}

	/*
	async #observeCurrentUser() {
		if (!this.#currentUserContext) return;

		this.observe(this.#currentUserContext.currentUser, (currentUser) => (this.#currentUser = currentUser));
	}
	*/

	async #onAction() {
		const selectedElm = this.editor.selection.getNode();
		let currentTarget: MediaPickerTargetData = {};

		if (selectedElm.nodeName === 'IMG') {
			const img = selectedElm as HTMLImageElement;
			const hasUdi = img.hasAttribute('data-udi');
			const hasDataTmpImg = img.hasAttribute('data-tmpimg');

			currentTarget = {
				altText: img.alt,
				url: img.src,
				caption: img.dataset.caption,
			};

			if (hasUdi) {
				currentTarget['udi'] = img.dataset.udi;
			} else {
				currentTarget['id'] = img.getAttribute('rel') ?? undefined;
			}

			if (hasDataTmpImg) {
				currentTarget['tmpimg'] = img.dataset.tmpimg;
			}
		}

		this.#showMediaPicker(currentTarget);
	}

	async #showMediaPicker(currentTarget: MediaPickerTargetData) {
		/*
		let startNodeId;
		let startNodeIsVirtual;

		if (!this.configuration?.getByAlias('startNodeId')) {
			if (this.configuration?.getValueByAlias<boolean>('ignoreUserStartNodes') === true) {
				startNodeId = -1;
				startNodeIsVirtual = true;
			} else {
				startNodeId = this.#currentUser?.mediaStartNodeIds?.length !== 1 ? -1 : this.#currentUser?.mediaStartNodeIds[0];
				startNodeIsVirtual = this.#currentUser?.mediaStartNodeIds?.length !== 1;
			}
		}
		*/

		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_PICKER_MODAL, {
			data: {
				multiple: false,
				//startNodeIsVirtual,
			},
			value: {
				selection: currentTarget.udi ? [getGuidFromUdi(currentTarget.udi)] : [],
			},
		});

		if (!modalHandler) return;

		const { selection } = await modalHandler.onSubmit().catch(() => ({ selection: undefined }));
		if (!selection?.length) return;

		this.#showMediaCaptionAltText(selection[0], currentTarget);
		this.editor.dispatch('Change');
	}

	async #showMediaCaptionAltText(mediaUnique: string | null, currentTarget: MediaPickerTargetData) {
		if (!mediaUnique) return;

		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_CAPTION_ALT_TEXT_MODAL, {
			data: { mediaUnique },
			value: {
				url: '',
				altText: currentTarget.altText,
				caption: currentTarget.caption,
			},
		});

		const mediaData = await modalHandler?.onSubmit().catch(() => null);
		if (!mediaData) return;

		const media: MediaPickerTargetData = {
			altText: mediaData?.altText,
			caption: mediaData?.caption,
			url: mediaData?.url,
			udi: 'umb://media/' + mediaUnique?.replace(/-/g, ''),
		};

		this.#insertInEditor(media);
	}

	async #insertInEditor(media: MediaPickerTargetData) {
		if (!media) return;

		// We need to create a NEW DOM <img> element to insert
		// setting an attribute of ID to __mcenew, so we can gather a reference to the node, to be able to update its size accordingly to the size of the image.
		const img: MediaPickerResultData = {
			alt: media.altText,
			src: media.url ? media.url : 'nothing.jpg',
			id: '__mcenew',
			'data-udi': media.udi,
			'data-caption': media.caption,
		};
		const newImage = this.editor.dom.createHTML('img', img as Record<string, string | null>);
		const parentElement = this.editor.selection.getNode().parentElement;

		if (img['data-caption'] && parentElement) {
			const figCaption = this.editor.dom.createHTML('figcaption', {}, img['data-caption']);
			const combined = newImage + figCaption;

			if (parentElement.nodeName !== 'FIGURE') {
				const fragment = this.editor.dom.createHTML('figure', {}, combined);
				this.editor.selection.setContent(fragment);
			} else {
				parentElement.innerHTML = combined;
			}
		} else if (parentElement?.nodeName === 'FIGURE' && parentElement.parentElement) {
			//if caption is removed, remove the figure element
			parentElement.parentElement.innerHTML = newImage;
		} else {
			this.editor.selection.setContent(newImage);
		}

		// Using settimeout to wait for a DoM-render, so we can find the new element by ID.
		setTimeout(() => {
			const imgElm = this.editor.dom.get('__mcenew') as HTMLImageElement;
			if (!imgElm) return;

			this.editor.dom.setAttrib(imgElm, 'id', null);

			// When image is loaded we are ready to call sizeImageInEditor.
			const onImageLoaded = () => {
				sizeImageInEditor(this.editor, imgElm, img.src);
				this.editor.dispatch('Change');
			};

			// Check if image already is loaded.
			if (imgElm.complete === true) {
				onImageLoaded();
			} else {
				imgElm.onload = onImageLoaded;
			}
		});
	}

	readonly #uploadImageHandler: RawEditorOptions['images_upload_handler'] = (blobInfo, progress) => {
		return new Promise((resolve, reject) => {
			progress(0);

			const id = UmbId.new();
			const fileBlob = blobInfo.blob();
			const file = new File([fileBlob], blobInfo.filename(), { type: fileBlob.type });

			document.dispatchEvent(new CustomEvent('rte.file.uploading', { composed: true, bubbles: true }));

			this.#temporaryFileRepository
				.upload(id, file, (evt) => {
					progress((evt.loaded / evt.total) * 100);
				})
				.then((response) => {
					if (response.error) {
						reject(response.error);
						return;
					}

					// Put temp location into localstorage (used to update the img with data-tmpimg later on)
					const blobUri = window.URL.createObjectURL(fileBlob);
					sessionStorage.setItem(`tinymce__${blobUri}`, id);
					resolve(blobUri);
				})
				.catch(reject)
				.finally(() => {
					progress(100);
					document.dispatchEvent(new CustomEvent('rte.file.uploaded', { composed: true, bubbles: true }));
				});
		});
	};
}
