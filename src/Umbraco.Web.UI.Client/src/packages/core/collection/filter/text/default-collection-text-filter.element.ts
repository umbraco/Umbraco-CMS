import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Default element for the collection text filter extension.
 *
 * Renders a text input that allows users to filter collection items.
 * Works in conjunction with {@link UmbDefaultCollectionTextFilterApi} to apply the filter.
 * @element umb-default-collection-text-filter
 */
@customElement('umb-default-collection-text-filter')
export class UmbDefaultCollectionTextFilterElement extends UmbLitElement {
	@state()
	private _text: string | undefined = undefined;

	#api?: UmbCollectionTextFilterApi;

	/**
	 * The API instance that handles the filter logic.
	 * This is set by the extension system when the element is created.
	 * @returns {UmbCollectionTextFilterApi | undefined} The collection text filter API instance.
	 */
	public get api(): UmbCollectionTextFilterApi | undefined {
		return this.#api;
	}
	/**
	 * Sets the API instance that handles the filter logic.
	 * @param {UmbCollectionTextFilterApi | undefined} value - The collection text filter API instance.
	 */
	public set api(value: UmbCollectionTextFilterApi | undefined) {
		this.#api = value;
		this.observe(this.#api?.text, (value) => (this._text = value), 'umbApiTextObserver');
	}

	#onTextFilterChange(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const value = target.value || '';
		this.#api?.setText(value);
	}

	protected override render() {
		return html`
			<uui-input
				value=${this._text || ''}
				@input=${this.#onTextFilterChange}
				label=${this.localize.term('placeholders_filter')}
				placeholder=${this.localize.term('placeholders_filter')}
				data-mark="collection-text-filter"></uui-input>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex: 1 1 auto;
			}

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
