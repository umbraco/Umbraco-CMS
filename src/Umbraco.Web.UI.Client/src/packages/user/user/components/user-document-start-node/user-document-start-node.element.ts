import { html, customElement, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import { UmbDocumentItemRepository } from '@umbraco-cms/backoffice/document';

@customElement('umb-user-document-start-node')
export class UmbUserDocumentStartNodeElement extends UmbLitElement {
	@property({ type: Array, attribute: false })
	uniques: Array<string> = [];

	@state()
	_displayValue: Array<UmbDocumentItemModel> = [];

	#itemRepository = new UmbDocumentItemRepository(this);

	protected async firstUpdated(): Promise<void> {
		if (this.uniques.length === 0) return;
		const { data } = await this.#itemRepository.requestItems(this.uniques);
		this._displayValue = data || [];
	}

	render() {
		if (this.uniques.length < 1) {
			return html`
				<uui-ref-node name="Content Root">
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
					<uui-ref-node name=${item.variants[0]?.name}>
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
