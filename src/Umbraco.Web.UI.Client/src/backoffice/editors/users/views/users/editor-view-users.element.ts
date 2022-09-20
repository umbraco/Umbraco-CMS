import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import type { DocumentTypeEntity } from '../../../../../mocks/data/document-type.data';
import { Subscription, distinctUntilChanged } from 'rxjs';
import './editor-view-users-list.element';
import './editor-view-users-grid.element';

export type UsersViewType = 'list' | 'grid';

@customElement('umb-editor-view-users')
export class UmbEditorViewUsersElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			#top-bar,
			#user-list-top-bar {
				display: flex;
				justify-content: space-between;
			}

			#user-list {
				margin-top: var(--uui-size-layout-2);
				font-size: 1rem;
			}
		`,
	];

	@state()
	private _viewType: UsersViewType = 'grid';

	@state()
	private _users = [
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
			lastLogin: 'Tue, 6 June 2021', // random date
			status: 'Invited',
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

	private _renderViewType() {
		switch (this._viewType) {
			case 'list':
				return html`<umb-editor-view-users-list .users=${this._users}></umb-editor-view-users-list>`;
			case 'grid':
				return html`<umb-editor-view-users-grid .users=${this._users}></umb-editor-view-users-grid>`;
			default:
				return html`<umb-editor-view-users-grid .users=${this._users}></umb-editor-view-users-grid>`;
		}
	}

	private _toggleViewType() {
		this._viewType = this._viewType === 'list' ? 'grid' : 'list';
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
