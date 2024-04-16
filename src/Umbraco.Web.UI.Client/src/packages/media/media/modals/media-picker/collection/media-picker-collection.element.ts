import type { UmbMediaPickerCollectionContext } from './media-picker-collection.context.js';
import { customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UMB_DEFAULT_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import type { UmbProgressEvent } from '@umbraco-cms/backoffice/event';

import './media-picker-collection-toolbar.element.js';

@customElement('umb-media-picker-collection')
export class UmbMediaPickerCollectionElement extends UmbCollectionDefaultElement {
	#mediaCollection?: UmbMediaPickerCollectionContext;

	@state()
	private _progress = -1;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#mediaCollection = instance as UmbMediaPickerCollectionContext;
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
			<umb-media-picker-collection-toolbar slot="header"></umb-media-picker-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone-media @change=${this.#onChange} @progress=${this.#onProgress}></umb-dropzone-media>
		`;
	}
}

export default UmbMediaPickerCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-collection': UmbMediaPickerCollectionElement;
	}
}
