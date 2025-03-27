import type { MediaValueType } from '../../property-editors/upload-field/types.js';
import type { ManifestFileUploadPreview } from './file-upload-preview.extension.js';
import { getMimeTypeFromExtension } from './utils.js';

import {
	UmbFileDropzoneItemStatus,
	UmbInputDropzoneDashedStyles,
	type UmbDropzoneChangeEvent,
	type UmbInputDropzoneElement,
	type UmbUploadableFile,
} from '@umbraco-cms/backoffice/dropzone';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-upload-field')
export class UmbInputUploadFieldElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	set value(value: MediaValueType) {
		this.#src = value?.src ?? '';
		this.#setPreviewAlias();
	}
	get value(): MediaValueType {
		return {
			src: this.#src,
			temporaryFileId: this._file?.temporaryFile.temporaryUnique,
		};
	}
	#src = '';

	/**
	 * @description Allowed file extensions. Allow all if empty.
	 * @type {Array<string>}
	 * @default
	 */
	@property({
		type: Array,
		attribute: 'allowed-file-extensions',
		converter(value) {
			if (typeof value === 'string') {
				return value.split(',').map((ext) => ext.trim());
			}
			return value;
		},
	})
	allowedFileExtensions?: Array<string>;

	@state()
	private _file?: UmbUploadableFile;

	@state()
	private _extensions?: string[];

	@state()
	private _previewAlias?: string;

	@state()
	private _serverUrl = '';

	#manifests: Array<ManifestFileUploadPreview> = [];

	constructor() {
		super();

		this.consumeContext(UMB_APP_CONTEXT, (context) => {
			this._serverUrl = context.getServerUrl();
		});
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#clearObjectUrl();
	}

	async #getManifests() {
		if (this.#manifests.length) return this.#manifests;

		await new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'fileUploadPreview', null, (exts) => {
			this.#manifests = exts.map((exts) => exts.manifest);
		}).asPromise();

		return this.#manifests;
	}

	async #setPreviewAlias(): Promise<void> {
		this._previewAlias = await this.#getPreviewElementAlias();
	}

	async #getPreviewElementAlias() {
		if (!this.value.src) return;
		const manifests = await this.#getManifests();
		const fallbackAlias = manifests.find((manifest) =>
			stringOrStringArrayContains(manifest.forMimeTypes, '*/*'),
		)?.alias;

		let mimeType: string | null = null;
		if (this._file?.temporaryFile.file) {
			mimeType = this._file.temporaryFile.file.type;
		} else {
			mimeType = this.#getMimeTypeFromPath(this.value.src);
		}

		if (!mimeType) return fallbackAlias;

		// Check for an exact match
		const exactMatch = manifests.find((manifest) => {
			return stringOrStringArrayContains(manifest.forMimeTypes, mimeType);
		});
		if (exactMatch) return exactMatch.alias;

		// Check for wildcard match (e.g. image/*)
		const wildcardMatch = manifests.find((manifest) => {
			const forMimeTypes = Array.isArray(manifest.forMimeTypes) ? manifest.forMimeTypes : [manifest.forMimeTypes];
			return forMimeTypes.find((type) => {
				const snippet = type.replace(/\*/g, '');
				if (mimeType.startsWith(snippet)) return manifest.alias;
				if (mimeType.endsWith(snippet)) return manifest.alias;
				return undefined;
			});
		});
		if (wildcardMatch) return wildcardMatch.alias;

		// Use fallbackAlias.
		return fallbackAlias;
	}

	#getMimeTypeFromPath(path: string) {
		// Extract the the MIME type from the data url
		if (path.startsWith('data:')) {
			const mimeType = path.substring(5, path.indexOf(';'));
			return mimeType;
		}

		// Extract the file extension from the path
		const extension = path.split('.').pop()?.toLowerCase();
		if (!extension) return null;
		return getMimeTypeFromExtension('.' + extension);
	}

	async #onUpload(e: UmbDropzoneChangeEvent) {
		e.stopImmediatePropagation();

		const target = e.target as UmbInputDropzoneElement;
		const file = target.value?.[0];

		if (file?.status !== UmbFileDropzoneItemStatus.COMPLETE) return;

		this._file = file as UmbUploadableFile;

		this.#clearObjectUrl();

		const blobUrl = URL.createObjectURL(this._file.temporaryFile.file);
		this.value = { src: blobUrl };

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		if (!this._file && !this.value.src) {
			return this.#renderDropzone();
		}

		if (this.value?.src && this._previewAlias) {
			return this.#renderFile(this.value.src);
		}

		return nothing;
	}

	#renderDropzone() {
		return html`
			<umb-input-dropzone
				id="dropzone"
				disable-folder-upload
				accept=${ifDefined(this._extensions?.join(','))}
				@change=${this.#onUpload}></umb-input-dropzone>
		`;
	}

	#renderFile(src: string) {
		if (!src.startsWith('blob:')) {
			src = this._serverUrl + src;
		}

		return html`
			<div id="wrapper">
				<div id="wrapperInner">
					<umb-extension-slot
						type="fileUploadPreview"
						.props=${{ path: src, file: this._file?.temporaryFile.file }}
						.filter=${(manifest: ManifestFileUploadPreview) => manifest.alias === this._previewAlias}>
					</umb-extension-slot>
				</div>
			</div>
			${this.#renderButtonRemove()}
		`;
	}

	#renderButtonRemove() {
		return html`
			<uui-button compact @click=${this.#handleRemove} label=${this.localize.term('content_uploadClear')}>
				<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
			</uui-button>
		`;
	}

	#handleRemove() {
		// If the upload promise happens to be in progress, cancel it.
		this._file?.temporaryFile.abortController?.abort();

		this.value = { src: undefined };
		this._file = undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * If there is a former File, revoke the object URL.
	 */
	#clearObjectUrl(): void {
		if (this.value?.src?.startsWith('blob:')) {
			URL.revokeObjectURL(this.value.src);
		}
	}

	static override readonly styles = [
		UmbTextStyles,
		UmbInputDropzoneDashedStyles,
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

			#wrapperInner {
				position: relative;
				display: flex;
				width: fit-content;
				max-width: 100%;
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
