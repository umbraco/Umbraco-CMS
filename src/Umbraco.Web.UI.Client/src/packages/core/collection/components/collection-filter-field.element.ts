import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-filter-field')
export class UmbCollectionFilterFieldElement extends UmbLitElement {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
		});
	}

	#debouncedFilter = debounce((filter: string) => this.#collectionContext?.setFilter({ filter }), 500);

	#onInput(event: InputEvent & { target: HTMLInputElement }) {
		const filter = event.target.value ?? '';
		this.#debouncedFilter(filter);
	}

	override render() {
		return html`
			<uui-input
				label=${this.localize.term('general_search')}
				placeholder=${this.localize.term('placeholders_search')}
				@input=${this.#onInput}></uui-input>
		`;
	}

	static override readonly styles = [
		css`
			uui-input {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-filter-field': UmbCollectionFilterFieldElement;
	}
}
