import { UmbDropzoneMediaManager } from './dropzone-media-manager.class.js';
import {
	UmbInputDropzoneElement,
	UmbFileDropzoneItemStatus,
	UmbDropzoneSubmittedEvent,
	type UmbUploadableItem,
} from '@umbraco-cms/backoffice/dropzone';
import { css, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * A dropzone for uploading files and folders as media items. It is hidden by default and will be shown when dragging files over the window.
 * @element umb-dropzone
 * @fires ProgressEvent When the progress of the upload changes.
 * @fires UmbDropzoneSubmittedEvent When the upload is submitted.
 * @fires UmbDropzoneChangeEvent When any upload changes.
 * @fires CustomEvent<'complete'> When all uploads are complete (deprecated: use {@link UmbDropzoneChangeEvent} instead).
 * @slot - The default slot.
 */
@customElement('umb-dropzone')
export class UmbDropzoneElement extends UmbInputDropzoneElement {
	@property({ attribute: 'parent-unique' })
	parentUnique: string | null = null;

	/**
	 * Determines if the dropzone should create temporary files or media items directly.
	 * @deprecated Use the {@link UmbInputDropzoneElement} instead.
	 */
	@property({ type: Boolean, attribute: 'create-as-temporary' })
	createAsTemporary: boolean = false;

	#dropzoneManager = new UmbDropzoneMediaManager(this);

	/**
	 * @deprecated Please use `getItems()` instead; this method will be removed in Umbraco 17.
	 * @returns {Array<UmbUploadableItem>} An array of uploadable items.
	 */
	public getFiles = this.getItems;

	/**
	 * Gets the current value of the uploaded items.
	 * @returns {Array<UmbUploadableItem>} An array of uploadable items.
	 */
	public getItems(): Array<UmbUploadableItem> {
		return this._progressItems;
	}

	public progressItems = () => this.#dropzoneManager.progressItems;
	public progress = () => this.#dropzoneManager.progress;

	public browse() {
		if (this.disabled) return;
		const element = this.shadowRoot?.querySelector('#dropzone') as UUIFileDropzoneElement;
		return element.browse();
	}

	constructor() {
		super();

		document.addEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.addEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.addEventListener('drop', this.#handleDrop.bind(this));

		this.observe(
			this.#dropzoneManager.progressItems,
			(progressItems: Array<UmbUploadableItem>) => {
				const waiting = progressItems.find((item) => item.status === UmbFileDropzoneItemStatus.WAITING);
				if (progressItems.length && !waiting) {
					this.dispatchEvent(new CustomEvent('complete', { detail: progressItems }));
				}
			},
			'_observeProgressItems',
		);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#dropzoneManager.destroy();
		document.removeEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.removeEventListener('drop', this.#handleDrop.bind(this));
	}

	override async onUpload(event: UUIFileDropzoneEvent) {
		if (this.disabled) return;
		if (!event.detail.files.length && !event.detail.folders.length) return;

		if (this.createAsTemporary) {
			const uploadable = this.#dropzoneManager.createTemporaryFiles(event.detail.files);
			this.dispatchEvent(new UmbDropzoneSubmittedEvent(await uploadable));
		} else {
			const uploadable = this.#dropzoneManager.createMediaItems(event.detail, this.parentUnique);
			this.dispatchEvent(new UmbDropzoneSubmittedEvent(uploadable));
		}
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

	static override styles = [
		...UmbInputDropzoneElement.styles,
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
				z-index: 100;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-focus);
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
