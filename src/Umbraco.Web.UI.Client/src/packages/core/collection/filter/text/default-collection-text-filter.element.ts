import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-text-filter')
export class UmbDefaultCollectionTextFilterElement extends UmbLitElement {
	#api?: UmbCollectionTextFilterApi;

	public get api(): UmbCollectionTextFilterApi | undefined {
		return this.#api;
	}
	public set api(value: UmbCollectionTextFilterApi | undefined) {
		this.#api = value;
	}

	#onTextFilterChange(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const value = target.value || '';
		this.#api?.updateTextFilter(value);
	}

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
