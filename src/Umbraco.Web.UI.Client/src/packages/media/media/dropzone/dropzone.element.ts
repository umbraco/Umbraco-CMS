import { UmbDropzoneManager } from './dropzone-manager.class.js';
import { UmbFileDropzoneItemStatus, type UmbUploadableItem } from './types.js';
import { css, customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-dropzone')
export class UmbDropzoneElement extends UmbLitElement {
	@property({ attribute: false })
	parentUnique: string | null = null;

	@property({ type: Boolean })
	multiple: boolean = true;

	@property({ type: Boolean })
	createAsTemporary: boolean = false;

	@property({ type: String })
	accept?: string;

	@property({ type: Boolean, reflect: true })
	disabled = false;

	@property({ type: Boolean, attribute: 'disable-folder-upload', reflect: true })
	public get disableFolderUpload() {
		return this._disableFolderUpload;
	}
	public set disableFolderUpload(isAllowed: boolean) {
		this.dropzoneManager.setIsFoldersAllowed(!isAllowed);
	}
	private readonly _disableFolderUpload = false;

	@state()
	private _progressItems: Array<UmbUploadableItem> = [];

	public dropzoneManager: UmbDropzoneManager;

	/**
	 * @deprecated Please use `getItems()` instead; this method will be removed in Umbraco 17.
	 * @returns {Array<UmbUploadableItem>} An array of uploadable items.
	 */
	public getFiles() {
		return this.getItems();
	}

	public getItems() {
		return this._progressItems;
	}

	public browse() {
		if (this.disabled) return;
		const element = this.shadowRoot?.querySelector('#dropzone') as UUIFileDropzoneElement;
		return element.browse();
	}

	constructor() {
		super();
		this.dropzoneManager = new UmbDropzoneManager(this);
		document.addEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.addEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.addEventListener('drop', this.#handleDrop.bind(this));
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.dropzoneManager.destroy();
		document.removeEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.removeEventListener('drop', this.#handleDrop.bind(this));
	}

	#handleDragEnter(e: DragEvent) {
		if (this.disabled) return;
		// Avoid collision with UmbSorterController
		const types = e.dataTransfer?.types;
		if (!types?.length || !types?.includes('Files')) return;

		this.toggleAttribute('dragging', true);
	}

	#handleDragLeave() {
		if (this.disabled) return;
		this.toggleAttribute('dragging', false);
	}

	#handleDrop(event: DragEvent) {
		event.preventDefault();
		if (this.disabled) return;
		this.toggleAttribute('dragging', false);
	}

	async #onDropFiles(event: UUIFileDropzoneEvent) {
		if (this.disabled) return;
		if (!event.detail.files.length && !event.detail.folders.length) return;

		// TODO Create some placeholder items while files are being uploaded? Could update them as they get completed.
		// We can observe progressItems and check for any files that did not succeed, then show some kind of dialog to the user with the information.

		this.observe(
			this.dropzoneManager.progress,
			(progress) =>
				this.dispatchEvent(new ProgressEvent('progress', { loaded: progress.completed, total: progress.total })),
			'_observeProgress',
		);

		this.observe(this.dropzoneManager.progressItems, (progressItems: Array<UmbUploadableItem>) => {
			this._progressItems = progressItems;
			const waiting = progressItems.find((item) => item.status === UmbFileDropzoneItemStatus.WAITING);
			if (progressItems.length && !waiting) {
				this.dispatchEvent(new CustomEvent('complete', { detail: progressItems }));
			}
		});

		if (this.createAsTemporary) {
			this.dropzoneManager.createTemporaryFiles(event.detail.files);
		} else {
			this.dropzoneManager.createMediaItems(event.detail, this.parentUnique);
		}
	}

	override render() {
		return html`<uui-file-dropzone
			id="dropzone"
			accept=${ifDefined(this.accept)}
			?multiple=${this.multiple}
			@change=${this.#onDropFiles}
			label=${this.localize.term('media_dragAndDropYourFilesIntoTheArea')}></uui-file-dropzone>`;
	}

	static override styles = [
		css`
			:host(:not([disabled])[dragging]) #dropzone {
				opacity: 1;
				pointer-events: all;
			}

			[dropzone] {
				opacity: 0;
			}

			#dropzone {
				opacity: 0;
				pointer-events: none;
				display: flex;
				align-items: center;
				justify-content: center;
				position: absolute;
				inset: 0px;
				z-index: 100;
				backdrop-filter: opacity(1); /* Removes the built in blur effect */
				border-radius: var(--uui-border-radius);
				overflow: clip;
				border: 1px solid var(--uui-color-focus);
			}
			#dropzone:after {
				content: '';
				display: block;
				position: absolute;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-focus);
				opacity: 0.2;
			}
		`,
	];
}

export default UmbDropzoneElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone': UmbDropzoneElement;
	}
}
