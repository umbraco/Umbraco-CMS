import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/** @deprecated This component is no longer used in core; to be removed in Umbraco 17. */
@customElement('umb-media-collection-toolbar')
export class UmbMediaCollectionToolbarElement extends UmbLitElement {
	#collectionContext?: UmbMediaCollectionContext;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (instance) => {
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

export default UmbMediaCollectionToolbarElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection-toolbar': UmbMediaCollectionToolbarElement;
	}
}
