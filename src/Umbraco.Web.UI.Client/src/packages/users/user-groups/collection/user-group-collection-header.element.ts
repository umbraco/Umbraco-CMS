import { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
import { UUITextStyles, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-user-group-collection-header')
export class UmbUserGroupCollectionHeaderElement extends UmbLitElement {
	#collectionContext?: UmbUserGroupCollectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this.#collectionContext = instance as UmbUserGroupCollectionContext;
		});
	}

	#onCreate() {
		history.pushState(null, '', 'section/users/view/user-groups/user-group/create/');
	}

	#onSearch(event: UUIInputEvent) {
		//TODO How do we handle search when theres no endpoint (we have to do it locally)
	}

	render() {
		return html`
			<uui-button @click=${this.#onCreate} label="Create group" look="outline"></uui-button>
			<uui-input @input=${this.#onSearch} label="search" id="input-search"></uui-input>
		`;
	}
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				width: 100%;
				display: flex;
				justify-content: space-between;
				white-space: nowrap;
				gap: var(--uui-size-space-5);
				align-items: center;
			}

			#input-search {
				width: 100%;
			}
		`,
	];
}

export default UmbUserGroupCollectionHeaderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection-header': UmbUserGroupCollectionHeaderElement;
	}
}
