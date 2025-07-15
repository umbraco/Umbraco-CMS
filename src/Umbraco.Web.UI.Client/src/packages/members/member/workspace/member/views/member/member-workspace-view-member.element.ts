import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../member-workspace.context-token.js';
import type { UmbMemberDetailModel } from '../../../../types.js';
import { TimeFormatOptions } from './utils.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

import './member-workspace-view-member-info.element.js';
import type { UmbInputMemberGroupElement } from '@umbraco-cms/backoffice/member-group';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

@customElement('umb-member-workspace-view-member')
export class UmbMemberWorkspaceViewMemberElement extends UmbLitElement implements UmbWorkspaceViewElement {
	private _workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;

			this.observe(this._workspaceContext?.isNew, (isNew) => {
				this._isNew = !!isNew;
			});
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context?.hasAccessToSensitiveData, (hasAccessToSensitiveData) => {
				this._hasAccessToSensitiveData = hasAccessToSensitiveData === true;
			});
		});
	}

	@state()
	private _showChangePasswordForm = false;

	@state()
	private _newPasswordError = '';

	@state()
	private _isNew = true;

	@state()
	private _hasAccessToSensitiveData = false;

	#onChange(propertyName: keyof UmbMemberDetailModel, value: UmbMemberDetailModel[keyof UmbMemberDetailModel]) {
		if (!this._workspaceContext) return;

		this._workspaceContext.set(propertyName, value);
	}

	#onGroupsUpdated(event: CustomEvent) {
		const uniques = (event.target as UmbInputMemberGroupElement).selection;

		this._workspaceContext?.set('groups', uniques);
	}

	#onPasswordUpdate = () => {
		const newPassword = this.shadowRoot?.querySelector<HTMLInputElement>('uui-input[name="newPassword"]')?.value;
		const confirmPassword = this.shadowRoot?.querySelector<HTMLInputElement>(
			'uui-input[name="confirmPassword"]',
		)?.value;

		if (newPassword !== confirmPassword) {
			this._newPasswordError = 'Passwords do not match';
			return;
		}

		this._newPasswordError = '';

		this._workspaceContext?.set('newPassword', newPassword);
	};

	#onNewPasswordCancel = () => {
		this._workspaceContext?.set('newPassword', '');
		this._showChangePasswordForm = false;
		this._newPasswordError = '';
	};

	#renderPasswordInput() {
		if (this._isNew) {
			return html`
				<umb-property-layout label=${this.localize.term('user_password')} mandatory>
					<uui-input
						slot="editor"
						name="newPassword"
						label=${this.localize.term('user_passwordEnterNew')}
						type="password"
						@input=${() => this.#onPasswordUpdate()}></uui-input>
				</umb-property-layout>

				<umb-property-layout label="Confirm password" mandatory>
					<uui-input
						slot="editor"
						name="confirmPassword"
						label="Confirm password"
						type="password"
						@input=${() => this.#onPasswordUpdate()}></uui-input>
				</umb-property-layout>

				${when(this._newPasswordError, () => html`<p class="validation-error">${this._newPasswordError}</p>`)}
			`;
		}

		return html`
			<umb-property-layout label=${this.localize.term('general_changePassword')}>
				${when(
					this._showChangePasswordForm,
					() => html`
						<div slot="editor">
							<umb-property-layout label=${this.localize.term('user_newPassword')} mandatory>
								<uui-input
									slot="editor"
									name="newPassword"
									label=${this.localize.term('user_newPassword')}
									type="password"
									@input=${() => this.#onPasswordUpdate()}></uui-input>
							</umb-property-layout>
							<umb-property-layout label=${this.localize.term('user_confirmNewPassword')} mandatory>
								<uui-input
									slot="editor"
									name="confirmPassword"
									label=${this.localize.term('user_confirmNewPassword')}
									type="password"
									@input=${() => this.#onPasswordUpdate()}></uui-input>
							</umb-property-layout>
							${when(this._newPasswordError, () => html`<p class="validation-error">${this._newPasswordError}</p>`)}
							<uui-button
								label=${this.localize.term('general_cancel')}
								look="secondary"
								@click=${this.#onNewPasswordCancel}></uui-button>
						</div>
					`,
					() => html`
						<uui-button
							slot="editor"
							label=${this.localize.term('general_changePassword')}
							look="secondary"
							@click=${() => (this._showChangePasswordForm = true)}></uui-button>
					`,
				)}
			</umb-property-layout>
		`;
	}

	#renderLeftColumn() {
		if (!this._workspaceContext) return;

		return html`
			<div id="left-column">
				<uui-box>
					<umb-property-layout label=${this.localize.term('general_username')} mandatory>
						<uui-input
							slot="editor"
							name="login"
							label=${this.localize.term('general_username')}
							value=${this._workspaceContext.username}
							required
							required-message=${this.localize.term('user_loginnameRequired')}
							@input=${(e: Event) => this.#onChange('username', (e.target as HTMLInputElement).value)}></uui-input>
					</umb-property-layout>

					<umb-property-layout label=${this.localize.term('general_email')} mandatory>
						<uui-input
							slot="editor"
							name="email"
							label=${this.localize.term('general_email')}
							value=${this._workspaceContext.email}
							required
							required-message=${this.localize.term('user_emailRequired')}
							@input=${(e: Event) => this.#onChange('email', (e.target as HTMLInputElement).value)}></uui-input>
					</umb-property-layout>

					${this.#renderPasswordInput()}

					<umb-property-layout label=${this.localize.term('content_membergroup')}>
						<umb-input-member-group
							slot="editor"
							@change=${this.#onGroupsUpdated}
							.selection=${this._workspaceContext.memberGroups}></umb-input-member-group>
					</umb-property-layout>

					${when(
						this._hasAccessToSensitiveData,
						() => html`
							<umb-property-layout label=${this.localize.term('user_stateApproved')}>
								<uui-toggle
									slot="editor"
									.checked=${this._workspaceContext!.isApproved}
									@change=${(e: UUIBooleanInputEvent) => this.#onChange('isApproved', e.target.checked)}>
								</uui-toggle>
							</umb-property-layout>

							<umb-property-layout label=${this.localize.term('user_stateLockedOut')}>
								<uui-toggle
									slot="editor"
									?disabled=${this._isNew || !this._workspaceContext!.isLockedOut}
									.checked=${this._workspaceContext!.isLockedOut}
									@change=${(e: UUIBooleanInputEvent) => this.#onChange('isLockedOut', e.target.checked)}>
								</uui-toggle>
							</umb-property-layout>
						`,
					)}
					<umb-property-layout label=${this.localize.term('member_2fa')}>
						<uui-toggle
							slot="editor"
							?disabled=${this._isNew || !this._workspaceContext.isTwoFactorEnabled}
							.checked=${this._workspaceContext.isTwoFactorEnabled}
							@change=${(e: UUIBooleanInputEvent) => this.#onChange('isTwoFactorEnabled', e.target.checked)}>
						</uui-toggle>
					</umb-property-layout>
				</uui-box>

				<div class="container">
					<umb-extension-slot id="workspace-info-apps" type="workspaceInfoApp"></umb-extension-slot>
				</div>
			</div>
		`;
	}

	#renderRightColumn() {
		if (!this._workspaceContext) return;

		return html`
			<div id="right-column">
				<uui-box>
					<umb-stack look="compact">
						<div>
							<h4><umb-localize key="user_failedPasswordAttempts">Failed login attempts</umb-localize></h4>
							<span>${this._workspaceContext.failedPasswordAttempts}</span>
						</div>
						<div>
							<h4><umb-localize key="user_lastLockoutDate">Last lockout date</umb-localize></h4>
							<span>
								${this._workspaceContext.lastLockOutDate
									? this.localize.date(this._workspaceContext.lastLockOutDate, TimeFormatOptions)
									: this.localize.term('general_never')}
							</span>
						</div>
						<div>
							<h4><umb-localize key="user_lastLogin">Last login</umb-localize></h4>
							<span>
								${this._workspaceContext.lastLoginDate
									? this.localize.date(this._workspaceContext.lastLoginDate, TimeFormatOptions)
									: this.localize.term('general_never')}
							</span>
						</div>
						<div>
							<h4><umb-localize key="user_passwordChangedGeneric">Password changed</umb-localize></h4>
							<span>
								${this._workspaceContext.lastPasswordChangeDate
									? this.localize.date(this._workspaceContext.lastPasswordChangeDate, TimeFormatOptions)
									: this.localize.term('general_never')}
							</span>
						</div>
					</umb-stack>
				</uui-box>

				<uui-box>
					<umb-member-workspace-view-member-info></umb-member-workspace-view-member-info>
				</uui-box>
			</div>
		`;
	}

	override render() {
		return html`
			<umb-body-layout header-fit-height>
				<div id="main">${this.#renderLeftColumn()} ${this.#renderRightColumn()}</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
			}
			#main {
				display: flex;
				flex-wrap: wrap;
				gap: var(--uui-size-space-4);
			}
			#left-column {
				/* Is there a way to make the wrapped right column grow only when wrapped? */
				flex: 9999 1 500px;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#right-column {
				flex: 1 1 350px;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			uui-box {
				height: fit-content;
			}
			umb-property-layout {
				padding-block: var(--uui-size-space-4);
			}
			umb-property-layout:first-child {
				padding-top: 0;
			}
			umb-property-layout:last-child {
				padding-bottom: 0;
			}
			.validation-error {
				margin-top: 0;
				color: var(--uui-color-danger);
			}

			h4 {
				margin: 0;
			}
		`,
	];
}

export default UmbMemberWorkspaceViewMemberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-view-member': UmbMemberWorkspaceViewMemberElement;
	}
}
