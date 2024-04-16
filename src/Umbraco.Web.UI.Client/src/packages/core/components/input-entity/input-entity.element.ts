import { css, html, customElement, property, state, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbUniqueItemModel } from '@umbraco-cms/backoffice/models';

@customElement('umb-input-entity')
export class UmbInputEntityElement extends UUIFormControlMixin(UmbLitElement, '') {
	// TODO: [LK] Add sort ordering.

	protected getFormElement() {
		return undefined;
	}
	@property({ type: Number })
	public set min(value: number) {
		this.#min = value;
		if (this.#pickerContext) {
			this.#pickerContext.min = value;
		}
	}
	public get min(): number {
		return this.#min;
	}
	#min: number = 0;

	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	@property({ type: Number })
	public set max(value: number) {
		this.#max = value;
		if (this.#pickerContext) {
			this.#pickerContext.max = value;
		}
	}
	public get max(): number {
		return this.#max;
	}
	#max: number = Infinity;

	@property({ attribute: false })
	getIcon?: (item: any) => string;

	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property()
	public set value(value: string) {
		this.selection = splitStringToArray(value);
	}
	public get value(): string {
		return this.selection?.join(',') ?? '';
	}

	@property({ attribute: false })
	public set pickerContext(ctor: new (host: UmbControllerHost) => UmbPickerInputContext<any>) {
		if (this.#pickerContext) return;
		this.#pickerContext = new ctor(this);
		this.#observePickerContext();
	}
	#pickerContext?: UmbPickerInputContext<any>;

	public set selection(value: Array<string>) {
		this.#pickerContext?.setSelection(value);
	}
	public get selection(): Array<string> | undefined {
		return this.#pickerContext?.getSelection();
	}

	@state()
	private _items?: Array<UmbUniqueItemModel>;

	constructor() {
		super();
	}

	connectedCallback() {
		super.connectedCallback();

		if (!this.#pickerContext) return;

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && (this.#pickerContext?.getSelection().length ?? 0) < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && (this.#pickerContext?.getSelection().length ?? 0) > this.max,
		);
	}

	async #observePickerContext() {
		if (!this.#pickerContext) return;

		this.#pickerContext.min = this.min;
		this.#pickerContext.max = this.max;

		this.observe(
			this.#pickerContext.selection,
			(selection) => (this.value = selection?.join(',') ?? ''),
			'observeSelection',
		);

		this.observe(
			this.#pickerContext.selectedItems,
			(selectedItems) => {
				this._items = selectedItems;
			},
			'observeSelectedItems',
		);
	}

	#openPicker() {
		this.#pickerContext?.openPicker({
			hideTreeRoot: true,
		});
	}

	render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max === 1 && this.selection && this.selection.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label="${this.localize.term('general_choose')}"></uui-button>
		`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-ref-list>
		`;
	}

	#renderItem(item: UmbUniqueItemModel) {
		if (!item.unique) return;
		const icon = this.getIcon?.(item) ?? item.icon ?? '';
		return html`
			<uui-ref-node name=${item.name}>
				${when(icon, () => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext?.requestRemoveItem(item.unique)}
						label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbInputEntityElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-entity': UmbInputEntityElement;
	}
}
