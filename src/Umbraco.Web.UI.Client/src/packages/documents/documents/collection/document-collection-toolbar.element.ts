import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

/** @deprecated This component is no longer used in core; to be removed in Umbraco 17. */
@customElement('umb-document-collection-toolbar')
export class UmbDocumentCollectionToolbarElement extends UmbLitElement {
	#collectionContext?: UmbDefaultCollectionContext;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	#updateSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
	}

	override render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			<uui-input @input=${this.#updateSearch} label="Search" placeholder="Search..." id="input-search"></uui-input>
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
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
