import { getDisplayStateFromUserStatus } from '../../utils.js';
import { type UmbUserDetail } from '../index.js';
import { UmbUserWorkspaceContext } from './user-workspace.context.js';
import { UmbUserRepository } from 'src/packages/user/user/index.js';
import { UUIInputElement, UUIInputEvent, UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	html,
	nothing,
	TemplateResult,
	customElement,
	state,
	ifDefined,
	repeat,
} from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_CHANGE_PASSWORD_MODAL, type UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_AUTH, type UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { type UmbUserGroupInputElement } from 'src/packages/user/user-group/index.js';

@customElement('umb-user-workspace-editor')
export class UmbUserWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbLoggedInUser;

	@state()
	private _user?: UmbUserDetail;

	@state()
	private languages: Array<{ name: string; value: string; selected: boolean }> = [];

	#auth?: typeof UMB_AUTH.TYPE;
	#modalContext?: UmbModalManagerContext;
	#workspaceContext?: UmbUserWorkspaceContext;

	#userRepository = new UmbUserRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_AUTH, (instance) => {
			this.#auth = instance;
			this.#observeCurrentUser();
		});

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext as UmbUserWorkspaceContext;
			this.#observeUser();
		});
	}

	#observeUser() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (user) => (this._user = user));
	}

	#observeCurrentUser() {
		if (!this.#auth) return;
		this.observe(this.#auth.currentUser, async (currentUser) => {
			this._currentUser = currentUser;

			if (!currentUser) {
				return;
			}

			// Find all translations and make a unique list of iso codes
			const translations = await firstValueFrom(umbExtensionsRegistry.extensionsOfType('localization'));

			this.languages = translations
				.filter((isoCode) => isoCode !== undefined)
				.map((translation) => ({
					value: translation.meta.culture.toLowerCase(),
					name: translation.name,
					selected: false,
				}));

			const currentUserLanguageCode = currentUser.languageIsoCode?.toLowerCase();

			// Set the current user's language as selected
			const currentUserLanguage = this.languages.find((language) => language.value === currentUserLanguageCode);

			if (currentUserLanguage) {
				currentUserLanguage.selected = true;
			} else {
				// If users language code did not fit any of the options. We will create an option that fits, named unknown.
				// In this way the user can keep their choice though a given language was not present at this time.
				this.languages.push({
					value: currentUserLanguageCode ?? 'en-us',
					name: currentUserLanguageCode ? `${currentUserLanguageCode} (unknown)` : 'Unknown',
					selected: true,
				});
			}
		});
	}

	#onUserStatusChange() {
		if (!this._user || !this._user.id) return;

		if (this._user.state === UserStateModel.ACTIVE || this._user.state === UserStateModel.INACTIVE) {
			this.#userRepository?.disable([this._user.id]);
		}

		if (this._user.state === UserStateModel.DISABLED) {
			this.#userRepository?.enable([this._user.id]);
		}
	}

	#onUserGroupsChange(userGroupIds: Array<string>) {
		this.#workspaceContext?.updateProperty('userGroupIds', userGroupIds);
	}

	#onUserDelete() {
		if (!this._user || !this._user.id) return;

		this.#userRepository?.delete(this._user.id);
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

	#onLanguageChange(event: Event) {
		const target = event.composedPath()[0] as UUISelectElement;

		if (typeof target?.value === 'string') {
			this.#workspaceContext?.updateProperty('languageIsoCode', target.value);
		}
	}

	#onPasswordChange() {
		// TODO: check if current user is admin
		this.#modalContext?.open(UMB_CHANGE_PASSWORD_MODAL, {
			requireOldPassword: false,
		});
	}

	render() {
		if (!this._user) return html`User not found`;

		return html`
			<umb-workspace-editor alias="Umb.Workspace.User" class="uui-text">
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
				<a href="/section/users">
					<uui-icon name="umb:arrow-left"></uui-icon>
				</a>
				<uui-input id="name" .value=${this._user?.name ?? ''} @input="${this.#onNameChange}"></uui-input>
			</div>
		`;
	}

	#renderLeftColumn() {
		if (!this._user) return nothing;

		return html` <uui-box>
				<div slot="headline"><umb-localize key="user_profile">Profile</umb-localize></div>
				<umb-workspace-property-layout label="${this.localize.term('general_email')}">
					<uui-input
						slot="editor"
						name="email"
						label="${this.localize.term('general_email')}"
						readonly
						value=${ifDefined(this._user.email)}></uui-input>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="${this.localize.term('user_language')}"
					description=${this.localize.term('user_languageHelp')}>
					<uui-select
						slot="editor"
						name="language"
						label="${this.localize.term('user_language')}"
						.options=${this.languages}
						@change="${this.#onLanguageChange}">
					</uui-select>
				</umb-workspace-property-layout>
			</uui-box>
			<uui-box>
				<div slot="headline"><umb-localize key="user_assignAccess">Assign Access</umb-localize></div>
				<div id="assign-access">
					<umb-workspace-property-layout
						label="${this.localize.term('general_groups')}"
						description="${this.localize.term('user_groupsHelp')}">
						<umb-user-group-input
							slot="editor"
							.selectedIds=${this._user.userGroupIds ?? []}
							@change=${(e: Event) =>
								this.#onUserGroupsChange((e.target as UmbUserGroupInputElement).selectedIds)}></umb-user-group-input>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout
						label=${this.localize.term('user_startnodes')}
						description=${this.localize.term('user_startnodeshelp')}>
						<umb-property-editor-ui-document-picker
							.value=${this._user.contentStartNodeIds ?? []}
							@property-value-change=${(e: any) =>
								this.#workspaceContext?.updateProperty('contentStartNodeIds', e.target.value)}
							slot="editor"></umb-property-editor-ui-document-picker>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout
						label=${this.localize.term('user_mediastartnodes')}
						description=${this.localize.term('user_mediastartnodeshelp')}>
						<b slot="editor">NEED MEDIA PICKER</b>
					</umb-workspace-property-layout>
				</div>
			</uui-box>
			<uui-box headline=${this.localize.term('user_access')}>
				<div slot="header" class="faded-text">
					<umb-localize key="user_accessHelp"
						>Based on the assigned groups and start nodes, the user has access to the following nodes</umb-localize
					>
				</div>

				<b><umb-localize key="sections_content">Content</umb-localize></b>
				${this.#renderContentStartNodes()}
				<hr />
				<b><umb-localize key="sections_media">Media</umb-localize></b>
				<uui-ref-node name="Media Root">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			</uui-box>`;
	}

	#renderRightColumn() {
		if (!this._user || !this.#workspaceContext) return nothing;

		const displayState = getDisplayStateFromUserStatus(this._user.state);

		return html` <uui-box>
			<div id="user-info">
				<uui-avatar .name=${this._user?.name || ''}></uui-avatar>
				<uui-button label=${this.localize.term('user_changePhoto')}></uui-button>
				<hr />
				${this.#renderActionButtons()}

				<div>
					<b><umb-localize key="general_status">Status</umb-localize>:</b>
					<uui-tag look="${ifDefined(displayState?.look)}" color="${ifDefined(displayState?.color)}">
						${this.localize.term('user_' + displayState.key)}
					</uui-tag>
				</div>

				${this._user?.state === UserStateModel.INVITED
					? html`
							<uui-textarea placeholder=${this.localize.term('placeholders_enterMessage')}></uui-textarea>
							<uui-button look="primary" label=${this.localize.term('actions_resendInvite')}></uui-button>
					  `
					: nothing}
				${this.#renderInfoItem(
					'user_lastLogin',
					this.localize.date(this._user.lastLoginDate!) ||
						`${this._user.name + ' ' + this.localize.term('user_noLogin')} `,
				)}
				${this.#renderInfoItem('user_failedPasswordAttempts', this._user.failedLoginAttempts)}
				${this.#renderInfoItem(
					'user_lastLockoutDate',
					this._user.lastLockoutDate || `${this._user.name + ' ' + this.localize.term('user_noLockouts')}`,
				)}
				${this.#renderInfoItem(
					'user_lastPasswordChangeDate',
					this._user.lastLoginDate || `${this._user.name + ' ' + this.localize.term('user_noPasswordChange')}`,
				)}
				${this.#renderInfoItem('user_createDate', this.localize.date(this._user.createDate!))}
				${this.#renderInfoItem('user_updateDate', this.localize.date(this._user.updateDate!))}
				${this.#renderInfoItem('general_id', this._user.id)}
			</div>
		</uui-box>`;
	}

	#renderInfoItem(labelkey: string, value?: string | number) {
		return html`
			<div>
				<b><umb-localize key=${labelkey}></umb-localize></b>
				<span>${value}</span>
			</div>
		`;
	}

	#renderActionButtons() {
		if (!this._user) return nothing;

		//TODO: Find out if the current user is an admin. If not, show no buttons.
		// if (this._currentUserStore?.isAdmin === false) return nothing;

		const buttons: TemplateResult[] = [];

		if (this._user.id !== this._currentUser?.id) {
			if (this._user.state === UserStateModel.DISABLED) {
				buttons.push(html`
					<uui-button
						@click=${this.#onUserStatusChange}
						look="secondary"
						color="positive"
						label=${this.localize.term('actions_enable')}></uui-button>
				`);
			}

			if (this._user.state === UserStateModel.ACTIVE || this._user.state === UserStateModel.INACTIVE) {
				buttons.push(html`
					<uui-button
						@click=${this.#onUserStatusChange}
						look="secondary"
						color="warning"
						label=${this.localize.term('actions_disable')}></uui-button>
				`);
			}
		}

		if (this._currentUser?.id !== this._user?.id) {
			const button = html`
				<uui-button
					@click=${this.#onUserDelete}
					look="secondary"
					color="danger"
					label=${this.localize.term('user_deleteUser')}></uui-button>
			`;

			buttons.push(button);
		}

		buttons.push(
			html`<uui-button
				@click=${this.#onPasswordChange}
				look="secondary"
				label=${this.localize.term('general_changePassword')}></uui-button>`,
		);

		return buttons;
	}

	#renderContentStartNodes() {
		if (!this._user || !this._user.contentStartNodeIds) return;

		if (this._user.contentStartNodeIds.length < 1)
			return html`
				<uui-ref-node name="Content Root">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			`;

		//TODO Render the name of the content start node instead of it's id.
		return repeat(
			this._user.contentStartNodeIds,
			(node) => node,
			(node) => {
				return html`
					<uui-ref-node name=${node}>
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#header {
				width: 100%;
				display: grid;
				grid-template-columns: var(--uui-size-layout-1) 1fr;
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
			#right-column > uui-box > div {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			uui-avatar {
				font-size: var(--uui-size-16);
				place-self: center;
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
			.faded-text {
				color: var(--uui-color-text-alt);
				font-size: 0.8rem;
			}
			uui-tag {
				width: fit-content;
			}
			#user-info {
				display: flex;
				gap: var(--uui-size-space-6);
			}
			#user-info > div {
				display: flex;
				flex-direction: column;
			}
			#assign-access {
				display: flex;
				flex-direction: column;
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
