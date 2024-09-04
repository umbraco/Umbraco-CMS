import type { UmbUserDetailModel } from '../index.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import type { UmbUserWorkspaceContext } from './user-workspace.context.js';
import { UMB_USER_WORKSPACE_CONTEXT } from './user-workspace.context-token.js';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

// import local components. Theses are not meant to be used outside of this component.
import './components/user-workspace-profile-settings/user-workspace-profile-settings.element.js';
import './components/user-workspace-access/user-workspace-access.element.js';
import './components/user-workspace-info/user-workspace-info.element.js';
import './components/user-workspace-avatar/user-workspace-avatar.element.js';
import './components/user-workspace-client-credentials/user-workspace-client-credentials.element.js';

@customElement('umb-user-workspace-editor')
export class UmbUserWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _user?: UmbUserDetailModel;

	#workspaceContext?: UmbUserWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeUser();
		});
	}

	#observeUser() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (user) => (this._user = user), 'umbUserObserver');
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.updateProperty('name', target.value);
			}
		}
	}

	override render() {
		if (!this._user) return html`User not found`;

		return html`
			<umb-workspace-editor alias="Umb.Workspace.User" class="uui-text" back-path="section/user-management">
				${this.#renderHeader()}
				<div id="main">
					<div id="left-column">${this.#renderLeftColumn()}</div>
					<div id="right-column">${this.#renderRightColumn()}</div>
				</div>
			</umb-workspace-editor>
		`;
	}

	#renderHeader() {
		return html`
			<div id="header" slot="header">
				<uui-input id="name" .value=${this._user?.name ?? ''} @input="${this.#onNameChange}" ${umbFocus()}></uui-input>
			</div>
			<umb-workspace-entity-action-menu slot="action-menu"></umb-workspace-entity-action-menu>
		`;
	}

	#renderLeftColumn() {
		if (!this._user) return nothing;

		return html`
			<umb-stack>
				<umb-user-workspace-profile-settings></umb-user-workspace-profile-settings>
				<umb-user-workspace-assign-access></umb-user-workspace-assign-access>
				<umb-user-workspace-access></umb-user-workspace-access>
			</umb-stack>
		`;
	}

	#renderRightColumn() {
		if (!this._user) return nothing;

		return html`
			<umb-stack look="compact">
				<umb-user-workspace-avatar></umb-user-workspace-avatar>
				<umb-user-workspace-info></umb-user-workspace-info>
				<umb-user-workspace-client-credentials></umb-user-workspace-client-credentials>
			</umb-stack>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#header {
				width: 100%;
				display: grid;
			}

			#main {
				display: grid;
				grid-template-columns: 1fr 350px;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
			}

			#left-column {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
		`,
	];
}

export default UmbUserWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-editor': UmbUserWorkspaceEditorElement;
	}
}
