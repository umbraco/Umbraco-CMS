import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIPopoverElement } from '@umbraco-ui/uui';
import { UmbUserCollectionContext } from './user-collection.context';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import {
	UMB_CREATE_USER_MODAL,
	UMB_INVITE_USER_MODAL,
	UMB_MODAL_CONTEXT_TOKEN,
	UmbModalContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-user-collection-header')
export class UmbUserCollectionHeaderElement extends UmbLitElement {
	@state()
	private isCloud = true; //NOTE: Used to show either invite or create user buttons and views.

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
			this.#collectionContext = instance;
		});
	}

	private _toggleViewType() {
		const isList = window.location.pathname.split('/').pop() === 'list';

		isList
			? history.pushState(null, '', 'section/users/view/users/overview/grid')
			: history.pushState(null, '', 'section/users/view/users/overview/list');
	}

	private _handleTogglePopover(event: PointerEvent) {
		const composedPath = event.composedPath();

		const popover = composedPath.find((el) => el instanceof UUIPopoverElement) as UUIPopoverElement;
		if (popover) {
			popover.open = !popover.open;
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
		if (this.isCloud) {
			token = UMB_INVITE_USER_MODAL;
		} else {
			token = UMB_CREATE_USER_MODAL;
		}

		this.#modalContext?.open(token);
	}

	render() {
		return html`
			<div id="sticky-top">
				<div id="user-list-top-bar">
					<uui-button
						@click=${this._showInviteOrCreate}
						label=${this.isCloud ? 'Invite' : 'Create' + ' user'}
						look="outline"></uui-button>
					<uui-input @input=${this._updateSearch} label="search" id="input-search"></uui-input>
					<div>
						<!-- TODO: consider making this a shared component, as we need similar for other locations, example media library, members. -->
						<uui-popover margin="8">
							<uui-button @click=${this._handleTogglePopover} slot="trigger" label="status">
								Status: <b>All</b>
							</uui-button>
							<div slot="popover" class="filter-dropdown">
								<uui-checkbox label="Active"></uui-checkbox>
								<uui-checkbox label="Inactive"></uui-checkbox>
								<uui-checkbox label="Invited"></uui-checkbox>
								<uui-checkbox label="Disabled"></uui-checkbox>
							</div>
						</uui-popover>
						<uui-popover margin="8">
							<uui-button @click=${this._handleTogglePopover} slot="trigger" label="groups">
								Groups: <b>All</b>
							</uui-button>
							<div slot="popover" class="filter-dropdown">
								<uui-checkbox label="Active"></uui-checkbox>
								<uui-checkbox label="Inactive"></uui-checkbox>
								<uui-checkbox label="Invited"></uui-checkbox>
								<uui-checkbox label="Disabled"></uui-checkbox>
							</div>
						</uui-popover>
						<uui-popover margin="8">
							<uui-button @click=${this._handleTogglePopover} slot="trigger" label="order by">
								Order by: <b>Name (A-Z)</b>
							</uui-button>
							<div slot="popover" class="filter-dropdown">
								<uui-checkbox label="Active"></uui-checkbox>
								<uui-checkbox label="Inactive"></uui-checkbox>
								<uui-checkbox label="Invited"></uui-checkbox>
								<uui-checkbox label="Disabled"></uui-checkbox>
							</div>
						</uui-popover>
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
			#user-list {
				padding: var(--uui-size-layout-1);
				padding-top: var(--uui-size-space-2);
			}
			#input-search {
				width: 100%;
			}

			uui-popover {
				width: unset;
			}

			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				background-color: var(--uui-color-surface);
				padding: var(--uui-size-space-4);
				border-radius: var(--uui-size-border-radius);
				box-shadow: var(--uui-shadow-depth-2);
				width: fit-content;
			}
			a {
				color: inherit;
				text-decoration: none;
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
