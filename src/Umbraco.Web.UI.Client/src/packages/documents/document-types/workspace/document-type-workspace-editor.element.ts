import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDocumentTypeWorkspaceContext } from './document-type-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
@customElement('umb-document-type-workspace-editor')
export class UmbDocumentTypeWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _icon?: string;

	@state()
	private _iconColorAlias?: string;
	// TODO: Color should be using an alias, and look up in some dictionary/key/value) of project-colors.

	#workspaceContext?: UmbDocumentTypeWorkspaceContext;

	@state()
	private _name?: string;

	@state()
	private _alias?: string;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbDocumentTypeWorkspaceContext;
			this.#observeDocumentType();
		});

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#observeDocumentType() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name));
		this.observe(this.#workspaceContext.alias, (alias) => (this._alias = alias));
		this.observe(this.#workspaceContext.icon, (icon) => (this._icon = icon));
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleNameInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleAliasInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setAlias(target.value);
			}
		}
		event.stopPropagation();
	}

	private async _handleIconClick() {
		const modalHandler = this._modalContext?.open(UMB_ICON_PICKER_MODAL, {
			icon: this._icon,
			color: this._iconColorAlias,
		});

		modalHandler?.onSubmit().then((saved) => {
			if (saved.icon) this.#workspaceContext?.setIcon(saved.icon);
			// TODO: save color ALIAS as well
		});
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<uui-button id="icon" @click=${this._handleIconClick} compact>
						<uui-icon name="${this._icon}" style="color: ${this._iconColorAlias}"></uui-icon>
					</uui-button>

					<uui-input id="name" .value=${this._name} @input="${this._handleNameInput}">
						<uui-input-lock id="alias" slot="append" .value=${this._alias} @input="${this._handleAliasInput}"></uui-input
						></uui-input-lock>
					</uui-input>
				</div>

				<div slot="footer">
					<!-- TODO: Shortcuts Modal? -->
					<uui-button label="Show keyboard shortcuts">
						Keyboard Shortcuts
						<uui-keyboard-shortcut>
							<uui-key>ALT</uui-key>
							+
							<uui-key>shift</uui-key>
							+
							<uui-key>k</uui-key>
						</uui-keyboard-shortcut>
					</uui-button>
				</div>
			</umb-workspace-editor>
		`;
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
				display: flex;
				flex: 1 1 auto;
				margin: 0 var(--uui-size-layout-1);
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}

			#alias {
				height: calc(100% - 2px);
				--uui-input-border-width: 0;
				--uui-button-height: calc(100% -2px);
			}

			#icon {
				font-size: calc(var(--uui-size-layout-3) / 2);
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-editor': UmbDocumentTypeWorkspaceEditorElement;
	}
}
