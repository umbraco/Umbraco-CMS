import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceMemberGroupContext } from '../../member-group-workspace.context';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

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
	_memberGroup?: MemberGroupDetails;

	private _workspaceContext?: UmbWorkspaceMemberGroupContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext<UmbWorkspaceMemberGroupContext>('umbWorkspaceContext', (memberGroupContext) => {
			this._workspaceContext = memberGroupContext;
			this._observeMemberGroup();
		});
	}

	private _observeMemberGroup() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.data.pipe(distinctUntilChanged()), (memberGroup) => {
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

	// TODO => should use umb-empty-state when it exists
	private _renderMemberGroupInfo() {
		return html`
			<uui-box headline="Member Group">
				<p>Member groups have no additional properties for editing.</p>
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
