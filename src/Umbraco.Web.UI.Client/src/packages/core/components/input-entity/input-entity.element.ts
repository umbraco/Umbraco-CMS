import { css, html, customElement, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbUniqueItemModel } from '@umbraco-cms/backoffice/models';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-entity')
export class UmbInputEntityElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputEntity',
		itemSelector: 'uui-ref-node',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	protected override getFormElement() {
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
	getIcon?: (item: UmbUniqueItemModel) => string;

	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Array })
	public set selection(uniques: Array<string>) {
		this.#pickerContext?.setSelection(uniques);
		this.#sorter.setModel(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerContext?.getSelection() ?? [];
	}

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	@property({ attribute: false })
	public set pickerContext(ctor: (new (host: UmbControllerHost) => UmbPickerInputContext) | undefined) {
		if (this.#pickerContext) return;
		this.#pickerContext = ctor ? new ctor(this) : undefined;
		this.#observePickerContext();
	}
	public get pickerContext(): UmbPickerInputContext | undefined {
		return this.#pickerContext;
	}

	@state()
	private _items?: Array<UmbUniqueItemModel>;

	#pickerContext?: UmbPickerInputContext;

	constructor() {
		super();

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

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), '_observeSelection');
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), '_observerItems');
	}

	#openPicker() {
		this.#pickerContext?.openPicker({
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			// TODO: ignoring this for now to prevent breaking existing functionality.
			// if we want a very generic input it should be possible to pass in picker config
			hideTreeRoot: true,
		});
	}

	#removeItem(item: UmbUniqueItemModel) {
		this.#pickerContext?.requestRemoveItem(item.unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max === 1 && this.selection && this.selection.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}></uui-button>
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
			<uui-ref-node name=${item.name} id=${item.unique}>
				${when(icon, () => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#removeItem(item)} label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static override styles = [
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
