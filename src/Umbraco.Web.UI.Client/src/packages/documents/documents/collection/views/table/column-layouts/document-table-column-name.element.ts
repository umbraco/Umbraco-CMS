import type { UmbDocumentCollectionItemModel } from '../../../types.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-table-column-name')
export class UmbDocumentTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@state()
	private _editDocumentPath = '';

	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbDocumentCollectionItemModel;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document')
			.onSetup(() => {
				return { data: { entityType: 'document', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});
	}

	#onClick(event: Event & { target: UUIButtonElement }) {
		event.preventDefault();
		event.stopPropagation();
		window.history.pushState({}, '', event.target.href);
	}

	render() {
		return html`
			<uui-button
				compact
				href="${this._editDocumentPath}edit/${this.value.unique}"
				label=${this.value.name}
				@click=${this.#onClick}></uui-button>
		`;
	}

	static styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

export default UmbDocumentTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-name': UmbDocumentTableColumnNameElement;
	}
}
