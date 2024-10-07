import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import {
	UMB_COLLECTION_CONTEXT,
	UmbCollectionDefaultElement,
	type UmbDefaultCollectionContext,
} from '@umbraco-cms/backoffice/collection';

@customElement('umb-dictionary-collection')
export class UmbDictionaryCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: UmbDefaultCollectionContext;
	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
		});
	}

	#updateSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
	}

	protected override renderToolbar() {
		return html`<umb-collection-toolbar slot="header">${this.#renderSearch()}</umb-collection-toolbar>`;
	}

	#renderSearch() {
		return html`<uui-input
			id="input-search"
			@input=${this.#updateSearch}
			placeholder=${this.localize.term('placeholders_search')}></uui-input>`;
	}

	static override styles = [
		css`
			#input-search {
				width: 100%;
			}
		`,
	];
}

export default UmbDictionaryCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-collection': UmbDictionaryCollectionElement;
	}
}
