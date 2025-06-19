import { UmbMediaDropzoneManager } from './media-dropzone.manager.js';
import {
	UmbInputDropzoneElement,
	UmbFileDropzoneItemStatus,
	UmbDropzoneSubmittedEvent,
	type UmbUploadableItem,
} from '@umbraco-cms/backoffice/dropzone';
import { css, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * A dropzone for uploading files and folders as media items. It is hidden by default and will be shown when dragging files over the window.
 * @element umb-dropzone-media
 * @fires ProgressEvent When the progress of the upload changes.
 * @fires UmbDropzoneSubmittedEvent When the upload is submitted.
 * @fires UmbDropzoneChangeEvent When any upload changes.
 * @fires CustomEvent<'complete'> When all uploads are complete (deprecated: use {@link UmbDropzoneChangeEvent} instead).
 * @slot - The default slot.
 */
@customElement('umb-dropzone-media')
export class UmbDropzoneMediaElement extends UmbInputDropzoneElement {
	@property({ attribute: 'parent-unique' })
	parentUnique: string | null = null;

	/**
	 * Gets the current value of the uploaded items.
	 * @returns {Array<UmbUploadableItem>} An array of uploadable items.
	 */
	public getItems(): Array<UmbUploadableItem> {
		return this._progressItems;
	}

	protected override _manager = new UmbMediaDropzoneManager(this);
	public progressItems = () => this._manager.progressItems;
	public progress = () => this._manager.progress;

	constructor() {
		super();

		document.addEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.addEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.addEventListener('drop', this.#handleDrop.bind(this));

		// TODO: Revisit this. I am not sure why it is needed to call these methods here when they are already called in the constructor of the parent class.
		// If we do not call them here, the observer will use the wrong instance of the dropzone manager (UmbDropZoneManager instead of UmbMediaDropzoneManager).
		this._observeProgress();
		this._observeProgressItems();
	}

	protected override _observeProgressItems() {
		super._observeProgressItems();
		this.observe(
			this._manager.progressItems,
			(progressItems: Array<UmbUploadableItem>) => {
				const waiting = progressItems.find((item) => item.status === UmbFileDropzoneItemStatus.WAITING);
				if (progressItems.length && !waiting) {
					this.dispatchEvent(new CustomEvent('complete', { detail: progressItems }));
				}
			},
			'_observeProgressItemsComplete',
		);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.removeEventListener('drop', this.#handleDrop.bind(this));
	}

	override async onUpload(event: UUIFileDropzoneEvent) {
		if (this.disabled) return;
		if (!event.detail.files.length && !event.detail.folders.length) return;

		const uploadable = this._manager.createMediaItems(event.detail, this.parentUnique);
		this.dispatchEvent(new UmbDropzoneSubmittedEvent(uploadable));
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

export default UmbDropzoneMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone-media': UmbDropzoneMediaElement;
	}
}
