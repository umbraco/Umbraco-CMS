// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../member-workspace.context.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbMemberDetailModel } from '../../../types.js';
import { UUIBooleanInputEvent } from '@umbraco-ui/uui';

@customElement('umb-member-workspace-view-member')
export class UmbMemberWorkspaceViewMemberElement extends UmbLitElement implements UmbWorkspaceViewElement {
	private _workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			console.log('workspaceContext', !!this._workspaceContext.get('isApproved'));
		});
	}

	#onChange(propertyName: keyof UmbMemberDetailModel, value: UmbMemberDetailModel[keyof UmbMemberDetailModel]) {
		if (!this._workspaceContext) return;

		this._workspaceContext.set(propertyName, value);
	}

	render() {
		if (!this._workspaceContext) {
			return html`<div>Not found</div>`;
		}

		return html` <umb-body-layout header-fit-height>
			<uui-box>
				<umb-property-layout label="${this.localize.term('general_login')}">
					<uui-input slot="editor" name="login" label="${this.localize.term('general_login')}" value=""></uui-input>
				</umb-property-layout>

				<umb-property-layout label="${this.localize.term('general_email')}">
					<uui-input
						slot="editor"
						name="email"
						label="${this.localize.term('general_email')}"
						value=${this._workspaceContext.getEmail()}></uui-input>
				</umb-property-layout>

				<umb-property-layout label="enter your password">
					<uui-input slot="editor" name="login" label="enter your password" value=""></uui-input>
				</umb-property-layout>

				<umb-property-layout label="Member Group">
					<div slot="editor">MEMBER GROUP PICKER</div>
				</umb-property-layout>

				<umb-property-layout label="Failed login attempts">
					<div slot="editor">${this._workspaceContext.get('failedPasswordAttempts')}</div>
				</umb-property-layout>

				<umb-property-layout label="Approved">
					<uui-toggle
						slot="editor"
						.checked=${!!this._workspaceContext.get('isApproved')}
						@change=${(e: UUIBooleanInputEvent) => this.#onChange('isApproved', e.target.checked)}>
					</uui-toggle>
				</umb-property-layout>

				<umb-property-layout label="Locked out">
					<uui-toggle
						slot="editor"
						.checked=${!!this._workspaceContext.get('isLockedOut')}
						@change=${(e: UUIBooleanInputEvent) => this.#onChange('isLockedOut', e.target.checked)}>
					</uui-toggle>
				</umb-property-layout>

				<umb-property-layout label="Two-Factor authentication">
					<uui-toggle slot="editor"></uui-toggle>
				</umb-property-layout>

				<umb-property-layout label="Last logout date"> <div slot="editor">never</div> </umb-property-layout>

				<umb-property-layout label="Last login"> <div slot="editor">never</div> </umb-property-layout>

				<umb-property-layout label="Password changed"> <div slot="editor">never</div> </umb-property-layout>
			</uui-box>
		</umb-body-layout>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
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
