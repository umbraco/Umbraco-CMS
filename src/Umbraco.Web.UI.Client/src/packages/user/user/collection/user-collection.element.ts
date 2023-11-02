import { UmbUserCollectionContext } from './user-collection.context.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionElement } from '@umbraco-cms/backoffice/collection';

import './user-collection-header.element.js';

@customElement('umb-user-collection')
export class UmbUserCollectionElement extends UmbCollectionElement {
	constructor() {
		super();
		new UmbUserCollectionContext(this);
	}

	protected renderToolbar() {
		return html`<umb-user-collection-header slot="header"></umb-user-collection-header> `;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection': UmbUserCollectionElement;
	}
}
