import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserGroupItemModel } from '../../repository/index.js';
import { UmbUserGroupPickerInputContext } from './user-group-input.context.js';
import { css, html, customElement, property, state, ifDefined, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';
import '@umbraco-cms/backoffice/entity-item';

@customElement('umb-user-group-input')
export class UmbUserGroupInputElement extends UUIFormControlMixin(UmbLitElement, '') {
	/**
	 * This is a minimum amount of selected items in this input.
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
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
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
	maxMessage = 'This field exceeds the allowed amount of items';

	public set selection(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property()
	public override set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selection = splitStringToArray(idsString);
	}
	public override get value(): string {
		return this.selection.join(',');
	}

	@state()
	private _items?: Array<UmbUserGroupItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	#pickerContext = new UmbUserGroupPickerInputContext(this);

	@state()
	private _editUserGroupPath = '';

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

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), '_observeSelection');
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), '_observerItems');
		this.observe(this.#pickerContext.statuses, (statuses) => (this._statuses = statuses), '_observeStatuses');

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_USER_GROUP_ENTITY_TYPE)
			.onSetup(async () => {
				return { data: { entityType: UMB_USER_GROUP_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editUserGroupPath = routeBuilder({});
			});
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			<uui-ref-list>
				${this._statuses
					? repeat(
							this._statuses,
							(status) => status.unique,
							(status) => this.#renderItem(status),
					  )
					: nothing}
			</uui-ref-list>
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${() => this.#pickerContext.openPicker()}
				label=${this.localize.term('general_choose')}></uui-button>
		`;
	}

	#renderItem(status: UmbRepositoryItemsStatus) {
		const unique = status.unique;
		const item = this._items?.find((x) => x.unique === unique);
		const isError = status.state.type === 'error';

		// For error state, use umb-entity-item-ref
		if (isError) {
			return html`<umb-entity-item-ref
				id=${unique}
				?error=${true}
				.errorMessage=${status.state.error}
				.errorDetail=${unique}>
				<uui-action-bar slot="actions">
					<uui-button
						label=${this.localize.term('general_remove')}
						@click=${() => this.#pickerContext.requestRemoveItem(unique)}></uui-button>
				</uui-action-bar>
			</umb-entity-item-ref>`;
		}

		// For successful items, use umb-user-group-ref
		if (!item?.unique) return nothing;
		const href = `${this._editUserGroupPath}edit/${unique}`;
		return html`
			<umb-user-group-ref name="${ifDefined(item.name)}" href=${href}>
				${item.icon ? html`<umb-icon slot="icon" name=${item.icon}></umb-icon>` : nothing}
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(unique)}
						label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</umb-user-group-ref>
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

export default UmbUserGroupInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-input': UmbUserGroupInputElement;
	}
}
