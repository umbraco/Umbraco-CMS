import type { UmbMediaTypeWorkspaceContext } from './media-type-workspace.context.js';
import { UMB_MEDIA_TYPE_WORKSPACE_CONTEXT } from './media-type-workspace.context-token.js';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ICON_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('umb-media-type-workspace-editor')
export class UmbMediaTypeWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string;

	@state()
	private _alias?: string;

	@state()
	private _aliasLocked = true;

	@state()
	private _icon?: string;

	@state()
	private _isNew?: string;

	#workspaceContext?: UmbMediaTypeWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_TYPE_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeMediaType();
		});
	}

	#observeMediaType() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name), '_observeName');
		this.observe(this.#workspaceContext.alias, (alias) => (this._alias = alias), '_observeAlias');
		this.observe(this.#workspaceContext.icon, (icon) => (this._icon = icon), '_observeIcon');
		this.observe(this.#workspaceContext.isNew, (isNew) => (this._isNew = isNew), '_observeIsNew');
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
		return html`<umb-workspace-editor alias="Umb.Workspace.MediaType">
			<div id="header" slot="header">
				<uui-button id="icon" @click=${this._handleIconClick} label="icon" compact>
					<umb-icon name=${ifDefined(this._icon)}></umb-icon>
				</uui-button>

				<umb-input-with-alias
					id="name"
					label="name"
					value=${this._name}
					alias=${this._alias}
					?auto-generate-alias=${this._isNew}
					@change="${this.#onNameAndAliasChange}"
					${umbFocus()}></umb-input-with-alias>
			</div>
		</umb-workspace-editor>`;
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

export default UmbMediaTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-editor': UmbMediaTypeWorkspaceEditorElement;
	}
}
