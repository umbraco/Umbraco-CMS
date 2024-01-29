import { UmbDocumentPickerContext } from '../input-document/input-document.context.js';
import type { UmbDocumentItemModel } from '../../repository/index.js';
import { html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-document-picker-root')
export class UmbInputDocumentPickerRootElement extends FormControlMixin(UmbLitElement) {
	public get unique(): string | null | undefined {
		return this.#documentPickerContext.getSelection()[0];
	}
	public set unique(unique: string | null | undefined) {
		const selection = unique ? [unique] : [];
		this.#documentPickerContext.setSelection(selection);
	}

	@property()
	public set value(unique: string) {
		this.unique = unique;
	}

	@state()
	private _items?: Array<UmbDocumentItemModel>;

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
							(item) => item.unique,
							(item) => this._renderItem(item),
						)}
				  </uui-ref-list>`
				: ''}
			${this.#renderButtons()}
		`;
	}

	#renderButtons() {
		if (this.unique) return;

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

	private _renderItem(item: UmbDocumentItemModel) {
		if (!item.unique) return;
		// TODO: get correct variant name
		const name = item.variants[0]?.name;
		return html`
			<uui-ref-node name=${name} detail=${ifDefined(item.unique)}>
				<!-- TODO: implement is trashed <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> -->
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#documentPickerContext.openPicker()} label="Edit document ${name}"
						>Edit</uui-button
					>
					<uui-button
						@click=${() => this.#documentPickerContext.requestRemoveItem(item.unique)}
						label="Remove document ${name}"
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
