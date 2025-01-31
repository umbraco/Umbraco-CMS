import {
	css,
	html,
	customElement,
	repeat,
	nothing,
	when,
	state,
	ifDefined,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbItemPickerModalData, UmbItemPickerModel } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-item-picker-modal')
export class UmbItemPickerModalElement extends UmbModalBaseElement<UmbItemPickerModalData, UmbItemPickerModel> {
	@state()
	private _filtered: Array<UmbItemPickerModel> = [];

	#close() {
		this.modalContext?.reject();
	}

	#filter(event: { target: HTMLInputElement }) {
		if (!this.data) return;

		if (event.target.value) {
			const query = event.target.value.toLowerCase();
			this._filtered = this.data.items.filter(
				(item) => item.label.toLowerCase().includes(query) || item.value.toLowerCase().includes(query),
			);
		} else {
			this._filtered = this.data.items;
		}
	}

	#submit(item: UmbItemPickerModel) {
		this.modalContext?.setValue(item);
		this.modalContext?.submit();
	}

	override connectedCallback() {
		super.connectedCallback();

		if (!this.data) return;
		this._filtered = this.data.items;
	}

	override render() {
		if (!this.data) return nothing;
		const items = this._filtered;
		return html`
			<umb-body-layout headline=${this.localize.string(this.data.headline)}>
				<div id="main">
					<uui-input
						type="search"
						placeholder=${this.localize.term('placeholders_filter')}
						@input=${this.#filter}
						${umbFocus()}>
						<div slot="prepend">
							<uui-icon name="search"></uui-icon>
						</div>
					</uui-input>
					${when(
						items.length,
						() => html`
							<uui-box>
								<uui-ref-list>
									${repeat(
										items,
										(item) => item.value,
										(item) => html`
											<umb-ref-item
												name=${this.localize.string(item.label)}
												detail=${ifDefined(item.description)}
												icon=${ifDefined(item.icon)}
												@open=${() => this.#submit(item)}>
											</umb-ref-item>
										`,
									)}
								</uui-ref-list>
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

	static override styles = [
		UmbTextStyles,
		css`
			#main {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}

			uui-box > uui-input {
				width: 100%;
			}

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
