import { UmbUserStateFilter } from './utils/index.js';
import { UMB_USER_COLLECTION_CONTEXT } from './user-collection.context-token.js';
import type { UmbUserOrderByOption } from './types.js';
import type { UmbUserStateFilterType } from './utils/index.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbUserGroupCollectionRepository } from '@umbraco-cms/backoffice/user-group';
import type { UmbUserGroupDetailModel } from '@umbraco-cms/backoffice/user-group';
import type { UUIBooleanInputEvent, UUICheckboxElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-user-collection-header')
export class UmbUserCollectionHeaderElement extends UmbLitElement {
	@state()
	private _stateFilterOptions: Array<UmbUserStateFilterType> = Object.values(UmbUserStateFilter);

	@state()
	private _stateFilterSelection: Array<UmbUserStateFilterType> = [];

	@state()
	private _userGroups: Array<UmbUserGroupDetailModel> = [];

	@state()
	private _userGroupFilterSelection: Array<UmbUserGroupDetailModel> = [];

	@state()
	private _orderByOptions: Array<UmbUserOrderByOption> = [];

	@state()
	_activeOrderByOption?: UmbUserOrderByOption;

	#collectionContext?: typeof UMB_USER_COLLECTION_CONTEXT.TYPE;

	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeOrderByOptions();
		});
	}

	#observeOrderByOptions() {
		if (!this.#collectionContext) return;
		this.observe(
			observeMultiple([this.#collectionContext.orderByOptions, this.#collectionContext.activeOrderByOption]),
			([options, activeOption]) => {
				// the options are hardcoded in the context, so we can just compare the length
				if (this._orderByOptions.length !== options.length) {
					this._orderByOptions = options;
				}

				if (activeOption && activeOption !== this._activeOrderByOption?.unique) {
					this._activeOrderByOption = this._orderByOptions.find((option) => option.unique === activeOption);
				}
			},
			'_umbObserveUserOrderByOptions',
		);
	}

	protected override firstUpdated() {
		this.#requestUserGroups();
	}

	async #requestUserGroups() {
		const { data } = await this.#userGroupCollectionRepository.requestCollection();

		if (data) {
			this._userGroups = data.items;
		}
	}

	#onStateFilterChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.currentTarget as UUICheckboxElement;
		const value = target.value as UmbUserStateFilterType;
		const isChecked = target.checked;

		this._stateFilterSelection = isChecked
			? [...this._stateFilterSelection, value]
			: this._stateFilterSelection.filter((v) => v !== value);

		this.#collectionContext?.setStateFilter(this._stateFilterSelection);
	}

	#onUserGroupFilterChange(event: UUIBooleanInputEvent) {
		const target = event.currentTarget as UUICheckboxElement;
		const item = this._userGroups.find((group) => group.unique === target.value);

		if (!item) return;

		if (target.checked) {
			this._userGroupFilterSelection = [...this._userGroupFilterSelection, item];
		} else {
			this._userGroupFilterSelection = this._userGroupFilterSelection.filter((group) => group.unique !== item.unique);
		}

		const uniques = this._userGroupFilterSelection.map((group) => group.unique);
		this.#collectionContext?.setUserGroupFilter(uniques);
	}

	#onOrderByChange(option: UmbUserOrderByOption) {
		this.#collectionContext?.setActiveOrderByOption(option.unique);
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

	override render() {
		return html`
			<umb-collection-toolbar slot="header">
				<div id="toolbar">
					<umb-collection-filter-field></umb-collection-filter-field>
					${this.#renderStatusFilter()} ${this.#renderUserGroupFilter()} ${this.#renderOrderBy()}
				</div>
			</umb-collection-toolbar>
		`;
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
							(group) => group.unique,
							(group) => html`
								<uui-checkbox
									label=${ifDefined(group.name)}
									value=${ifDefined(group.unique)}
									@change=${this.#onUserGroupFilterChange}></uui-checkbox>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderOrderBy() {
		return html`
			<uui-button popovertarget="popover-order-by-filter" label="order by">
				<umb-localize key="general_orderBy"></umb-localize>:
				<b> ${this._activeOrderByOption ? this.localize.string(this._activeOrderByOption.label) : ''}</b>
			</uui-button>
			<uui-popover-container id="popover-order-by-filter" placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${this._orderByOptions.map(
							(option) => html`
								<uui-menu-item
									label=${this.localize.string(option.label)}
									@click-label=${() => this.#onOrderByChange(option)}
									?active=${this._activeOrderByOption?.unique === option.unique}></uui-menu-item>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
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

			#toolbar {
				flex: 1;
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				align-items: center;
			}

			umb-collection-filter-field {
				width: 100%;
			}

			.filter {
				max-width: 200px;
			}

			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				padding: var(--uui-size-space-3);
				overflow-y: auto;
    			max-height: 500px;
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
