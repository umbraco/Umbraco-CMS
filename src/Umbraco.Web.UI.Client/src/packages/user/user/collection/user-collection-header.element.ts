import { UmbUserCollectionContext } from './user-collection.context.js';
import {
	UUIBooleanInputEvent,
	UUICheckboxElement,
	UUIRadioGroupElement,
	UUIRadioGroupEvent,
} from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDropdownElement } from '@umbraco-cms/backoffice/components';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import {
	UMB_CREATE_USER_MODAL,
	UMB_INVITE_USER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UserOrderModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-collection-header')
export class UmbUserCollectionHeaderElement extends UmbLitElement {
	@state()
	private _isCloud = false; //NOTE: Used to show either invite or create user buttons and views.

	@state()
	private _stateFilterOptions: Array<UserStateModel> = Object.values(UserStateModel);

	@state()
	private _stateFilterSelection: Array<UserStateModel> = [];

	@state()
	private _orderByOptions: Array<UserOrderModel> = Object.values(UserOrderModel);

	@state()
	private _orderBy?: UserOrderModel;

	#modalContext?: UmbModalManagerContext;
	#collectionContext?: UmbUserCollectionContext;
	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
		});
	}

	#onDropdownClick(event: PointerEvent) {
		const composedPath = event.composedPath();

		const dropdown = composedPath.find((el) => el instanceof UmbDropdownElement) as UmbDropdownElement;
		if (dropdown) {
			dropdown.open = !dropdown.open;
		}
	}

	private _updateSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
	}

	#onCreateUserClick() {
		this.#modalContext?.open(UMB_CREATE_USER_MODAL);
	}

	#onInviteUserClick() {
		this.#modalContext?.open(UMB_INVITE_USER_MODAL);
	}

	#onStateFilterChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUICheckboxElement;
		const value = target.value as UserStateModel;
		const isChecked = target.checked;

		this._stateFilterSelection = isChecked
			? [...this._stateFilterSelection, value]
			: this._stateFilterSelection.filter((v) => v !== value);

		this.#collectionContext?.setStateFilter(this._stateFilterSelection);
	}

	#onOrderByChange(event: UUIRadioGroupEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUIRadioGroupElement | null;

		if (target) {
			this._orderBy = target.value as UserOrderModel;
			this.#collectionContext?.setOrderByFilter(this._orderBy);
		}
	}

	render() {
		return html`
			${this.#renderCollectionActions()} ${this.#renderSearch()} ${this.#renderFilters()}
			${this.#renderCollectionViews()}
		`;
	}

	#renderCollectionActions() {
		return html` <uui-button
				@click=${this.#onCreateUserClick}
				label=${this.localize.term('user_createUser')}
				look="outline"></uui-button>

			<uui-button
				@click=${this.#onInviteUserClick}
				label=${this.localize.term('user_inviteUser')}
				look="outline"></uui-button>`;
	}

	#renderSearch() {
		return html`
			<uui-input
				@input=${this._updateSearch}
				label=${this.localize.term('visuallyHiddenTexts_userSearchLabel')}
				placeholder=${this.localize.term('visuallyHiddenTexts_userSearchLabel')}
				id="input-search"></uui-input>
		`;
	}

	#renderFilters() {
		return html`
			<div>
				<!-- TODO: we should consider using the uui-combobox. We need to add a multiple options to it first -->
				<umb-dropdown margin="8">
					<uui-button @click=${this.#onDropdownClick} slot="trigger" label="status">
						<umb-localize key="general_status"></umb-localize>:
						<umb-localize key=${'user_state' + this._stateFilterSelection}></umb-localize>
					</uui-button>
					<div slot="dropdown" class="filter-dropdown">
						${this._stateFilterOptions.map(
							(option) =>
								html`<uui-checkbox
									label=${this.localize.term('user_state' + option)}
									@change=${this.#onStateFilterChange}
									name="state"
									value=${option}></uui-checkbox>`,
						)}
					</div>
				</umb-dropdown>

				<!-- TODO: we should consider using the uui-combobox. We need to add a multiple options to it first -->
				<umb-dropdown margin="8">
					<uui-button @click=${this.#onDropdownClick} slot="trigger" label=${this.localize.term('general_groups')}>
						<umb-localize key="general_groups"></umb-localize>:
						<!-- TODO: show the value here -->
					</uui-button>
					<div slot="dropdown" class="filter-dropdown">
						<!-- TODO: GET THESE FROM SERVER (not localized) -->
						<uui-checkbox label="Administrators"></uui-checkbox>
						<uui-checkbox label="Editors"></uui-checkbox>
						<uui-checkbox label="Sensitive Data"></uui-checkbox>
						<uui-checkbox label="Translators"></uui-checkbox>
						<uui-checkbox label="Writers"></uui-checkbox>
					</div>
				</umb-dropdown>

				<!-- TODO: we should consider using the uui-combobox. We need to add a multiple options to it first -->
				<umb-dropdown margin="8">
					<uui-button @click=${this.#onDropdownClick} slot="trigger" label=${this.localize.term('general_orderBy')}>
						<umb-localize key="general_orderBy"></umb-localize>:
						<b>${this._orderBy}</b>
					</uui-button>
					<div slot="dropdown" class="filter-dropdown" name="orderBy">
						<uui-radio-group name="radioGroup" @change=${this.#onOrderByChange}>
							${this._orderByOptions.map((option) => html`<uui-radio label=${option} value=${option}></uui-radio>`)}
						</uui-radio-group>
					</div>
				</umb-dropdown>
			</div>
		`;
	}

	#renderCollectionViews() {
		return html`
			<uui-button label="view toggle" compact look="outline">
				<uui-icon name="settings"></uui-icon>
			</uui-button>
		`;
	}

	static styles = [
		css`
			:host {
				height: 100%;
				width: 100%;
				display: flex;
				justify-content: space-between;
				white-space: nowrap;
				gap: var(--uui-size-space-5);
				align-items: center;
			}

			#input-search {
				width: 100%;
			}

			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				width: fit-content;
			}
		`,
	];
}

export default UmbUserCollectionHeaderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection-header': UmbUserCollectionHeaderElement;
	}
}
