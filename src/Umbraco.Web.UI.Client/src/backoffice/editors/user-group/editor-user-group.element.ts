import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

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
				gap: var(--uui-size-space-4);
			}
			.access-content {
				margin-top: var(--uui-size-space-1);
				margin-bottom: var(--uui-size-space-4);
				display: flex;
				align-items: center;
				line-height: 1;
				gap: var(--uui-size-space-3);
			}
			.access-content > span {
				align-self: end;
			}
		`,
	];

	@state()
	private _userName = '';

	@property({ type: String })
	entityKey = '';

	private _languages = []; //TODO Add languages

	private renderLeftColumn() {
		return html` <uui-box>
				<div slot="headline">Profile</div>
				<uui-form-layout-item style="margin-top: 0">
					<uui-label for="email">Email</uui-label>
					<uui-input name="email" label="email" readonly value=${''}></uui-input>
				</uui-form-layout-item>
				<uui-form-layout-item style="margin-bottom: 0">
					<uui-label for="language">Language</uui-label>
					<uui-select name="language" label="language" .options=${this._languages}> </uui-select>
				</uui-form-layout-item>
			</uui-box>
			<uui-box>
				<div id="assign-access">
					<div slot="headline">Assign access</div>
					<div>
						<b>Groups</b>
						<div class="faded-text">Add groups to assign access and permissions</div>
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
				</div>
			</uui-box>
			<uui-box>
				<div slot="headline">Access</div>
				<div slot="header" class="faded-text">
					Based on the assigned groups and start nodes, the user has access to the following nodes
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
			</uui-box>`;
	}

	private renderRightColumn() {
		return html` <uui-box> RIGHT </uui-box>`;
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
				<uui-input id="name" slot="name" .value=${this._userName} @input="${this._handleInput}"></uui-input>
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
