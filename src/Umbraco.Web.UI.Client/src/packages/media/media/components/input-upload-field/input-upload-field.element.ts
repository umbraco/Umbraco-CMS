import type { MediaValueType } from '../../property-editors/upload-field/types.js';
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	css,
	html,
	nothing,
	ifDefined,
	customElement,
	property,
	query,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import './input-upload-field-file.element.js';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { type ManifestFileUploadPreview, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-upload-field')
export class UmbInputUploadFieldElement extends UmbLitElement {
	@property({ type: Object })
	set value(value: MediaValueType) {
		if (!value?.src) return;
		this._src = value.src;
	}
	get value(): MediaValueType {
		return !this.temporaryFile ? { src: this._src } : { temporaryFileId: this.temporaryFile.temporaryUnique };
	}

	/**
	 * @description Allowed file extensions. Allow all if empty.
	 * @type {Array<string>}
	 * @default
	 */
	@property({ type: Array })
	set allowedFileExtensions(value: Array<string>) {
		this.#setExtensions(value);
	}
	get allowedFileExtensions(): Array<string> | undefined {
		return this._extensions;
	}

	@state()
	public temporaryFile?: UmbTemporaryFileModel;

	@state()
	private _src = '';

	@state()
	private _extensions?: string[];

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	#manager = new UmbTemporaryFileManager(this);

	#previewers = new UmbArrayState(<Array<ManifestFileUploadPreview>>[], (x) => x.alias);

	constructor() {
		super();
		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'fileUploadPreview', null, (previews) => {
			previews.forEach((preview) => {
				this.#previewers.appendOne(preview.manifest);
			});
		});
	}

	#setExtensions(extensions: Array<string>) {
		if (!extensions?.length) {
			this._extensions = undefined;
			return;
		}
		// TODO: The dropzone uui component does not support file extensions without a dot. Remove this when it does.
		this._extensions = extensions?.map((extension) => `.${extension}`);
	}

	async #onUpload(e: UUIFileDropzoneEvent) {
		//Property Editor for Upload field will always only have one file.
		const item: UmbTemporaryFileModel = {
			temporaryUnique: UmbId.new(),
			file: e.detail.files[0],
		};

		const upload = this.#manager.uploadOne(item);

		const reader = new FileReader();
		reader.onload = () => {
			this._src = reader.result as string;
		};
		reader.readAsDataURL(item.file);

		const uploaded = await upload;
		if (uploaded.status === TemporaryFileStatus.SUCCESS) {
			this.temporaryFile = { temporaryUnique: item.temporaryUnique, file: item.file };
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	#handleBrowse(e: Event) {
		if (!this._dropzone) return;
		e.stopImmediatePropagation();
		this._dropzone.browse();
	}

	override render() {
		return html`${this._src ? this.#renderFile(this._src, this.temporaryFile?.file) : this.#renderDropzone()}`;
	}

	#renderDropzone() {
		return html`
			<uui-file-dropzone
				@click=${this.#handleBrowse}
				id="dropzone"
				label="dropzone"
				@change="${this.#onUpload}"
				accept="${ifDefined(this._extensions?.join(', '))}">
				<uui-button label=${this.localize.term('media_clickToUpload')} @click="${this.#handleBrowse}"></uui-button>
			</uui-file-dropzone>
		`;
	}

	#renderFile(src: string, file?: File) {
		const extension = this.#getFileExtensionFromPath(src);
		const element = this.#getElementFromFilePath(src);
		console.log('element', element);

		return html`
			<div id="wrapper">
				<div style="position:relative; display: flex; width: fit-content; max-width: 100%">
					${getElementTemplate()}
					${this.temporaryFile?.status === TemporaryFileStatus.WAITING
						? html`<umb-temporary-file-badge></umb-temporary-file-badge>`
						: nothing}
				</div>
			</div>
			${this.#renderButtonRemove()}
		`;

		/**
		 * @returns {string} The template for the file extension.
		 */
		function getElementTemplate() {
			switch (extension) {
				case 'audio':
					return html`<umb-input-upload-field-audio .path=${src}></umb-input-upload-field-audio>`;
				case 'video':
					return html`<umb-input-upload-field-video .path=${src}></umb-input-upload-field-video>`;
				case 'image':
					return html`<umb-input-upload-field-image .path=${src}></umb-input-upload-field-image>`;
				case 'svg':
					return html`<umb-input-upload-field-svg .path=${src}></umb-input-upload-field-svg>`;
				default:
					return html`<umb-input-upload-field-file .path=${src} .file=${file}></umb-input-upload-field-file>`;
			}
		}
	}

	#getElementFromFilePath(path: string) {
		const previews = this.#previewers.getValue();
		const fallbackElement = previews.find((preview) => !preview.forMimeTypes?.length)?.element;

		// Extract the the MIME type from the data url and get corresponding previewer.
		if (path.startsWith('data:')) {
			const mimeType = path.substring(5, path.indexOf(';'));

			const manifest = previews.find((preview) => {
				return preview.forMimeTypes?.find((type) => {
					const snippet = type.replace('*', '');
					if (mimeType.startsWith(snippet)) return preview;
					if (mimeType.endsWith(snippet)) return preview;
					return undefined;
				});
			});

			if (manifest) return manifest.element;
			return fallbackElement;
		}

		// Extract the file extension from the path
		const extension = path.split('.').pop()?.toLowerCase();
		if (!extension) return fallbackElement;

		return fallbackElement;
	}

	#getFileExtensionFromPath(path: string): 'audio' | 'video' | 'image' | 'svg' | 'file' {
		// Extract the MIME type from the data URL
		if (path.startsWith('data:')) {
			const mimeType = path.substring(5, path.indexOf(';'));
			if (mimeType === 'image/svg+xml') return 'svg';
			if (mimeType.startsWith('image/')) return 'image';
			if (mimeType.startsWith('audio/')) return 'audio';
			if (mimeType.startsWith('video/')) return 'video';
		}

		// Extract the file extension from the path
		const extension = path.split('.').pop()?.toLowerCase();
		if (!extension) return 'file';
		if (['svg'].includes(extension)) return 'svg';
		if (['mp3', 'weba', 'oga', 'opus'].includes(extension)) return 'audio';
		if (['mp4', 'mov', 'webm', 'ogv'].includes(extension)) return 'video';
		if (['jpg', 'jpeg', 'png', 'gif'].includes(extension)) return 'image';

		return 'file';
	}

	#renderButtonRemove() {
		return html`<uui-button compact @click=${this.#handleRemove} label=${this.localize.term('content_uploadClear')}>
			<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
		</uui-button>`;
	}

	#handleRemove() {
		this._src = '';
		this.temporaryFile = undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	static override styles = [
		css`
			:host {
				position: relative;
			}
			uui-icon {
				vertical-align: sub;
				margin-right: var(--uui-size-space-4);
			}

			#wrapper {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
				gap: var(--uui-size-space-4);
				box-sizing: border-box;
			}

			#wrapper:has(umb-input-upload-field-file) {
				padding: var(--uui-size-space-4);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
			}

			uui-file-dropzone {
				position: relative;
				display: block;
				padding: 3px; /** Dropzone background is blurry and covers slightly into other elements. Hack to avoid this */
			}
			uui-file-dropzone::after {
				content: '';
				position: absolute;
				inset: 0;
				cursor: pointer;
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbInputUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field': UmbInputUploadFieldElement;
	}
}
