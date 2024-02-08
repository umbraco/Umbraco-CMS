import type {
	UmbListitemPickerModalData,
	UmbListitemPickerModalValue,
} from '../../token/listitem-picker-modal.token.js';
import { UmbModalBaseElement } from '../../modal-element.element.js';
import { html, customElement, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-listitem-picker-modal')
export class UmbListitemPickerModalElement extends UmbModalBaseElement<
	UmbListitemPickerModalData,
	UmbListitemPickerModalValue
> {
	private _handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		const form = e.target as HTMLFormElement;
		const formData = new FormData(form);
		const selectedItems = formData.getAll('listitem').map((item) => item.toString());
		this.modalContext?.setValue({ selection: selectedItems });
		this.modalContext?.submit();
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	#renderItems() {
		return html` <uui-form-layout-item>
			${repeat(
				this.data?.items || [],
				(item) => item.key,
				(item) => {
					return html`<uui-checkbox
						value=${item.key}
						label=${item.name}
						name="listitem"
						.checked=${item.selected || false}></uui-checkbox>`;
				},
			)}
		</uui-form-layout-item>`;
	}

	render() {
		return html`
			<uui-dialog-layout class="uui-text" headline=${ifDefined(this.data?.headline)}>
				<uui-form>
					<form name="listitem-picker" @submit=${this._handleSubmit}>
						${this.#renderItems()}
						<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}"></uui-button>
						<uui-button
							type="submit"
							slot="actions"
							id="confirm"
							look="primary"
							label="${this.data?.confirmLabel || 'Confirm'}"></uui-button>
					</form>
				</uui-form>
			</uui-dialog-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbListitemPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-listitem-picker-modal': UmbListitemPickerModalElement;
	}
}
