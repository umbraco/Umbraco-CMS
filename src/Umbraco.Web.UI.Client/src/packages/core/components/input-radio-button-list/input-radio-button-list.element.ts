import { css, html, nothing, repeat, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin, UUIRadioElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIRadioEvent } from '@umbraco-cms/backoffice/external/uui';

type UmbRadioButtonItem = { label: string; value: string };

@customElement('umb-input-radio-button-list')
export class UmbInputRadioButtonListElement extends UUIFormControlMixin(UmbLitElement, '') {
	#value: string = '';

	@property()
	public override set value(value: string) {
		this.#value = value;
	}
	public override get value(): string {
		return this.#value;
	}

	@property({ type: Array })
	public list: Array<UmbRadioButtonItem> = [];

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	protected override getFormElement() {
		return undefined;
	}

	#onChange(event: UUIRadioEvent) {
		event.stopPropagation();
		if (!(event.target instanceof UUIRadioElement)) return;
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		if (!this.list) return nothing;

		return html`
			<uui-radio-group .value=${this.value} @change=${this.#onChange} ?readonly=${this.readonly}>
				${repeat(
					this.list,
					(item) => item,
					(item) => this.#renderRadioButton(item),
				)}
			</uui-radio-group>
		`;
	}

	#renderRadioButton(item: (typeof this.list)[0]) {
		return html`<uui-radio value=${item.value} label=${item.label}></uui-radio>`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbInputRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-radio-button-list': UmbInputRadioButtonListElement;
	}
}
