import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceMemberGroupContext } from '../../member-group-workspace.context';
import type { MemberGroupDetails } from '@umbraco-cms/backoffice/models';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-workspace-view-member-group-info')
export class UmbWorkspaceViewMemberGroupInfoElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				margin: var(--uui-size-layout-1);
				gap: var(--uui-size-layout-1);
				justify-content: space-between;
			}

			uui-box {
				margin-bottom: var(--ui-size-layout-1);
			}

			uui-box:first-child {
				flex: 1 1 75%;
			}

			uui-box:last-child {
				min-width: 320px;
			}
		`,
	];

	@state()
	private _memberGroup?: MemberGroupDetails;

	#workspaceContext?: UmbWorkspaceMemberGroupContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbWorkspaceMemberGroupContext;
			this.#observeMemberGroup();
		});
	}

	#observeMemberGroup() {
		if (!this.#workspaceContext) return;

		this.observe(this.#workspaceContext.data, (memberGroup) => {
			if (!memberGroup) return;

			// TODO: handle if model is not of the type wanted.
			// TODO: Make method to identify wether data is of type MemberGroupDetails
			this._memberGroup = memberGroup as MemberGroupDetails;
		});
	}

	private _renderGeneralInfo() {
		return html`
			<uui-box headline="General">
				<umb-workspace-property-layout label="Key" orientation="vertical">
					<div slot="editor">${this._memberGroup?.key}</div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}

	private _renderMemberGroupInfo() {
		return html`
			<uui-box headline="Member Group">
				<umb-empty-state size="small">Member groups have no additional properties for editing.</umb-empty-state>
			</uui-box>
		`;
	}

	render() {
		return html` ${this._renderMemberGroupInfo()}${this._renderGeneralInfo()} `;
	}
}

export default UmbWorkspaceViewMemberGroupInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-member-group-info': UmbWorkspaceViewMemberGroupInfoElement;
	}
}
