import type { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
import { UMB_USER_GROUP_COLLECTION_CONTEXT } from './user-group-collection.context-token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { debounce } from '@umbraco-cms/backoffice/utils';

const elementName = 'umb-user-group-collection-header';

/** @deprecated This component is no longer used in core; to be removed in Umbraco 17. */
@customElement(elementName)
export class UmbUserGroupCollectionHeaderElement extends UmbLitElement {
	#collectionContext: UmbUserGroupCollectionContext | undefined;

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

	#debouncedSearch = debounce((query: any) => this.#collectionContext?.setFilter({ query }), 500);

	override render() {
		return html`<umb-collection-action-bundle></umb-collection-action-bundle>
			<uui-input
				@input=${this.#onSearch}
				label=${this.localize.term('general_search')}
				placeholder=${this.localize.term('general_search')}
				id="input-search"></uui-input>
			<umb-collection-view-bundle></umb-collection-view-bundle>`;
	}

	static override styles = [
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

			uui-input {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserGroupCollectionHeaderElement;
	}
}
