import { UMB_USER_GROUP_COLLECTION_CONTEXT } from './user-group-collection.context-token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

import './user-group-collection-header.element.js';

@customElement('umb-user-group-collection')
export class UmbUserGroupCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: typeof UMB_USER_GROUP_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_USER_GROUP_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	#onSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const query = target.value || '';
		this.#debouncedSearch(query);
	}

	#debouncedSearch = debounce((query: string) => this.#collectionContext?.setFilter({ query }), 500);

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<uui-input
					@input=${this.#onSearch}
					label=${this.localize.term('general_search')}
					placeholder=${this.localize.term('general_search')}
					id="input-search"></uui-input>
			</umb-collection-toolbar>
		`;
	}

	static override styles = [
		css`
			uui-input {
				display: block;
			}
		`,
	];
}

export { UmbUserGroupCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection': UmbUserGroupCollectionElement;
	}
}
