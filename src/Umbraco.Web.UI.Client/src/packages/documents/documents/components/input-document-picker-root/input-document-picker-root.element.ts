import { UmbDocumentPickerContext } from '../input-document/input-document.context.js';
import { html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-document-picker-root')
export class UmbInputDocumentPickerRootElement extends FormControlMixin(UmbLitElement) {
	public get nodeId(): string | null | undefined {
		return this.#documentPickerContext.getSelection()[0];
	}
	public set nodeId(id: string | null | undefined) {
		const selection = id ? [id] : [];
		this.#documentPickerContext.setSelection(selection);
	}

	@property()
	public set value(id: string) {
		this.nodeId = id;
	}

	@state()
	private _items?: Array<DocumentItemResponseModel>;

	#documentPickerContext = new UmbDocumentPickerContext(this);

	// TODO: DynamicRoot - once feature implemented, wire up context and picker UI. [LK]
	#dynamicRootPickerContext = {
		openPicker: () => {
			throw new Error('DynamicRoot picker has not been implemented yet.');
		},
	};

	constructor() {
		super();

		this.#documentPickerContext.max = 1;

		this.observe(this.#documentPickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#documentPickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`
			${this._items
				? html` <uui-ref-list
						>${repeat(
							this._items,
							(item) => item.id,
							(item) => this._renderItem(item),
						)}
				  </uui-ref-list>`
				: ''}
			${this.#renderButtons()}
		`;
	}

	#renderButtons() {
		if (this.nodeId) return;

		//TODO: Dynamic paths
		return html` <uui-button-group>
			<uui-button
				look="placeholder"
				@click=${() => this.#documentPickerContext.openPicker()}
				label=${this.localize.term('contentPicker_defineRootNode')}></uui-button>
			<uui-button
				look="placeholder"
				@click=${() => this.#dynamicRootPickerContext.openPicker()}
				label=${this.localize.term('contentPicker_defineDynamicRoot')}></uui-button>
		</uui-button-group>`;
	}

	private _renderItem(item: DocumentItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.id)}>
				<!-- TODO: implement is trashed <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> -->
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#documentPickerContext.openPicker()} label="Edit document ${item.name}"
						>Edit</uui-button
					>
					<uui-button
						@click=${() => this.#documentPickerContext.requestRemoveItem(item.id!)}
						label="Remove document ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}
}

export default UmbInputDocumentPickerRootElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-picker-root': UmbInputDocumentPickerRootElement;
	}
}
