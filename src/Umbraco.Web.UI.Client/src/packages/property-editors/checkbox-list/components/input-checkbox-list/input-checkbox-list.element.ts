import { css, html, nothing, repeat, customElement, property, classMap } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

export type UmbCheckboxListItem = { label: string; value: string; checked: boolean; invalid?: boolean };

@customElement('umb-input-checkbox-list')
export class UmbInputCheckboxListElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ attribute: false })
	public list: Array<UmbCheckboxListItem> = [];

	#selection: Array<string> = [];
	@property({ type: Array })
	public set selection(values: Array<string>) {
		this.#selection = values;
		super.value = values.join(',');
	}
	public get selection(): Array<string> {
		return this.#selection;
	}

	@property()
	public override set value(value: string) {
		this.selection = value.split(',');
	}
	public override get value(): string {
		return this.selection.join(',');
	}

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

	#onChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		if (event.target.checked) this.selection = [...this.selection, event.target.value];
		else this.#removeFromSelection(this.selection.findIndex((value) => event.target.value === value));

		this.dispatchEvent(new UmbChangeEvent());
	}

	#removeFromSelection(index: number) {
		if (index == -1) return;
		const values = [...this.selection];
		values.splice(index, 1);
		this.selection = values;
	}

	override render() {
		if (!this.list) return nothing;
		return html`
			<form>
				<uui-form @change=${this.#onChange}>
					${repeat(
						this.list,
						(item) => item.value,
						(item) => this.#renderCheckbox(item),
					)}
				</uui-form>
			</form>
		`;
	}

	#renderCheckbox(item: (typeof this.list)[0]) {
		return html`<uui-checkbox
			class=${classMap({ invalid: !!item.invalid })}
			?checked=${item.checked}
			label=${item.label + (item.invalid ? ` (${this.localize.term('validation_legacyOption')})` : '')}
			title=${item.invalid ? this.localize.term('validation_legacyOptionDescription') : ''}
			value=${item.value}
			?readonly=${this.readonly}></uui-checkbox>`;
	}

	static override readonly styles = [
		css`
			uui-checkbox {
				width: 100%;

				&.invalid {
					text-decoration: line-through;
				}
			}
		`,
	];
}

export default UmbInputCheckboxListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-checkbox-list': UmbInputCheckboxListElement;
	}
}
