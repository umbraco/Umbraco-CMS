import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import type { UmbDropzoneElement } from '../dropzone/index.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import { customElement, html, ref, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbDropzoneSubmittedEvent } from '@umbraco-cms/backoffice/dropzone';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: typeof UMB_MEDIA_COLLECTION_CONTEXT.TYPE;

	@state()
	private _progress = -1;

	@state()
	private _unique: string | null = null;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
		});

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.unique, (unique) => {
				this._unique = unique ?? null;
			});
		});
	}

	#observeProgressItems(dropzone?: Element) {
		if (!dropzone) return;
		this.observe(
			(dropzone as UmbDropzoneElement).progressItems(),
			(progressItems) => {
				progressItems.forEach((item) => {
					// We do not update folders as it may have children still being uploaded.
					if (item.folder?.name) return;

					this.#collectionContext?.updatePlaceholderStatus(item.unique, item.status);
					this.#collectionContext?.updatePlaceholderProgress(item.unique, item.progress);
				});
			},
			'_observeProgressItems',
		);
	}

	async #setupPlaceholders(event: UmbDropzoneSubmittedEvent) {
		event.preventDefault();
		const placeholders = event.items
			.filter((p) => p.parentUnique === this._unique)
			.map((p) => ({ unique: p.unique, status: p.status, name: p.temporaryFile?.file.name ?? p.folder?.name }));

		this.#collectionContext?.setPlaceholders(placeholders);
	}

	async #onComplete(event: Event) {
		event.preventDefault();
		this._progress = -1;
		this.#collectionContext?.requestCollection();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const reloadEvent = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: this._unique ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
			unique: this._unique,
		});
		eventContext.dispatchEvent(reloadEvent);
	}

	#onProgress(event: ProgressEvent) {
		event.preventDefault();
		this._progress = (event.loaded / event.total) * 100;
		if (this._progress >= 100) {
			this._progress = -1;
		}
	}

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone
				id="dropzone"
				${ref(this.#observeProgressItems)}
				multiple
				.parentUnique=${this._unique}
				@submitted=${this.#setupPlaceholders}
				@complete=${this.#onComplete}
				@progress=${this.#onProgress}></umb-dropzone>
		`;
	}
}

/** @deprecated Should be exported as `element` only; to be removed in Umbraco 17. */
export default UmbMediaCollectionElement;

export { UmbMediaCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection': UmbMediaCollectionElement;
	}
}
