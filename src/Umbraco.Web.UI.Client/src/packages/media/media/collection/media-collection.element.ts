import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import { UmbFileDropzoneItemStatus, type UmbUploadableItem } from '../dropzone/types.js';
import type { UmbDropzoneElement } from '../dropzone/dropzone.element.js';
import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import { customElement, html, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import './media-collection-toolbar.element.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#mediaCollection?: UmbMediaCollectionContext;

	@state()
	private _progress = -1;

	@state()
	private _unique: string | null = null;

	@query('#dropzone')
	private _dropzone!: UmbDropzoneElement;

	constructor() {
		super();
		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (instance) => {
			this.#mediaCollection = instance;
		});
		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.unique, (unique) => {
				this._unique = unique ?? null;
			});
		});
	}

	#observeProgressItems() {
		this.observe(
			this._dropzone.progressItems(),
			(progressItems) => {
				progressItems.forEach((item) => {
					if (item.status === UmbFileDropzoneItemStatus.COMPLETE && !item.folder?.name) {
						// We do not update folders as it may have children still being uploaded.
						this.#mediaCollection?.updatePlaceholderStatus(item.unique, UmbFileDropzoneItemStatus.COMPLETE);
					}
				});
			},
			'_observeProgressItems',
		);
	}

	async #setupPlaceholders(event: CustomEvent) {
		event.preventDefault();
		const uploadable = event.detail as Array<UmbUploadableItem>;
		const placeholders = uploadable
			.filter((p) => p.parentUnique === this._unique)
			.map((p) => ({ unique: p.unique, status: p.status, name: p.temporaryFile?.file.name ?? p.folder?.name }));

		this.#mediaCollection?.setPlaceholders(placeholders);
		this.#observeProgressItems();
	}

	async #onComplete() {
		this._progress = -1;
		this.#mediaCollection?.requestCollection();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: this._unique ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
			unique: this._unique,
		});
		eventContext.dispatchEvent(event);
	}

	#onProgress(event: ProgressEvent) {
		this._progress = (event.loaded / event.total) * 100;
		if (this._progress >= 100) {
			this._progress = -1;
		}
	}

	protected override renderToolbar() {
		return html`
			<umb-media-collection-toolbar slot="header"></umb-media-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone
				id="dropzone"
				multiple
				.parentUnique=${this._unique}
				@submitted=${this.#setupPlaceholders}
				@complete=${this.#onComplete}
				@progress=${this.#onProgress}></umb-dropzone>
		`;
	}
}

export default UmbMediaCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection': UmbMediaCollectionElement;
	}
}
