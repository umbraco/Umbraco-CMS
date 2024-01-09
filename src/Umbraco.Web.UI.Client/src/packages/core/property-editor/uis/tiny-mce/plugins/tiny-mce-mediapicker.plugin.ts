import { TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/components';
import { UmbMediaHelper } from '@umbraco-cms/backoffice/utils';
import {
	UMB_MEDIA_TREE_PICKER_MODAL,
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/modal';
import { UMB_CURRENT_USER_CONTEXT, UmbCurrentUser } from '@umbraco-cms/backoffice/current-user';
import { RawEditorOptions } from '@umbraco-cms/backoffice/external/tinymce';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';

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
	#mediaHelper: UmbMediaHelper;
	#currentUser?: UmbCurrentUser;
	#modalContext?: UmbModalManagerContext;
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#temporaryFileRepository;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.#mediaHelper = new UmbMediaHelper();
		this.#temporaryFileRepository = new UmbTemporaryFileRepository(args.host);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (modalContext) => {
			this.#modalContext = modalContext;
		});

		// TODO => this breaks tests. disabling for now
		// will ignore user media start nodes
		// this.host.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
		// 	this.#currentUserContext = instance;
		// 	this.#observeCurrentUser();
		// });

		this.editor.ui.registry.addToggleButton('umbmediapicker', {
			icon: 'image',
			tooltip: 'Media Picker',
			onAction: () => this.#onAction(),
			onSetup: (api) => {
				const changed = this.editor.selection.selectorChangedWithUnbind('img[data-udi]', (state) =>
					api.setActive(state),
				);
				return () => changed;
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

			// Listen for SetContent to update images
			this.editor.on('SetContent', async (e) => {
				const content = e.content;
				console.log('🚀 ~ UmbTinyMceMediaPickerPlugin ~ this.editor.on ~ content:', content);

				// Upload BLOB images (dragged/pasted ones)
				// find src attribute where value starts with `blob:`
				// search is case-insensitive and allows single or double quotes
				if (content.search(/src=["']blob:.*?["']/gi) !== -1) {
					const data = await this.editor.uploadImages();

					// Once all images have been uploaded
					for (const item of data) {
						// Skip items that failed upload
						if (item.status === false) {
							return;
						}

						// Select img element
						const img = item.element;

						// Get img src
						const imgSrc = img.getAttribute('src');
						const tmpLocation = sessionStorage.getItem(`tinymce__${imgSrc}`);

						// Select the img & add new attr which we can search for
						// When its being persisted in RTE property editor
						// To create a media item & delete this tmp one etc
						this.editor.dom.setAttrib(img, 'data-tmpimg', tmpLocation);

						// Resize the image to the max size configured
						// NOTE: no imagesrc passed into func as the src is blob://...
						// We will append ImageResizing Querystrings on perist to DB with node save
						this.#mediaHelper.sizeImageInEditor(args.editor, img);
					}

					// Get all img where src starts with blob: AND does NOT have a data=tmpimg attribute
					// This is most likely seen as a duplicate image that has already been uploaded
					// editor.uploadImages() does not give us any indiciation that the image been uploaded already
					const blobImageWithNoTmpImgAttribute = args.editor.dom.select("img[src^='blob:']:not([data-tmpimg])");

					//For each of these selected items
					blobImageWithNoTmpImgAttribute.forEach((imageElement) => {
						const blobSrcUri = args.editor.dom.getAttrib(imageElement, 'src');

						// Find the same image uploaded (Should be in LocalStorage)
						// May already exist in the editor as duplicate image
						// OR added to the RTE, deleted & re-added again
						// So lets fetch the tempurl out of localstorage for that blob URI item
						const tmpLocation = sessionStorage.getItem(`tinymce__${blobSrcUri}`);

						if (tmpLocation) {
							this.#mediaHelper.sizeImageInEditor(this.editor, imageElement);
							args.editor.dom.setAttrib(imageElement, 'data-tmpimg', tmpLocation);
						}
					});
				}
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
		// TODO: I dont think we should parse this one... it should be up to the modal to get this information, and then we could parse some configs on to affect this.
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

		// TODO => startNodeId and startNodeIsVirtual do not exist on ContentTreeItemResponseModel
		const modalHandler = this.#modalContext?.open(UMB_MEDIA_TREE_PICKER_MODAL, {
			data: {
				multiple: false,
				//startNodeId,
				//startNodeIsVirtual,
			},
			value: {
				selection: currentTarget.udi ? [...currentTarget.udi] : [],
			},
		});

		if (!modalHandler) return;

		const { selection } = await modalHandler.onSubmit();
		if (!selection.length) return;

		this.#insertInEditor(selection[0]);
		this.editor.dispatch('Change');
	}

	// TODO => mediaPicker returns a UDI, so need to fetch it. Wait for backend CLI before implementing
	async #insertInEditor(img: any) {
		if (!img) return;

		// We need to create a NEW DOM <img> element to insert
		// setting an attribute of ID to __mcenew, so we can gather a reference to the node, to be able to update its size accordingly to the size of the image.
		const data: MediaPickerResultData = {
			alt: img.altText || '',
			src: img.url ? img.url : 'nothing.jpg',
			id: '__mcenew',
			'data-udi': img.udi,
			'data-caption': img.caption,
		};
		const newImage = this.editor.dom.createHTML('img', data as Record<string, string | null>);
		const parentElement = this.editor.selection.getNode().parentElement;

		if (img.caption && parentElement) {
			const figCaption = this.editor.dom.createHTML('figcaption', {}, img.caption);
			const combined = newImage + figCaption;

			if (parentElement.nodeName !== 'FIGURE') {
				const fragment = this.editor.dom.createHTML('figure', {}, combined);
				this.editor.selection.setContent(fragment);
			} else {
				parentElement.innerHTML = combined;
			}
		} else {
			//if caption is removed, remove the figure element
			if (parentElement?.nodeName === 'FIGURE' && parentElement.parentElement) {
				parentElement.parentElement.innerHTML = newImage;
			} else {
				this.editor.selection.setContent(newImage);
			}
		}

		// Using settimeout to wait for a DoM-render, so we can find the new element by ID.
		setTimeout(() => {
			const imgElm = this.editor.dom.get('__mcenew') as HTMLImageElement;
			if (!imgElm) return;

			this.editor.dom.setAttrib(imgElm, 'id', null);

			// When image is loaded we are ready to call sizeImageInEditor.
			const onImageLoaded = () => {
				this.#mediaHelper?.sizeImageInEditor(this.editor, imgElm, img.url);
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

	#uploadImageHandler: RawEditorOptions['images_upload_handler'] = (blobInfo, progress) => {
		return new Promise((resolve, reject) => {
			// Fetch does not support progress, so we need to fake it.
			progress(0);

			const id = UmbId.new();
			const fileBlob = blobInfo.blob();
			const file = new File([fileBlob], blobInfo.filename(), { type: fileBlob.type });

			progress(50);

			this.#temporaryFileRepository
				.upload(id, file)
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
				.finally(() => progress(100));
		});
	};
}
