import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from './document-type-workspace.context-token.js';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
@customElement('umb-document-type-workspace-editor')
export class UmbDocumentTypeWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string;

	@state()
	private _alias?: string;

	@state()
	private _icon?: string;

	#workspaceContext?: typeof UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeDocumentType();
		});
	}

	#observeDocumentType() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name), '_observeName');
		this.observe(this.#workspaceContext.alias, (alias) => (this._alias = alias), '_observeAlias');
		this.observe(this.#workspaceContext.icon, (icon) => (this._icon = icon), '_observeIcon');
	}

	private async _handleIconClick() {
		const [alias, color] = this._icon?.replace('color-', '')?.split(' ') ?? [];
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_ICON_PICKER_MODAL, {
			value: {
				icon: alias,
				color: color,
			},
		});

		modalContext?.onSubmit().then((saved) => {
			if (saved.icon && saved.color) {
				this.#workspaceContext?.setIcon(`${saved.icon} color-${saved.color}`);
			} else if (saved.icon) {
				this.#workspaceContext?.setIcon(saved.icon);
			}
		});
	}

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#workspaceContext?.setName(event.target.value ?? '');
		this.#workspaceContext?.setAlias(event.target.alias ?? '');
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<uui-button id="icon" @click=${this._handleIconClick} label="icon" compact>
						<umb-icon name=${ifDefined(this._icon)}></umb-icon>
					</uui-button>

					<umb-input-with-alias
						id="name"
						label="name"
						value=${this._name}
						alias=${this._alias}
						@change="${this.#onNameAndAliasChange}"></umb-input-with-alias>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				flex: 1 1 auto;
			}

			#name {
				width: 100%;
			}

			#icon {
				font-size: calc(var(--uui-size-layout-3) / 2);
				margin-right: var(--uui-size-space-2);
				margin-left: calc(var(--uui-size-space-4) * -1);
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
