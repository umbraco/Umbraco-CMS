import type { UmbDocumentCollectionContext } from './document-collection.context.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';

@customElement('umb-document-collection-toolbar')
export class UmbDocumentCollectionToolbarElement extends UmbLitElement {
	#collectionContext?: UmbDocumentCollectionContext;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance as UmbDocumentCollectionContext;
		});
	}

	#updateSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
	}

	render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			${this.#renderSearch()}
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
	}

	#renderSearch() {
		return html`
			<uui-input @input=${this.#updateSearch} label="Search" placeholder="Search..." id="input-search"></uui-input>
		`;
	}

	static styles = [
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

export default UmbDocumentCollectionToolbarElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-collection-toolbar': UmbDocumentCollectionToolbarElement;
	}
}
