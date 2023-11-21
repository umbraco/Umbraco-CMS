import { UmbUserCollectionContext } from './user-collection.context.js';
import {
	UUIBooleanInputEvent,
	UUICheckboxElement,
	UUIRadioGroupElement,
	UUIRadioGroupEvent,
} from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDropdownElement } from '@umbraco-cms/backoffice/components';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import {
	UMB_CREATE_USER_MODAL,
	UMB_INVITE_USER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UserGroupResponseModel, UserOrderModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbUserGroupCollectionRepository } from '@umbraco-cms/backoffice/user-group';

@customElement('umb-user-collection-header')
export class UmbUserCollectionHeaderElement extends UmbLitElement {
	@state()
	private _stateFilterOptions: Array<UserStateModel> = Object.values(UserStateModel);

	@state()
	private _stateFilterSelection: Array<UserStateModel> = [];

	@state()
	private _orderByOptions: Array<UserOrderModel> = Object.values(UserOrderModel);

	@state()
	private _orderBy?: UserOrderModel;

	@state()
	private _userGroups: Array<UserGroupResponseModel> = [];

	@state()
	private _userGroupFilterSelection: Array<UserGroupResponseModel> = [];

	#modalContext?: UmbModalManagerContext;
	#collectionContext?: UmbUserCollectionContext;
	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
		});
	}

	protected firstUpdated() {
		this.#requestUserGroups();
	}

	async #requestUserGroups() {
		const { data } = await this.#userGroupCollectionRepository.requestCollection();

		if (data) {
			this._userGroups = data.items;
		}
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

	#onUserGroupFilterChange(event: UUIBooleanInputEvent) {
		const target = event.currentTarget as UUICheckboxElement;
		const item = this._userGroups.find((group) => group.id === target.value);

		if (!item) return;

		if (target.checked) {
			this._userGroupFilterSelection = [...this._userGroupFilterSelection, item];
		} else {
			this._userGroupFilterSelection = this._userGroupFilterSelection.filter((group) => group.id !== item.id);
		}

		const ids = this._userGroupFilterSelection.map((group) => group.id!);
		this.#collectionContext?.setUserGroupFilter(ids);
	}

	#getUserGroupFilterLabel() {
		return this._userGroupFilterSelection.length === 0
			? this.localize.term('general_all')
			: this._userGroupFilterSelection.map((group) => group.name).join(', ');
	}

	#getStatusFilterLabel() {
		return this._stateFilterSelection.length === 0
			? this.localize.term('general_all')
			: this._stateFilterSelection.map((state) => this.localize.term('user_state' + state)).join(', ');
	}

	#renderFilters() {
		return html` ${this.#renderStatusFilter()} ${this.#renderUserGroupFilter()} `;
	}

	#renderStatusFilter() {
		return html`
			<uui-button popovertarget="popover-user-status-filter" label="status">
				<umb-localize key="general_status"></umb-localize>: ${this.#getStatusFilterLabel()}
			</uui-button>
			<uui-popover-container id="popover-user-status-filter" popover placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${this._stateFilterOptions.map(
							(option) =>
								html`<uui-checkbox
									label=${this.localize.term('user_state' + option)}
									@change=${this.#onStateFilterChange}
									name="state"
									value=${option}></uui-checkbox>`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderUserGroupFilter() {
		return html`
			<uui-button popovertarget="popover-user-group-filter" label=${this.localize.term('general_groups')}>
				<umb-localize key="general_groups"></umb-localize>: ${this.#getUserGroupFilterLabel()}
			</uui-button>
			<uui-popover-container id="popover-user-group-filter" popover placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${repeat(
							this._userGroups,
							(group) => group.id,
							(group) => html`
								<uui-checkbox
									label=${ifDefined(group.name)}
									value=${ifDefined(group.id)}
									@change=${this.#onUserGroupFilterChange}></uui-checkbox>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderCollectionViews() {
		return html` <umb-collection-view-bundle></umb-collection-view-bundle> `;
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

			.filter {
				max-width: 200px;
			}

			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
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
