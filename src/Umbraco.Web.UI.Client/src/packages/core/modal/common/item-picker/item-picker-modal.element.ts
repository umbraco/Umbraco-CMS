import { css, html, customElement, repeat, nothing, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbItemPickerModalData, UmbItemPickerModel } from '@umbraco-cms/backoffice/modal';

@customElement('umb-item-picker-modal')
export class UmbItemPickerModalElement extends UmbModalBaseElement<UmbItemPickerModalData, UmbItemPickerModel> {
	#close() {
		this.modalContext?.reject();
	}

	#submit(item: UmbItemPickerModel) {
		this.modalContext?.setValue(item);
		this.modalContext?.submit();
	}

	render() {
		if (!this.data) return nothing;
		const items = this.data.items;
		return html`
			<umb-body-layout headline=${this.data.headline}>
				<div>
					${when(
						items.length,
						() => html`
							<uui-box>
								${repeat(
									items,
									(item) => item.value,
									(item) => html`
										<uui-button @click=${() => this.#submit(item)} look="placeholder" label="${item.label}">
											<h4>${item.label}</h4>
											<p>${item.description}</p>
										</uui-button>
									`,
								)}
							</uui-box>
						`,
						() => html`<p>There are no items to select.</p>`,
					)}
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} label="${this.localize.term('general_close')}"></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box > uui-button {
				display: block;
				--uui-button-content-align: flex-start;
			}

			uui-box > uui-button:not(:last-of-type) {
				margin-bottom: var(--uui-size-space-5);
			}

			h4 {
				text-align: left;
				margin: 0.5rem 0;
			}

			p {
				text-align: left;
				margin: 0 0 0.5rem 0;
			}
		`,
	];
}

export default UmbItemPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-item-picker-modal': UmbItemPickerModalElement;
	}
}
