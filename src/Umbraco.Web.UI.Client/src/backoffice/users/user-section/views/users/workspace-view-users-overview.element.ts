import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UUIPopoverElement } from '@umbraco-ui/uui';
import { UMB_INVITE_USER_MODAL_TOKEN } from '../../../../users/users/modals/invite-user';
import { UMB_CREATE_USER_MODAL_TOKEN } from '../../../../users/users/modals/create-user';
import type { UmbSectionViewUsersElement } from './section-view-users.element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import type { IRoute } from '@umbraco-cms/router';

import { UmbLitElement } from '@umbraco-cms/element';

import './list-view-layouts/table/workspace-view-users-table.element';
import './list-view-layouts/grid/workspace-view-users-grid.element';
import './workspace-view-users-selection.element';

export type UsersViewType = 'list' | 'grid';
@customElement('umb-workspace-view-users-overview')
export class UmbWorkspaceViewUsersOverviewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
			}

			#sticky-top {
				position: sticky;
				top: -1px;
				z-index: 1;
				box-shadow: 0 1px 3px rgba(0, 0, 0, 0), 0 1px 2px rgba(0, 0, 0, 0);
				transition: 250ms box-shadow ease-in-out;
			}

			#sticky-top.header-shadow {
				box-shadow: var(--uui-shadow-depth-2);
			}

			#user-list-top-bar {
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				background-color: var(--uui-color-background);
				display: flex;
				justify-content: space-between;
				white-space: nowrap;
				gap: var(--uui-size-space-5);
				align-items: center;
			}
			#user-list {
				padding: var(--uui-size-space-6);
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
			router-slot {
				overflow: hidden;
			}
		`,
	];

	@state()
	private _selection: Array<string> = [];

	@state()
	private isCloud = false; //NOTE: Used to show either invite or create user buttons and views.

	@state()
	private _routes: IRoute[] = [
		{
			path: 'grid',
			component: () => import('./list-view-layouts/grid/workspace-view-users-grid.element'),
		},
		{
			path: 'list',
			component: () => import('./list-view-layouts/table/workspace-view-users-table.element'),
		},
		{
			path: '**',
			redirectTo: 'grid',
		},
	];

	private _usersContext?: UmbSectionViewUsersElement;
	private _modalContext?: UmbModalContext;
	private _inputTimer?: NodeJS.Timeout;
	private _inputTimerAmount = 500;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUsersContext', (usersContext: UmbSectionViewUsersElement) => {
			this._usersContext = usersContext;
			this._observeSelection();
		});

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _observeSelection() {
		if (!this._usersContext) return;
		this.observe(this._usersContext.selection, (selection) => (this._selection = selection));
	}

	private _toggleViewType() {
		const isList = window.location.pathname.split('/').pop() === 'list';

		isList
			? history.pushState(null, '', 'section/users/view/users/overview/grid')
			: history.pushState(null, '', 'section/users/view/users/overview/list');
	}

	private _renderSelection() {
		if (this._selection.length === 0) return nothing;

		return html`<umb-workspace-view-users-selection></umb-workspace-view-users-selection>`;
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
		const search = target.value || '';
		clearTimeout(this._inputTimer);
		this._inputTimer = setTimeout(() => this._refreshUsers(search), this._inputTimerAmount);
	}

	private _refreshUsers(search: string) {
		if (!this._usersContext) return;
		this._usersContext.setSearch(search);
	}

	private _showInviteOrCreate() {
		let token = undefined;
		// TODO: we need to find a better way to determine if we should create or invite
		if (this.isCloud) {
			token = UMB_INVITE_USER_MODAL_TOKEN;
		} else {
			token = UMB_CREATE_USER_MODAL_TOKEN;
		}

		this._modalContext?.open(token);
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

			<umb-router-slot .routes=${this._routes}></umb-router-slot>

			${this._renderSelection()}
		`;
	}
}

export default UmbWorkspaceViewUsersOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-users-overview': UmbWorkspaceViewUsersOverviewElement;
	}
}
