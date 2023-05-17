import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbUserGroupCollectionContext } from './user-group-collection.context';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './user-group-collection-view.element';
import './user-group-collection-header.element';

@customElement('umb-user-group-collection')
export class UmbUserCollectionElement extends UmbLitElement {
	#collectionContext = new UmbUserGroupCollectionContext(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this.#collectionContext);
	}

	render() {
		return html`
			<uui-scroll-container>
				<umb-user-group-collection-header></umb-user-group-collection-header>
				<umb-user-group-collection-view></umb-user-group-collection-view>
			</uui-scroll-container>
			<umb-collection-selection-actions></umb-collection-selection-actions>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbUserCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection': UmbUserCollectionElement;
	}
}
