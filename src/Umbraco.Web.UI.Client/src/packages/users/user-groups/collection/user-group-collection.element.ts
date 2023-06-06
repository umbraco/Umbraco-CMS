import { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './user-group-collection-view.element.js';
import './user-group-collection-header.element.js';

@customElement('umb-user-group-collection')
export class UmbUserCollectionElement extends UmbLitElement {
	#collectionContext = new UmbUserGroupCollectionContext(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this.#collectionContext);
	}

	render() {
		return html`
			<umb-body-layout header-clear header-shadow>
				<umb-user-group-collection-header slot="header"></umb-user-group-collection-header>
				<umb-user-group-collection-view></umb-user-group-collection-view>
				<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>
			</umb-body-layout>
		`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbUserCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection': UmbUserCollectionElement;
	}
}
