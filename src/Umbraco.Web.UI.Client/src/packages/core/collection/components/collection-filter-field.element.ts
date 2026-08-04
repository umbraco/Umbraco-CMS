import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-filter-field')
export class UmbCollectionFilterFieldElement extends UmbLitElement {
	@state()
	private _value = '';

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;
	#hasUserInput = false;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;

			// Sync filter if the value is updated directly in the context, ex. when the filter is restored from memory.
			this.observe(
				context?.filter,
				(filter) => {
					if (this.#hasUserInput) return;
					this._value = (filter as { filter?: string } | undefined)?.filter ?? '';
				},
				'umbCollectionFilterFieldValueObserver',
			);
		});
	}

	#debouncedFilter = debounce((filter: string) => this.#collectionContext?.setFilter({ filter }), 500);

	#onInput(event: InputEvent & { target: HTMLInputElement }) {
		this.#hasUserInput = true;
		const filter = event.target.value ?? '';
		this.#debouncedFilter(filter);
	}

	override render() {
		return html`
			<uui-input
				label=${this.localize.term('general_filter')}
				placeholder=${this.localize.term('placeholders_filter')}
				data-mark="input:filter"
				.value=${this._value}
				@input=${this.#onInput}></uui-input>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-filter-field': UmbCollectionFilterFieldElement;
	}
}
