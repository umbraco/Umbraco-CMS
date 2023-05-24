import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { MemberGroupDetails } from '../types.js';
import { UmbWorkspaceMemberGroupContext } from './member-group-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
/**
 * @element umb-member-group-edit-workspace
 * @description - Element for displaying a Member Group Workspace
 */
@customElement('umb-member-group-workspace-edit')
export class UmbMemberGroupWorkspaceEditElement extends UmbLitElement {
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
		return html`<umb-workspace-editor alias="Umb.Workspace.MemberGroup">
			<div id="header" slot="header">
				<uui-input id="name" .value=${this._memberGroup?.name} @input="${this.#handleInput}"> </uui-input>
			</div>
		</umb-workspace-editor> `;
	}

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
}

export default UmbMemberGroupWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace-edit': UmbMemberGroupWorkspaceEditElement;
	}
}
