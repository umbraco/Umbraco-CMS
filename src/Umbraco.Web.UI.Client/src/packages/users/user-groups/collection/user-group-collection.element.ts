import { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './user-group-collection-view.element.js';
import './components/user-group-collection-header.element.js';

@customElement('umb-user-group-collection')
export class UmbUserCollectionElement extends UmbLitElement {
	#collectionContext = new UmbUserGroupCollectionContext(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this.#collectionContext);
	}

	render() {
		return html`
			<umb-body-layout header-transparent>
				<umb-user-group-collection-header slot="header"></umb-user-group-collection-header>
				<umb-user-group-collection-view></umb-user-group-collection-view>
				<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection': UmbUserCollectionElement;
	}
}
