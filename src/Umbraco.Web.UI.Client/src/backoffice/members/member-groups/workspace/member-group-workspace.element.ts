import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceEntityElement } from '../../../../backoffice/shared/components/workspace/workspace-entity-element.interface';
import { UmbWorkspaceMemberGroupContext } from './member-group-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-member-group-workspace
 * @description - Element for displaying a Member Group Workspace
 */
@customElement('umb-member-group-workspace')
export class UmbMemberGroupWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				/* TODO: can this be applied from layout slot CSS? */
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}
		`,
	];
	private _workspaceContext: UmbWorkspaceMemberGroupContext = new UmbWorkspaceMemberGroupContext(this);

	public load(entityKey: string) {
		this._workspaceContext.load(entityKey);
	}

	public create(parentKey: string | null) {
		this._workspaceContext.create(parentKey);
	}

	@state()
	private _memberGroupName = '';

	constructor() {
		super();

		this.observe(this._workspaceContext.data.pipe(distinctUntilChanged()), (memberGroup) => {
			if (memberGroup && memberGroup.name !== this._memberGroupName) {
				this._memberGroupName = memberGroup.name ?? '';
			}
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._workspaceContext.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.MemberGroup">
				<uui-input id="header" slot="header" .value=${this._memberGroupName} @input="${this._handleInput}"></uui-input>
			</umb-workspace-layout>
		`;
	}
}

export default UmbMemberGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace': UmbMemberGroupWorkspaceElement;
	}
}
