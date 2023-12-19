import { UmbDocumentPickerContext } from '../input-document/input-document.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-document-source')
export class UmbInputDocumentSourceElement extends FormControlMixin(UmbLitElement) {
	public get nodeId(): string | null | undefined {
		return this.#pickerContext.getSelection()[0];
	}
	public set nodeId(id: string | null | undefined) {
		const selection = id ? [id] : [];
		this.#pickerContext.setSelection(selection);
	}

	@property()
	public set value(id: string) {
		this.nodeId = id;
	}

	@state()
	private _items?: Array<DocumentItemResponseModel>;

	#pickerContext = new UmbDocumentPickerContext(this);

	constructor() {
		super();

		this.#pickerContext.max = 1;

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	// TODO: Wire up the DynamicRoot picker feature. [LK]
	private _openDynamicRootPicker(e: Event) {
		console.log('openDynamicRootPicker', e);
		const btn = e.target as UUIButtonElement;
		btn.color = 'warning';
		btn.label = 'TODO!';
		btn.look = 'primary';
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
				@click=${() => this.#pickerContext.openPicker()}
				label=${this.localize.term('contentPicker_defineRootNode')}></uui-button>
			<uui-button
				look="placeholder"
				@click=${this._openDynamicRootPicker}
				label=${this.localize.term('contentPicker_defineDynamicRoot')}></uui-button>
		</uui-button-group>`;
	}

	private _renderItem(item: DocumentItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.id)}>
				<!-- TODO: implement is trashed <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> -->
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#pickerContext.openPicker()} label="Edit document ${item.name}"
						>Edit</uui-button
					>
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.id!)}
						label="Remove document ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static styles = [css``];
}

export default UmbInputDocumentSourceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-source': UmbInputDocumentSourceElement;
	}
}
