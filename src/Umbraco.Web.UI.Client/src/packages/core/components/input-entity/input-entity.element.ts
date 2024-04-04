import {
	css,
	html,
	customElement,
	property,
	state,
	repeat,
	ifDefined,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';

@customElement('umb-input-entity')
export class UmbInputEntityElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}
	@property({ type: Number })
	public set min(value: number) {
		if (this.#pickerContext) {
			this.#pickerContext.min = value;
		}
	}
	public get min(): number {
		return this.#pickerContext?.min ?? 0;
	}

	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	@property({ type: Number })
	public set max(value: number) {
		if (this.#pickerContext) {
			this.#pickerContext.max = value;
		}
	}
	public get max(): number {
		return this.#pickerContext?.max ?? Infinity;
	}

	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property()
	public set value(value: string) {
		this.selection = splitStringToArray(value);
	}
	public get value(): string {
		return this.selection?.join(',') ?? '';
	}

	public set pickerContext(ctor: new (host: UmbControllerHost) => UmbPickerInputContext<any>) {
		if (this.#pickerContext) return;
		this.#pickerContext = new ctor(this);
		this.#observePickerContext();
	}
	public get pickerContext(): UmbPickerInputContext<any> | undefined {
		return this.#pickerContext;
	}
	#pickerContext?: UmbPickerInputContext<any>;

	public set selection(value: Array<string>) {
		this.#pickerContext?.setSelection(value);
	}
	public get selection(): Array<string> | undefined {
		return this.#pickerContext?.getSelection();
	}

	@state()
	// TODO: [LK] Find out if we can have a common interface for tree-picker entities, (rather than use `any`).
	private _items?: Array<any>;

	constructor() {
		super();
	}

	// connectedCallback() {
	// 	super.connectedCallback();

	// 	if (!this.#pickerContext) return;

	// 	this.addValidator(
	// 		'rangeUnderflow',
	// 		() => this.minMessage,
	// 		() => !!this.min && this.#pickerContext.getSelection().length < this.min,
	// 	);

	// 	this.addValidator(
	// 		'rangeOverflow',
	// 		() => this.maxMessage,
	// 		() => !!this.max && this.#pickerContext.getSelection().length > this.max,
	// 	);
	// }

	async #observePickerContext() {
		if (!this.#pickerContext) return;

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
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
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

	#renderItem(item: any) {
		if (!item.unique) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)}>
				${when(item.icon, () => html`<umb-icon slot="icon" name=${item.icon}></umb-icon>`)}
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
