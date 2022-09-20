import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../../../core/context';
import './editor-view-users-list.element';
import './editor-view-users-grid.element';
import { BehaviorSubject, Observable } from 'rxjs';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';

export type UsersViewType = 'list' | 'grid';

export interface UserItem {
	key: string;
	name: string;
	userGroup: string;
	lastLogin: string;
	status?: string;
}

@customElement('umb-editor-view-users')
export class UmbEditorViewUsersElement extends UmbContextProviderMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			#top-bar,
			#user-list-top-bar {
				display: flex;
				justify-content: space-between;
			}

			#user-list-top-bar {
				margin-bottom: var(--uui-size-space-4);
			}

			#user-list {
				margin-top: var(--uui-size-layout-2);
				font-size: 1rem;
			}
		`,
	];

	private tempData = [
		{
			key: 'a9b18a00-58f2-420e-bf60-48d33ab156db',
			name: 'Cecílie Bryon',
			userGroup: 'Translators',
			lastLogin: 'Fri, 23 April 2021',
			status: 'Invited',
		},
		{
			key: '3179d0b2-eec2-4045-b86a-149e13b93e14',
			name: 'Kathleen G. Smith',
			userGroup: 'Editors',
			lastLogin: 'Tue, 6 June 2021',
			status: 'Disabled',
		},
		{
			key: '1b1c9733-b845-4d9a-9ed2-b2f46c05fd72',
			name: 'Adrian Andresen',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0207',
			name: 'Lorenza Trentino',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0202',
			name: 'John Doe',
			userGroup: 'Translators',
			lastLogin: 'Tue, 11 December 2021',
		},
	];

	@state()
	private _viewType: UsersViewType = 'grid';

	private _users: BehaviorSubject<Array<UserItem>> = new BehaviorSubject(this.tempData);
	public readonly users: Observable<Array<UserItem>> = this._users.asObservable();

	constructor() {
		super();

		this.provideContext('umbUsersContext', this);
	}

	private _renderViewType() {
		switch (this._viewType) {
			case 'list':
				return html`<umb-editor-view-users-list></umb-editor-view-users-list>`;
			case 'grid':
				return html`<umb-editor-view-users-grid></umb-editor-view-users-grid>`;
			default:
				return html`<umb-editor-view-users-grid></umb-editor-view-users-grid>`;
		}
	}

	private _toggleViewType() {
		this._viewType = this._viewType === 'list' ? 'grid' : 'list';
	}

	public getTagLookAndColor(status: string): { color: InterfaceColor; look: InterfaceLook } {
		switch (status.toLowerCase()) {
			case 'invited':
			case 'inactive':
				return { look: 'primary', color: 'warning' };
			case 'active':
				return { look: 'primary', color: 'positive' };
			case 'disabled':
				return { look: 'primary', color: 'danger' };
			default:
				return { look: 'secondary', color: 'default' };
		}
	}

	render() {
		return html`<div id="top-bar">
				<uui-button label="Invite user" look="outline"></uui-button>
				<div>
					<uui-button
						@click=${this._toggleViewType}
						look="${this._viewType === 'grid' ? 'outline' : 'primary'}"
						compact>
						<uui-icon name="settings"></uui-icon>
					</uui-button>
					<uui-button look="outline" compact>
						<uui-icon name="search"></uui-icon>
					</uui-button>
				</div>
			</div>

			<div id="user-list">
				<div id="user-list-top-bar">
					<b>Users (23)</b>
					<div>
						<uui-button> Status: <b>All</b> </uui-button>
						<uui-button> Groups: <b>All</b> </uui-button>
						<uui-button> Order by: <b>Name (A-Z)</b> </uui-button>
					</div>
				</div>

				${this._renderViewType()}
			</div> `;
	}
}

export default UmbEditorViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users': UmbEditorViewUsersElement;
	}
}
