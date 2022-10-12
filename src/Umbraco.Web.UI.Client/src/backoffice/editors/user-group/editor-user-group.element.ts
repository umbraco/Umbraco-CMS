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
		`,
	];

	@state()
	private _userName = '';

	@property({ type: String })
	entityKey = '';

	private renderLeftColumn() {
		return html`<uui-box> LEFT </uui-box>`;
	}

	private renderRightColumn() {
		return html`<uui-box> <div>RIGHT</div> </uui-box>`;
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
