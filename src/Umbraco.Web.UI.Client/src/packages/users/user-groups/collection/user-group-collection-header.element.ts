import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
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
			<div id="sticky-top">
				<div id="collection-top-bar">
					<uui-button @click=${this.#onCreate} label="Create group" look="outline"></uui-button>
					<uui-input @input=${this.#onSearch} label="search" id="input-search"></uui-input>
				</div>
			</div>
		`;
	}
	static styles = [
		UUITextStyles,
		css`
			#sticky-top {
				position: sticky;
				top: 0px;
				z-index: 1;
				box-shadow: 0 1px 3px rgba(0, 0, 0, 0), 0 1px 2px rgba(0, 0, 0, 0);
				transition: 250ms box-shadow ease-in-out;
			}

			#sticky-top.header-shadow {
				box-shadow: var(--uui-shadow-depth-2);
			}

			#collection-top-bar {
				padding: var(--uui-size-space-4) var(--uui-size-layout-1);
				background-color: var(--uui-color-background);
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
