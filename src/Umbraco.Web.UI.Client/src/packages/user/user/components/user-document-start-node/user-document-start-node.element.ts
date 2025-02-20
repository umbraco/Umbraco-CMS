import { html, customElement, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import { UmbDocumentItemRepository } from '@umbraco-cms/backoffice/document';

@customElement('umb-user-document-start-node')
export class UmbUserDocumentStartNodeElement extends UmbLitElement {
	#uniques: Array<string> = [];
	@property({ type: Array, attribute: false })
	public get uniques(): Array<string> {
		return this.#uniques;
	}
	public set uniques(value: Array<string>) {
		this.#uniques = value;

		if (this.#uniques.length > 0) {
			this.#observeItems();
		}
	}

	@property({ type: Boolean })
	readonly = false;

	@state()
	_displayValue: Array<UmbDocumentItemModel> = [];

	#itemRepository = new UmbDocumentItemRepository(this);

	async #observeItems() {
		const { asObservable } = await this.#itemRepository.requestItems(this.#uniques);

		this.observe(asObservable(), (data) => {
			this._displayValue = data || [];
		});
	}

	override render() {
		if (this.uniques.length < 1) {
			return html`
				<uui-ref-node
					name="Content Root"
					?disabled=${this.readonly}
					style="--uui-color-disabled-contrast: var(--uui-color-text)">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			`;
		}

		return repeat(
			this._displayValue,
			(item) => item.unique,
			(item) => {
				return html`
					<!-- TODO: get correct variant name -->
					<uui-ref-node
						name=${item.variants[0]?.name}
						?disabled=${this.readonly}
						style="--uui-color-disabled-contrast: var(--uui-color-text)">
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}
}

export default UmbUserDocumentStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-document-start-node': UmbUserDocumentStartNodeElement;
	}
}
