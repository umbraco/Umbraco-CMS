import type { UmbStaticFileItemModel } from '../../repository/item/types.js';
import { UmbStaticFilePickerInputContext } from './input-static-file.context.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import '@umbraco-cms/backoffice/entity-item';

@customElement('umb-input-static-file')
export class UmbInputStaticFileElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#serializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * This is a minimum amount of selected files in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set min(value: number) {
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
	}

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more files';

	/**
	 * This is a maximum amount of selected files in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set max(value: number) {
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of files';

	public set selection(paths: Array<string>) {
		this.#pickerContext.setSelection(paths);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	@property()
	public pickableFilter?: (item: UmbStaticFileItemModel) => boolean;

	@state()
	private _items?: Array<UmbStaticFileItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	#pickerContext = new UmbStaticFilePickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
		this.observe(this.#pickerContext.statuses, (statuses) => (this._statuses = statuses));
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		if (!this._statuses) return nothing;
		return html`
			<uui-ref-list>
				${repeat(
					this._statuses,
					(status) => status.unique,
					(status) => this.#renderItem(status),
				)}
			</uui-ref-list>
			${this.#renderAddButton()}
		`;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			pickableFilter: this.pickableFilter,
			multiple: this.max === 1 ? false : true,
			hideTreeRoot: true,
		});
	}

	#renderAddButton() {
		if (this.max === 1 && this.selection.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}></uui-button>
		`;
	}

	#renderItem(status: UmbRepositoryItemsStatus) {
		const unique = status.unique;
		const item = this._items?.find((x) => x.unique === unique);
		const isError = status.state.type === 'error';

		return html`<umb-entity-item-ref
			id=${unique}
			.item=${item}
			?error=${isError}
			.errorMessage=${status.state.error}
			.errorDetail=${isError ? this.#serializer.toServerPath(unique) : undefined}
			?standalone=${this.max === 1}>
			<uui-action-bar slot="actions">
				<uui-button
					label=${this.localize.term('general_remove')}
					@click=${() => this.#pickerContext.requestRemoveItem(unique)}></uui-button>
			</uui-action-bar>
		</umb-entity-item-ref>`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbInputStaticFileElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-static-file': UmbInputStaticFileElement;
	}
}
