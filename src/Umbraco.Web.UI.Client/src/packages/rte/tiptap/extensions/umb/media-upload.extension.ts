import { UmbTiptapExtensionApiBase, type UmbTiptapExtensionArgs } from '../types.js';
import {
	TemporaryFileStatus,
	UmbTemporaryFileManager,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { imageSize } from '@umbraco-cms/backoffice/utils';
import { type Editor, Extension } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export default class UmbTiptapMediaUploadExtension extends UmbTiptapExtensionApiBase {
	#configuration?: UmbPropertyEditorConfigCollection;

	/**
	 * @returns {number} The maximum width of uploaded images
	 */
	get maxWidth(): number {
		const maxImageSize = parseInt(this.#configuration?.getValueByAlias('maxImageSize') ?? '', 10);
		return isNaN(maxImageSize) ? 500 : maxImageSize;
	}

	/**
	 * @returns {Array<string>} The allowed mime types for uploads
	 */
	get allowedFileTypes(): string[] {
		return (
			this.#configuration?.getValueByAlias<string[]>('allowedFileTypes') ?? ['image/jpeg', 'image/png', 'image/gif']
		);
	}

	#manager = new UmbTemporaryFileManager(this);
	#localize = new UmbLocalizationController(this);
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
				onCreate() {
					this.parent?.();
					const host = this.editor.view.dom;

					host.addEventListener('dragover', (event) => {
						// Required to allow drop events
						event.preventDefault();
					});

					host.addEventListener('drop', (event) => {
						event.preventDefault();

						const files = event.dataTransfer?.files;
						if (!files) return;

						self.#uploadTemporaryFile(files, this.editor);
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
		const fileModels = filteredFiles.map((file) => this.#mapFileToTemporaryFile(file));

		this.dispatchEvent(new CustomEvent('rte.file.uploading', { composed: true, bubbles: true, detail: fileModels }));

		const uploads = await this.#manager.upload(fileModels);
		const maxImageSize = this.maxWidth;

		uploads.forEach(async (upload) => {
			if (upload.status !== TemporaryFileStatus.SUCCESS) {
				this.#notificationContext?.peek('danger', {
					data: {
						headline: upload.file.name,
						message: this.#localize.term('errors_dissallowedMediaType'),
					},
				});
				return;
			}

			let { width, height } = await imageSize(URL.createObjectURL(upload.file));

			if (maxImageSize > 0 && width > maxImageSize) {
				const ratio = maxImageSize / width;
				width = maxImageSize;
				height = Math.round(height * ratio);
			}

			editor
				.chain()
				.focus()
				.setImage({
					src: URL.createObjectURL(upload.file),
					width: width.toString(),
					height: height.toString(),
					'data-tmpimg': upload.temporaryUnique,
				})
				.run();
		});

		this.dispatchEvent(new CustomEvent('rte.file.uploaded', { composed: true, bubbles: true, detail: uploads }));
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
