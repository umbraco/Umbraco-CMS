import { UmbDocumentPickerContext } from '../input-document/input-document.context.js';
import { UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL } from '@umbraco-cms/backoffice/dynamic-root';
import { html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';

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

	private _modalContext?: UmbModalManagerContext;

	#openModal?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.#documentPickerContext.max = 1;

		this.observe(this.#documentPickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#documentPickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	#openDocumentPicker() {
		this.#documentPickerContext.openPicker({
			hideTreeRoot: true,
		});
	}

	#openDynamicRootPicker() {
		this.#openModal = this._modalContext?.open(UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL, {
			data: { testing: 123 },
		});
		this.#openModal?.onSubmit().then((data) => {
			console.log('openDynamicRootPicker.onSubmit', data);
		});
	}

	render() {
		return html`${this.#renderButtons()} ${this.#renderItems()}`;
	}

	#renderButtons() {
		if (this.nodeId) return;
		return html`<uui-button-group>
			<uui-button
				look="placeholder"
				@click=${this.#openDocumentPicker}
				label=${this.localize.term('contentPicker_defineRootNode')}></uui-button>
			<uui-button
				look="placeholder"
				@click=${this.#openDynamicRootPicker}
				label=${this.localize.term('contentPicker_defineDynamicRoot')}></uui-button>
		</uui-button-group>`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`<uui-ref-list>
			${repeat(
				this._items,
				(item) => item.id,
				(item) => this.#renderItem(item),
			)}
		</uui-ref-list>`;
	}

	#renderItem(item: DocumentItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.id)}>
				${this.#renderIcon(item)}
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					<uui-button @click=${this.#openDocumentPicker} label="Edit document ${item.name}">
						${this.localize.term('general_edit')}
					</uui-button>
					<uui-button
						@click=${() => this.#documentPickerContext.requestRemoveItem(item.id!)}
						label="Remove document ${item.name}">
						${this.localize.term('general_remove')}
					</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIcon(item: DocumentItemResponseModel) {
		if (!item.icon) return;
		return html`<uui-icon slot="icon" name=${item.icon}></uui-icon>`;
	}

	#renderIsTrashed(item: DocumentItemResponseModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}
}

export default UmbInputDocumentPickerRootElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-picker-root': UmbInputDocumentPickerRootElement;
	}
}
