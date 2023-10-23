import { css, html, customElement, property, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbDocumentRepository } from '@umbraco-cms/backoffice/document';
import { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-document-start-node')
export class UmbUserDocumentStartNodeElement extends UmbLitElement {
	@property({ type: Array, attribute: false })
	ids: Array<string> = [];

	@state()
	_displayValue: Array<DocumentItemResponseModel> = [];

	#itemRepository = new UmbDocumentRepository(this);

	protected async firstUpdated(): Promise<void> {
		if (this.ids.length === 0) return;
		const { data } = await this.#itemRepository.requestItems(this.ids);
		this._displayValue = data || [];
	}

	render() {
		if (this.ids.length < 1)
			return html`
				<uui-ref-node name="Content Root">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			`;

		return repeat(
			this._displayValue,
			(item) => item.id,
			(item) => {
				return html`
					<uui-ref-node name=${ifDefined(item.name)}>
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserDocumentStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-document-start-node': UmbUserDocumentStartNodeElement;
	}
}
