import { html } from 'lit';
import { property } from 'lit/decorators.js';
import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import {
	UmbModalContext,
	UmbModalToken,
	UmbModalType,
	UMB_MODAL_CONTEXT_TOKEN,
	UmbPickerModalData,
} from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';

/** TODO: Make use of UUI FORM Mixin, to make it easily take part of a form. */
export class UmbInputListBase extends UmbLitElement {
	@property({ type: Array })
	public value: Array<string> = [];

	@property({ type: Boolean })
	public multiple = true;

	@property({ type: String })
	public modalType: UmbModalType = 'sidebar';

	@property({ type: String })
	public modalSize: UUIModalSidebarSize = 'small';

	// TODO: not great that we use any, any here. Investigate if we can have some interface or base modal token for this type.
	protected pickerToken?: UmbModalToken<any, any>;
	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _openPicker() {
		if (!this.pickerToken) return;

		const modalHandler = this._modalContext?.open(this.pickerToken, {
			multiple: this.multiple,
			selection: this.value,
		});

		modalHandler?.onSubmit().then((data: UmbPickerModalData<string>) => {
			if (data) {
				this.value = data.selection;
				this.selectionUpdated();
			}
		});
	}

	protected removeFromSelection(key: string) {
		this.value = this.value.filter((k) => k !== key);
		this.selectionUpdated();
	}

	protected selectionUpdated() {
		// override this method to react to selection changes
	}

	protected renderButton() {
		return html`<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">
			Add
		</uui-button>`;
	}
	protected renderContent() {
		return html``;
	}

	render() {
		return html`${this.renderContent()}${this.renderButton()}`;
	}
}
