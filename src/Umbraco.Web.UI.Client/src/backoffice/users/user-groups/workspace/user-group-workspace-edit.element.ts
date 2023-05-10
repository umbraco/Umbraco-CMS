import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import type { UserGroupDetails } from '../types';
import { UmbUserGroupWorkspaceContext } from './user-group-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../../users/components/user-input/user-input.element';
import '../../../shared/components/input-section/input-section.element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-user-group-workspace-edit')
export class UmbUserGroupWorkspaceEditElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupDetails;

	@state()
	private _userKeys?: Array<string>;

	#workspaceContext?: UmbUserGroupWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbUserGroupWorkspaceContext;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup as any));
		});
	}

	#onUsersChange(userKeys: Array<string>) {
		this._userKeys = userKeys;
		// TODO: make a method on the UmbWorkspaceUserGroupContext:
		//this._workspaceContext.setUsers();
	}

	#updateSections(value: string[]) {
		this.#workspaceContext?.updateProperty('sections', value);
	}

	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.updateProperty('name', target.value);
			}
		}
	}

	render() {
		if (!this._userGroup) return nothing;

		return html`
			<umb-workspace-editor alias="Umb.Workspace.UserGroup">
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
				<a href="/section/users/view/user-groups">
					<uui-icon name="umb:arrow-left"></uui-icon>
				</a>
				<uui-input id="name" .value=${this._userGroup?.name ?? ''} @input="${this.#onNameChange}"></uui-input>
			</div>
		`;
	}

	#renderLeftColumn() {
		if (!this._userGroup) return nothing;

		return html` <uui-box>
				<div slot="headline">Assign access</div>
				<umb-workspace-property-layout label="Sections" description="Add sections to give users access">
					<umb-input-section
						slot="editor"
						.value=${this._userGroup.sections}
						@change=${(e: any) => this.#updateSections(e.target.value)}></umb-input-section>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Content start node"
					description="Limit the content tree to a specific start node">
					<uui-ref-node slot="editor" name="Content Root" border>
						<uui-icon slot="icon" name="folder"></uui-icon>
						<uui-button slot="actions" label="change"></uui-button>
						<uui-button slot="actions" label="remove" color="danger"></uui-button>
					</uui-ref-node>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Media start node"
					description="Limit the media library to a specific start node">
					<uui-ref-node slot="editor" name="Media Root" border>
						<uui-icon slot="icon" name="folder"></uui-icon>
						<uui-button slot="actions" label="change"></uui-button>
						<uui-button slot="actions" label="remove" color="danger"></uui-button>
					</uui-ref-node>
				</umb-workspace-property-layout>
			</uui-box>

			<uui-box>
				<div slot="headline">Default Permissions</div>
				<b>PERMISSIONS NOT IMPLEMENTED YET</b>
			</uui-box>

			<uui-box>
				<div slot="headline">Granular permissions</div>
				<b>PERMISSIONS NOT IMPLEMENTED YET</b>
			</uui-box>`;
	}

	#renderRightColumn() {
		return html`<uui-box>
			<div slot="headline">Users</div>
			<umb-input-user
				@change=${(e: Event) => this.#onUsersChange((e.target as any).value)}
				.value=${this._userKeys || []}></umb-input-user>
		</uui-box>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
			#header {
				width: 100%;
				display: grid;
				grid-template-columns: var(--uui-size-layout-1) 1fr;
				padding: var(--uui-size-layout-1);
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
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbUserGroupWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace-edit': UmbUserGroupWorkspaceEditElement;
	}
}
