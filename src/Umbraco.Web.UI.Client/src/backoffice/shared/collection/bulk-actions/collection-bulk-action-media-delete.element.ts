import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import type { UmbCollectionContext } from '../collection.context';
import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';
import { UmbLitElement } from 'src/core/element/lit-element.element';

@customElement('umb-collection-bulk-action-media-delete')
export class UmbCollectionBulkActionDeleteElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	// TODO: make a UmbCollectionContextMedia:
	private _collectionContext?: UmbCollectionContext<any, any>;

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
