import { UMB_USER_GROUP_ROOT_WORKSPACE_PATH } from '../../paths.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from './user-group-workspace.context-token.js';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-group-workspace-editor')
export class UmbUserGroupWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean = false;

	@state()
	private _name?: UmbUserGroupDetailModel['name'];

	@state()
	private _alias?: UmbUserGroupDetailModel['alias'];

	@state()
	private _aliasCanBeChanged?: UmbUserGroupDetailModel['aliasCanBeChanged'] = true;

	@state()
	private _icon?: UmbUserGroupDetailModel['icon'];

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeUserGroup();
		});
	}

	#observeUserGroup() {
		this.observe(this.#workspaceContext?.isNew, (value) => (this._isNew = value), '_observeIsNew');
		this.observe(this.#workspaceContext?.name, (value) => (this._name = value), '_observeName');
		this.observe(this.#workspaceContext?.alias, (value) => (this._alias = value), '_observeAlias');
		this.observe(
			this.#workspaceContext?.aliasCanBeChanged,
			(value) => (this._aliasCanBeChanged = value),
			'_observeAliasCanBeChanged',
		);
		this.observe(this.#workspaceContext?.icon, (value) => (this._icon = value), '_observeIcon');
	}

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#workspaceContext?.updateProperty('name', event.target.value ?? '');
		this.#workspaceContext?.updateProperty('alias', event.target.alias ?? '');
	}

	async #onIconClick() {
		const [alias, color] = this._icon?.replace('color-', '')?.split(' ') ?? [];
		const result = await umbOpenModal(this, UMB_ICON_PICKER_MODAL, {
			value: {
				icon: alias,
				color: color,
			},
		}).catch(() => undefined);

		if (!result) return;

		if (result.icon && result.color) {
			this.#workspaceContext?.updateProperty('icon', `${result.icon} color-${result.color}`);
		} else if (result.icon) {
			this.#workspaceContext?.updateProperty('icon', result.icon);
		}
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor .backPath=${UMB_USER_GROUP_ROOT_WORKSPACE_PATH}>
				${this.#renderHeader()}
			</umb-entity-detail-workspace-editor>
		`;
	}

	#renderHeader() {
		return html`
			<div id="header" slot="header">
				<uui-button id="icon" compact label="icon" look="outline" @click=${this.#onIconClick}>
					<umb-icon name=${this._icon || ''}></umb-icon>
				</uui-button>

				<umb-input-with-alias
					id="name"
					label=${this.localize.term('placeholders_entername')}
					.value=${this._name}
					alias=${ifDefined(this._alias)}
					?auto-generate-alias=${this._isNew}
					?alias-readonly=${this._aliasCanBeChanged === false}
					@change=${this.#onNameAndAliasChange}
					${umbFocus()}>
				</umb-input-with-alias>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
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
				align-items: center;
			}

			#icon {
				font-size: var(--uui-size-5);
				height: 30px;
				width: 30px;
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}
		`,
	];
}

export default UmbUserGroupWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace-editor': UmbUserGroupWorkspaceEditorElement;
	}
}
