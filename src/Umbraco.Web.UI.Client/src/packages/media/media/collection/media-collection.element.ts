import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UMB_DEFAULT_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import './media-collection-toolbar.element.js';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#mediaCollection?: UmbMediaCollectionContext;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#mediaCollection = instance as UmbMediaCollectionContext;
		});
	}

	protected renderToolbar() {
		return html` <umb-media-collection-toolbar slot="header"></umb-media-collection-toolbar>
			<umb-dropzone-media @change=${() => this.#mediaCollection?.requestCollection()}></umb-dropzone-media>`;
	}
}

export default UmbMediaCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection': UmbMediaCollectionElement;
	}
}
