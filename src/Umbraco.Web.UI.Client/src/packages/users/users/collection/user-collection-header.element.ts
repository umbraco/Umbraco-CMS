import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIBooleanInputEvent, UUICheckboxElement, UUIRadioGroupElement, UUIRadioGroupEvent } from '@umbraco-ui/uui';
import { UmbUserCollectionContext } from './user-collection.context';
import { UmbDropdownElement } from '@umbraco-cms/backoffice/components';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import {
	UMB_CREATE_USER_MODAL,
	UMB_INVITE_USER_MODAL,
	UMB_MODAL_CONTEXT_TOKEN,
	UmbModalContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserOrderModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-collection-header')
export class UmbUserCollectionHeaderElement extends UmbLitElement {
	@state()
	private _isCloud = true; //NOTE: Used to show either invite or create user buttons and views.

	@state()
	private _stateFilterOptions: Array<UserStateModel> = Object.values(UserStateModel);

	@state()
	private _stateFilterSelection: Array<UserStateModel> = [];

	@state()
	private _orderByOptions: Array<UserOrderModel> = Object.values(UserOrderModel);

	@state()
	private _orderBy?: UserOrderModel;

	#modalContext?: UmbModalContext;
	#collectionContext?: UmbUserCollectionContext;
	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 500;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
		});
	}

	// TODO: we need to render collection view extension
	private _toggleViewType() {
		/*
		const isList = window.location.pathname.split('/').pop() === 'list';

		isList
			? history.pushState(null, '', 'section/users/view/users/overview/grid')
			: history.pushState(null, '', 'section/users/view/users/overview/list');
			*/
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

	private _showInviteOrCreate() {
		let token = undefined;
		// TODO: we need to find a better way to determine if we should create or invite
		if (this._isCloud) {
			token = UMB_INVITE_USER_MODAL;
		} else {
			token = UMB_CREATE_USER_MODAL;
		}

		this.#modalContext?.open(token);
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
			<div id="sticky-top">
				<div id="user-list-top-bar">
					<uui-button
						@click=${this._showInviteOrCreate}
						label=${this._isCloud ? 'Invite' : 'Create' + ' user'}
						look="outline"></uui-button>
					<uui-input @input=${this._updateSearch} label="search" id="input-search"></uui-input>
					<div>
						<!-- TODO: we should consider using the uui-combobox. We need to add a multiple options to it first -->
						<umb-dropdown margin="8">
							<uui-button @click=${this.#onDropdownClick} slot="trigger" label="status">
								State: ${this._stateFilterSelection}
							</uui-button>
							<div slot="dropdown" class="filter-dropdown">
								${this._stateFilterOptions.map(
									(option) =>
										html`<uui-checkbox
											label=${option}
											@change=${this.#onStateFilterChange}
											name="state"
											value=${option}></uui-checkbox>`
								)}
							</div>
						</umb-dropdown>

						<!-- TODO: we should consider using the uui-combobox. We need to add a multiple options to it first -->
						<umb-dropdown margin="8">
							<uui-button @click=${this.#onDropdownClick} slot="trigger" label="order by"> Group: </uui-button>
							<div slot="dropdown" class="filter-dropdown">
								<uui-checkbox label="Active"></uui-checkbox>
								<uui-checkbox label="Inactive"></uui-checkbox>
								<uui-checkbox label="Invited"></uui-checkbox>
								<uui-checkbox label="Disabled"></uui-checkbox>
							</div>
						</umb-dropdown>

						<!-- TODO: we should consider using the uui-combobox. We need to add a multiple options to it first -->
						<umb-dropdown margin="8">
							<uui-button @click=${this.#onDropdownClick} slot="trigger" label="Order By">
								Order By: <b>${this._orderBy}</b>
							</uui-button>
							<div slot="dropdown" class="filter-dropdown" name="orderBy">
								<uui-radio-group name="radioGroup" @change=${this.#onOrderByChange}>
									${this._orderByOptions.map((option) => html`<uui-radio label=${option} value=${option}></uui-radio>`)}
								</uui-radio-group>
							</div>
						</umb-dropdown>

						<uui-button label="view toggle" @click=${this._toggleViewType} compact look="outline">
							<uui-icon name="settings"></uui-icon>
						</uui-button>
					</div>
				</div>
			</div>
		`;
	}
	static styles = [
		UUITextStyles,
		css`
			#sticky-top {
				position: sticky;
				top: 0px;
				z-index: 1;
				box-shadow: 0 1px 3px rgba(0, 0, 0, 0), 0 1px 2px rgba(0, 0, 0, 0);
				transition: 250ms box-shadow ease-in-out;
			}

			#sticky-top.header-shadow {
				box-shadow: var(--uui-shadow-depth-2);
			}

			#user-list-top-bar {
				padding: var(--uui-size-space-4) var(--uui-size-layout-1);
				background-color: var(--uui-color-background);
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
