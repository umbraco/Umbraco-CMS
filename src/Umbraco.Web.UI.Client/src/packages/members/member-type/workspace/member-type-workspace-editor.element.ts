import { UMB_MEMBER_TYPE_WORKSPACE_CONTEXT } from './member-type-workspace.context-token.js';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ICON_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('umb-member-type-workspace-editor')
export class UmbMemberTypeWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string;

	@state()
	private _alias?: string;

	@state()
	private _icon?: string;

	@state()
	private _iconColorAlias?: string;
	// TODO: Color should be using an alias, and look up in some dictionary/key/value) of project-colors.

	#workspaceContext?: typeof UMB_MEMBER_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeMemberType();
		});
	}

	#observeMemberType() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name), '_observeName');
		this.observe(this.#workspaceContext.alias, (alias) => (this._alias = alias), '_observeAlias');
		this.observe(this.#workspaceContext.icon, (icon) => (this._icon = icon), '_observeIcon');

		this.observe(
			this.#workspaceContext.isNew,
			(isNew) => {
				if (isNew) {
					// TODO: Would be good with a more general way to bring focus to the name input.
					(this.shadowRoot?.querySelector('#name') as HTMLElement)?.focus();
				}
				this.removeUmbControllerByAlias('_observeIsNew');
			},
			'_observeIsNew',
		);
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
				this.#workspaceContext?.set('icon', `${saved.icon} color-${saved.color}`);
			} else if (saved.icon) {
				this.#workspaceContext?.set('icon', saved.icon);
			}
		});
	}

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#workspaceContext?.setName(event.target.value ?? '');
		this.#workspaceContext?.setAlias(event.target.alias ?? '');
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.MemberType">
				<div id="header" slot="header">
					<uui-button id="icon" @click=${this._handleIconClick} label="icon" compact>
						<uui-icon name="${ifDefined(this._icon)}" style="color: ${this._iconColorAlias}"></uui-icon>
					</uui-button>

					<umb-input-with-alias
						id="name"
						label="name"
						value=${this._name}
						alias=${this._alias}
						@change="${this.#onNameAndAliasChange}"
						${umbFocus()}></umb-input-with-alias>
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
				flex: 1 1 auto;
				align-items: center;
			}

			#alias-lock {
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
			#alias-lock uui-icon {
				margin-bottom: 2px;
			}

			#icon {
				font-size: calc(var(--uui-size-layout-3) / 2);
				margin-right: var(--uui-size-space-2);
				margin-left: calc(var(--uui-size-space-4) * -1);
			}
		`,
	];
}

export default UmbMemberTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace-editor': UmbMemberTypeWorkspaceEditorElement;
	}
}
