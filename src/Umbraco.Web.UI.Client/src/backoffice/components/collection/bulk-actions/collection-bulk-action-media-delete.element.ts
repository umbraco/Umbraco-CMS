import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import type { UmbCollectionContext } from '../collection.context';
import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbMediaStore, UmbMediaStoreItemType } from '@umbraco-cms/stores/media/media.store';

@customElement('umb-collection-bulk-action-media-delete')
export class UmbCollectionBulkActionDeleteElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	// TODO: make a UmbCollectionContextMedia:
	private _collectionContext?: UmbCollectionContext<UmbMediaStoreItemType, UmbMediaStore>;

	public manifest?: ManifestCollectionBulkAction;

	constructor() {
		super();

		this.consumeContext('umbCollectionContext', (context) => {
			this._collectionContext = context;
		});
	}

	render() {
		// TODO: make a UmbCollectionContextMedia and use a deleteSelection method.
		return html`<uui-button
			@click=${() => this._collectionContext?.clearSelection()}
			label=${ifDefined(this.manifest?.meta.label)}
			color="default"
			look="secondary"></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-bulk-action-media-delete': UmbCollectionBulkActionDeleteElement;
	}
}
