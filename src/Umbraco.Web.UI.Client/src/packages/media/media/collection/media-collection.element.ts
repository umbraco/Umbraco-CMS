import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import { customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import type { UmbProgressEvent } from '@umbraco-cms/backoffice/event';

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

	async #onChange() {
		this._progress = -1;
		this.#mediaCollection?.requestCollection();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: this._unique ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
			unique: this._unique,
		});
		eventContext.dispatchEvent(event);
	}

	#onProgress(event: UmbProgressEvent) {
		this._progress = event.progress;
	}

	protected override renderToolbar() {
		return html`
			<umb-media-collection-toolbar slot="header"></umb-media-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone
				.parentUnique=${this._unique}
				@change=${this.#onChange}
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
