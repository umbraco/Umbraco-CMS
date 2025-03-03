import { UMB_MEMBER_TYPE_WORKSPACE_CONTEXT } from './member-type-workspace.context-token.js';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUITextareaElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';

@customElement('umb-member-type-workspace-editor')
export class UmbMemberTypeWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string;

	@state()
	private _description?: string;

	@state()
	private _alias?: string;

	@state()
	private _icon?: string;

	@state()
	private _isNew?: boolean;

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
		this.observe(
			this.#workspaceContext.description,
			(description) => (this._description = description),
			'_observeDescription',
		);
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

	#onDescriptionChange(event: InputEvent & { target: UUITextareaElement }) {
		this.#workspaceContext?.setDescription(event.target.value.toString() ?? '');
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<div id="header" slot="header">
					<uui-button id="icon" compact label="icon" look="outline" @click=${this._handleIconClick}>
						<umb-icon name=${ifDefined(this._icon)}></umb-icon>
					</uui-button>

					<div id="editors">
						<umb-input-with-alias
							id="name"
							label=${this.localize.term('placeholders_entername')}
							value=${this._name}
							alias=${this._alias}
							?auto-generate-alias=${this._isNew}
							@change=${this.#onNameAndAliasChange}
							${umbFocus()}>
						</umb-input-with-alias>

						<uui-input
							id="description"
							.label=${this.localize.term('placeholders_enterDescription')}
							.value=${this._description}
							.placeholder=${this.localize.term('placeholders_enterDescription')}
							@input=${this.#onDescriptionChange}></uui-input>
					</div>
				</div>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				flex: 1 1 auto;
				gap: var(--uui-size-space-2);
			}

			#editors {
				display: flex;
				flex: 1 1 auto;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}

			#description {
				width: 100%;
				--uui-input-height: var(--uui-size-8);
				--uui-input-border-color: transparent;
			}

			#description:hover {
				--uui-input-border-color: var(--uui-color-border);
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
				font-size: var(--uui-size-8);
				height: 60px;
				width: 60px;
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
