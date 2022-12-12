import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';

@customElement('umb-editor-user-group')
export class UmbEditorUserGroupElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#main {
				display: grid;
				grid-template-columns: 1fr 350px;
				gap: var(--uui-size-space-6);
				padding: var(--uui-size-space-6);
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
			.faded-text {
				color: var(--uui-color-text-alt);
				font-size: 0.8rem;
			}
			#default-permissions {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.default-permission {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-2);
			}
			.default-permission:not(:last-child) {
				border-bottom: 1px solid var(--uui-color-divider);
			}
			.permission-info {
				display: flex;
				flex-direction: column;
			}
		`,
	];

	@state()
	private _userName = '';

	@property({ type: String })
	entityKey = '';

	defaultPermissions: Array<{
		name: string;
		permissions: Array<{ name: string; description: string; value: boolean }>;
	}> = [
		{
			name: 'Administration',
			permissions: [
				{
					name: 'Culture and Hostnames',
					description: 'Allow access to assign culture and hostnames',
					value: false,
				},
				{
					name: 'Restrict Public Access',
					description: 'Allow access to set and change access restrictions for a node',
					value: false,
				},
				{
					name: 'Rollback',
					description: 'Allow access to roll back a node to a previous state',
					value: false,
				},
			],
		},
		{
			name: 'Content',
			permissions: [
				{
					name: 'Browse Node',
					description: 'Allow access to view a node',
					value: false,
				},
				{
					name: 'Create Content Template',
					description: 'Allow access to create a Content Template',
					value: false,
				},
				{
					name: 'Delete',
					description: 'Allow access to delete nodes',
					value: false,
				},
				{
					name: 'Create',
					description: 'Allow access to create nodes',
					value: false,
				},
				{
					name: 'Publish',
					description: 'Allow access to publish nodes',
					value: false,
				},
				{
					name: 'Permissions',
					description: 'Allow access to change permissions for a node',
					value: false,
				},
				{
					name: 'Send To Publish',
					description: 'Allow access to send a node for approval before publishing',
					value: false,
				},
				{
					name: 'Unpublish',
					description: 'Allow access to unpublish a node',
					value: false,
				},
				{
					name: 'Update',
					description: 'Allow access to save a node',
					value: false,
				},
				{
					name: 'Full restore',
					description: 'Allow the user to restore items',
					value: false,
				},
				{
					name: 'Partial restore',
					description: 'Allow the user to partial restore items',
					value: false,
				},
				{
					name: 'Queue for transfer',
					description: 'Allow the user to queue item(s)',
					value: false,
				},
			],
		},
		{
			name: 'Structure',
			permissions: [
				{
					name: 'Copy',
					description: 'Allow access to copy a node',
					value: false,
				},
				{
					name: 'Move',
					description: 'Allow access to move a node',
					value: false,
				},
				{
					name: 'Sort',
					description: 'Allow access to change the sort order for nodes',
					value: false,
				},
			],
		},
	];

	private renderLeftColumn() {
		return html` <uui-box>
				<div slot="headline">Assign access</div>
				<div>
					<b>Sections</b>
					<div class="faded-text">Add sections to give users access</div>
				</div>
				<div>
					<b>Content start nodes</b>
					<div class="faded-text">Limit the content tree to specific start nodes</div>
					<umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker>
				</div>
				<div>
					<b>Media start nodes</b>
					<div class="faded-text">Limit the media library to specific start nodes</div>
					<umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker>
				</div>

				<b>Content</b>
				<div class="access-content">
					<uui-icon name="folder"></uui-icon>
					<span>Content Root</span>
				</div>

				<b>Media</b>
				<div class="access-content">
					<uui-icon name="folder"></uui-icon>
					<span>Media Root</span>
				</div>
			</uui-box>

			<uui-box>
				<div slot="headline">Default Permissions</div>
				<div id="default-permissions">
					${repeat(
						this.defaultPermissions,
						(defaultPermission) => html`
							<div>
								<b>${defaultPermission.name}</b>
								${repeat(
									defaultPermission.permissions,
									(permission) => html`
										<div class="default-permission">
											<uui-toggle
												.checked=${permission.value}
												@change=${(e: Event) => {
													permission.value = (e.target as HTMLInputElement).checked;
												}}></uui-toggle>
											<div class="permission-info">
												<b>${permission.name}</b>
												<span class="faded-text">${permission.description}</span>
											</div>
										</div>
									`
								)}
							</div>
						`
					)}
				</div>
			</uui-box>

			<uui-box>
				<div slot="headline">Granular permissions</div>
			</uui-box>`;
	}

	private renderRightColumn() {
		return html`<uui-box>
			<div slot="headline">Users</div>
		</uui-box>`;
	}

	// TODO. find a way where we don't have to do this for all editors.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			console.log('input', target.value);
		}
	}

	render() {
		return html`
			<umb-editor-entity-layout alias="Umb.Editor.UserGroup">
				<uui-input id="name" slot="header" .value=${this._userName} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this.renderLeftColumn()}</div>
					<div id="right-column">${this.renderRightColumn()}</div>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbEditorUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-user-group': UmbEditorUserGroupElement;
	}
}
