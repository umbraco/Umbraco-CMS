import { Extension } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import type { Editor } from '../../externals.js';
import type { UmbTiptapExtensionArgs } from '../types.js';
import { getFileExtension, imageSize } from '@umbraco-cms/backoffice/utils';
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/media';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbMediaTypeStructureRepository, type UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';

export default class UmbTiptapMediaUploadExtensionApi extends UmbTiptapExtensionApiBase {
	#configuration?: UmbPropertyEditorConfigCollection;

	/**
	 * @returns {number} The configured maximum allowed image size
	 */
	get maxImageSize(): number {
		const maxImageSize = parseInt(this.#configuration?.getValueByAlias('maxImageSize') ?? '', 10);
		return isNaN(maxImageSize) ? 500 : maxImageSize;
	}

	/**
	 * @deprecated Use `maxImageSize` instead.
	 * @returns {number} The maximum width of uploaded images
	 */
	maxWidth = this.maxImageSize;

	/**
	 * @returns {Array<string>} The allowed mime types for uploads
	 */
	get allowedFileTypes(): string[] {
		return (
			this.#configuration?.getValueByAlias<string[]>('allowedFileTypes') ?? [
				'image/jpeg',
				'image/png',
				'image/gif',
				'image/webp',
				'image/svg+xml',
				'image/avif',
			]
		);
	}

	/**
	 * @returns {Array<string> | undefined} The allowed media type unique IDs from configuration
	 */
	get #allowedMediaTypeIds(): Array<string> | undefined {
		const raw = this.#configuration?.getValueByAlias<string>('allowedMediaTypes');
		return raw ? raw.split(',') : undefined;
	}

	readonly #manager = new UmbTemporaryFileManager(this);
	readonly #localize = new UmbLocalizationController(this);
	readonly #mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	readonly #mediaTypeCache = new Map<string, Array<UmbAllowedMediaTypeModel>>();
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	getTiptapExtensions(args: UmbTiptapExtensionArgs) {
		this.#configuration = args?.configuration;

		// eslint-disable-next-line @typescript-eslint/no-this-alias
		const self = this;
		return [
			Extension.create({
				name: 'umbMediaUpload',
				onCreate({ editor }) {
					this.parent?.({ editor });
					const host = editor.view.dom;

					host.addEventListener('dragover', (event) => {
						// Required to allow drop events
						event.preventDefault();
					});

					host.addEventListener('drop', (event) => {
						event.preventDefault();

						const files = event.dataTransfer?.files;
						if (!files) return;

						self.#uploadTemporaryFile(files, editor);
					});

					host.addEventListener('paste', (event) => {
						const htmlContent = event.clipboardData?.getData('text/html');
						if (htmlContent) {
							// If there is HTML content, exit early to prevent uploading the remote file(s).
							return;
						}

						const files = event.clipboardData?.files;
						if (!files) return;

						self.#uploadTemporaryFile(files, editor);
					});
				},
			}),
		];
	}

	/**
	 * Uploads the files to the server and inserts them into the editor as data URIs.
	 * The server will replace the data URI with a proper URL when the content is saved.
	 * @param {FileList} files The files to upload.
	 * @param {Editor} editor The editor to insert the images into.
	 */
	async #uploadTemporaryFile(files: FileList, editor: Editor): Promise<void> {
		const filteredFiles = this.#filterFiles(files);

		for (const file of filteredFiles) {
			const isAllowed = await this.#validateMediaType(file);
			if (!isAllowed) continue;

			const fileModel = this.#mapFileToTemporaryFile(file);

			this.dispatchEvent(new CustomEvent('rte.file.uploading', { composed: true, bubbles: true, detail: [fileModel] }));

			const uploads = await this.#manager.upload([fileModel]);
			const upload = uploads[0];

			if (!upload || upload.status !== TemporaryFileStatus.SUCCESS) {
				this.#notificationContext?.peek('danger', {
					data: {
						headline: file.name,
						message: this.#localize.term('errors_dissallowedMediaType'),
					},
				});
				continue;
			}

			const blobUrl = URL.createObjectURL(upload.file);
			const maxImageSize = this.maxImageSize;

			// Get the image dimensions - this essentially simulates what the server would do
			// when it resizes the image. The server will return the resized image URL.
			// We need to use the blob URL here, as the server will not be able to access the local file.
			const { width, height } = await imageSize(blobUrl, { maxWidth: maxImageSize, maxHeight: maxImageSize });

			editor
				.chain()
				.focus()
				.setImage({
					src: blobUrl,
					width: width.toString(),
					height: height.toString(),
					'data-tmpimg': upload.temporaryUnique,
				})
				.run();

			this.dispatchEvent(new CustomEvent('rte.file.uploaded', { composed: true, bubbles: true, detail: [upload] }));
		}
	}

	/**
	 * Validates a file against the allowed media types configuration.
	 * Uses the media type structure repository to check which media types support the file extension,
	 * then intersects with the configured allowedMediaTypes. Shows a type picker modal when ambiguous.
	 * @param {File} file The file to validate.
	 * @returns {boolean} Whether the file is allowed.
	 */
	async #validateMediaType(file: File): Promise<boolean> {
		const allowedIds = this.#allowedMediaTypeIds;
		if (!allowedIds?.length) return true; // No filter configured — allow all

		const extension = getFileExtension(file.name);
		if (!extension) {
			this.#showDisallowedNotification(file.name);
			return false;
		}

		const availableMediaTypes = await this.#getAvailableMediaTypesOf(extension);
		if (!availableMediaTypes.length) {
			this.#showDisallowedNotification(file.name);
			return false;
		}

		// Intersect available media types with the configured allowed types
		const options = availableMediaTypes.filter((type) => type.unique && allowedIds.includes(type.unique));

		if (!options.length) {
			this.#showDisallowedNotification(file.name);
			return false;
		}

		if (options.length === 1) return true;

		// Multiple matches — prefer specific extension matches over fallbacks
		const specificMatches = options.filter((x) => x.matchedFileExtension === true);

		if (specificMatches.length === 1) return true;

		if (specificMatches.length > 1) {
			// Multiple specific matches — let the user pick
			const picked = await this.#showMediaTypePicker(specificMatches);
			return !!picked;
		}

		// All fallbacks — auto-select
		return true;
	}

	async #getAvailableMediaTypesOf(extension: string): Promise<Array<UmbAllowedMediaTypeModel>> {
		const cached = this.#mediaTypeCache.get(extension);
		if (cached) return cached;

		try {
			const result = await this.#mediaTypeStructure.requestMediaTypesOf({ fileExtension: extension });
			this.#mediaTypeCache.set(extension, result);
			return result;
		} catch {
			return [];
		}
	}

	async #showMediaTypePicker(options: Array<UmbAllowedMediaTypeModel>): Promise<string | undefined> {
		const value = await umbOpenModal(this, UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL, { data: { options } }).catch(
			() => undefined,
		);
		return value?.mediaTypeUnique;
	}

	#showDisallowedNotification(fileName: string) {
		this.#notificationContext?.peek('warning', {
			data: {
				message: `${this.#localize.term('media_disallowedFileType')} (${fileName})`,
			},
		});
	}

	#mapFileToTemporaryFile(file: File): UmbTemporaryFileModel {
		return {
			file,
			temporaryUnique: UmbId.new(),
		};
	}

	#filterFiles(files: FileList): File[] {
		return Array.from(files).filter((file) => this.allowedFileTypes.includes(file.type));
	}
}
