import { UmbUserCollectionContext } from './user-collection.context.js';
import {
	UUIBooleanInputEvent,
	UUICheckboxElement,
	UUIRadioGroupElement,
	UUIRadioGroupEvent,
} from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
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

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
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

	private _updateSearch(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		const filter = target.value || '';
		clearTimeout(this.#inputTimer);
		this.#inputTimer = setTimeout(() => this.#collectionContext?.setFilter({ filter }), this.#inputTimerAmount);
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
			<umb-collection-action-bundle></umb-collection-action-bundle>
			${this.#renderSearch()}
			<div>${this.#renderFilters()} ${this.#renderCollectionViews()}</div>
		`;
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
		const length = this._userGroupFilterSelection.length;
		const max = 2;
		//TODO: Temp solution to limit the amount of states shown
		return length === 0
			? this.localize.term('general_all')
			: this._userGroupFilterSelection
					.slice(0, max)
					.map((group) => group.name)
					.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	#getStatusFilterLabel() {
		const length = this._stateFilterSelection.length;
		const max = 2;
		//TODO: Temp solution to limit the amount of states shown
		return length === 0
			? this.localize.term('general_all')
			: this._stateFilterSelection
					.slice(0, max)
					.map((state) => this.localize.term('user_state' + state))
					.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	#renderFilters() {
		return html` ${this.#renderStatusFilter()} ${this.#renderUserGroupFilter()} `;
	}

	#renderStatusFilter() {
		return html`
			<uui-button popovertarget="popover-user-status-filter" label="status">
				<umb-localize key="general_status"></umb-localize>: <b>${this.#getStatusFilterLabel()}</b>
			</uui-button>
			<uui-popover-container id="popover-user-status-filter" placement="bottom">
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
				<umb-localize key="general_groups"></umb-localize>: <b>${this.#getUserGroupFilterLabel()}</b>
			</uui-button>
			<uui-popover-container id="popover-user-group-filter" placement="bottom">
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
