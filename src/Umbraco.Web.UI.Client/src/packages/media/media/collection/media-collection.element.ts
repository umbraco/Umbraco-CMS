import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UMB_DEFAULT_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import type { UmbProgressEvent } from '@umbraco-cms/backoffice/event';

import './media-collection-toolbar.element.js';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#mediaCollection?: UmbMediaCollectionContext;

	@state()
	private _progress = -1;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#mediaCollection = instance as UmbMediaCollectionContext;
		});
	}

	#onChange() {
		this._progress = -1;
		this.#mediaCollection?.requestCollection();
	}

	#onProgress(event: UmbProgressEvent) {
		this._progress = event.progress;
	}

	protected renderToolbar() {
		return html`
			<umb-media-collection-toolbar slot="header"></umb-media-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone @change=${this.#onChange} @progress=${this.#onProgress}></umb-dropzone>
		`;
	}
}

export default UmbMediaCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection': UmbMediaCollectionElement;
	}
}
