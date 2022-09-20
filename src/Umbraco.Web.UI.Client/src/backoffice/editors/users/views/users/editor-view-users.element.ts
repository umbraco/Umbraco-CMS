import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import type { DocumentTypeEntity } from '../../../../../mocks/data/document-type.data';
import { Subscription, distinctUntilChanged } from 'rxjs';
import './editor-view-users-list.element';

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

			#user-grid {
				display: none; //TODO Remove
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
				margin-top: var(--uui-size-space-4);
			}

			uui-card-user {
				width: 100%;
				height: 180px;
			}

			.user-login-time {
				margin-top: auto;
			}
		`,
	];

	render() {
		return html`<div id="top-bar">
				<uui-button label="Invite user" look="outline"></uui-button>
				<div>
					<uui-button look="outline" compact>
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
				<div id="user-grid">
					<uui-card-user name="John Rabbit">
						<uui-tag slot="tag" size="s">Invited</uui-tag>
						<div>Editor</div>
						<div class="user-login-time">Has not logged in yet</div>
					</uui-card-user>

					<uui-card-user name="Prince Fox">
						<div>Admin</div>
						<div class="user-login-time">Has not logged in yet</div>
					</uui-card-user>

					<uui-card-user name="Lisa Strong">
						<div>Translator</div>
						<div class="user-login-time">Has not logged in yet</div>
					</uui-card-user>
				</div>

				<umb-editor-view-users-list></umb-editor-view-users-list>
			</div> `;
	}
}

export default UmbEditorViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users': UmbEditorViewUsersElement;
	}
}
