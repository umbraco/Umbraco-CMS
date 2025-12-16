import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-text-filter')
export class UmbDefaultCollectionTextFilterElement extends UmbLitElement {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	#onTextFilterChange(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const value = target.value || '';
		this.#debouncedSearch(value);
	}

	#debouncedSearch = debounce((value: string) => this.#collectionContext?.setFilter({ filter: value }), 500);

	protected override render() {
		return html`
			<uui-input
				@input=${this.#onTextFilterChange}
				label=${this.localize.term('placeholders_filter')}
				placeholder=${this.localize.term('placeholders_filter')}
				id="input-search"
				data-mark="collection-text-filter"></uui-input>
		`;
	}

	static override styles = [
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export { UmbDefaultCollectionTextFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-text-filter': UmbDefaultCollectionTextFilterElement;
	}
}
