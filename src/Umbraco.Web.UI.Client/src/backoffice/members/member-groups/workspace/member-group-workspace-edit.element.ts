import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceMemberGroupContext } from './member-group-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MemberGroupDetails } from '@umbraco-cms/backoffice/models';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

/**
 * @element umb-member-group-edit-workspace
 * @description - Element for displaying a Member Group Workspace
 */
@customElement('umb-member-group-workspace-edit')
export class UmbMemberGroupWorkspaceEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}
		`,
	];

	#workspaceContext?: UmbWorkspaceMemberGroupContext;

	@state()
	private _memberGroup?: MemberGroupDetails;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbWorkspaceMemberGroupContext;
			this.#observeMemberGroup();
		});
	}

	#observeMemberGroup() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (data) => (this._memberGroup = data));
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`<umb-workspace-layout alias="Umb.Workspace.MemberGroup">
			<div id="header" slot="header">
				<uui-input id="name" .value=${this._memberGroup?.name} @input="${this.#handleInput}"> </uui-input>
			</div>
		</umb-workspace-layout> `;
	}
}

export default UmbMemberGroupWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace-edit': UmbMemberGroupWorkspaceEditElement;
	}
}
