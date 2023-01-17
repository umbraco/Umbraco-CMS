import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_ALIAS } from '../collection.context';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-collection-view-media-table')
export class UmbCollectionViewMediaTableElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	private _mediaItems?: Array<MediaDetails>;

	@state()
	private _selection?: Array<string>;

	private _collectionContext?: UmbCollectionContext<MediaDetails>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_ALIAS, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.data, (nodes) => {
			this._mediaItems = nodes;
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	render() {
		return html`<h1>umb-collection-view-media-table</h1>
			<div>
				<h3>Selected Media Items:</h3>
				<ul>
					${this._selection?.map((key) => {
						const mediaItem = this._mediaItems?.find((item) => item.key === key);
						return html`<li>${mediaItem?.name}</li>`;
					})}
				</ul>
			</div> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media-table': UmbCollectionViewMediaTableElement;
	}
}
