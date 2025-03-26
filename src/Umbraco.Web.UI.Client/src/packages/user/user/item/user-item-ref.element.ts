import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import type { UmbUserItemModel } from '../repository/index.js';
import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from '../../section/constants.js';
import { UMB_EDIT_USER_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-user-item-ref')
export class UmbUserItemRefElement extends UmbLitElement {
	#item?: UmbUserItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbUserItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbUserItemModel | undefined) {
		this.#item = value;
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	@state()
	_userHasSectionAccess = false;

	constructor() {
		super();

		createExtensionApiByAlias(this, UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					match: UMB_USER_MANAGEMENT_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._userHasSectionAccess = permitted;
				},
			},
		]);

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.onSetup(() => {
				return { data: { entityType: UMB_USER_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref(item: UmbUserItemModel) {
		if (!this._editPath) return;
		const path = UMB_EDIT_USER_WORKSPACE_PATH_PATTERN.generateLocal({ unique: item.unique });
		return `${this._editPath}/${path}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node-user
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}>
				<umb-user-avatar
					slot="icon"
					.name=${this.item.name}
					.kind=${this.item.kind}
					.imgUrls=${this.item.avatarUrls}></umb-user-avatar>
				<slot name="actions" slot="actions"></slot>
			</uui-ref-node-user>
		`;
	}

	static override styles = [
		css`
			umb-user-avatar {
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { UmbUserItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-item-ref': UmbUserItemRefElement;
	}
}
