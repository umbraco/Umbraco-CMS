import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import type { UmbUserItemModel } from '../repository/index.js';
import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from '../../section/constants.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
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
		const oldValue = this.#item;
		this.#item = value;

		if (!this.#item) {
			this.#modalRoute?.destroy();
			return;
		}

		if (oldValue?.unique !== this.#item.unique) {
			this.#modalRoute = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
				.addAdditionalPath(UMB_USER_ENTITY_TYPE + '/' + this.#item.unique)
				.onSetup(() => {
					return { data: { entityType: UMB_USER_ENTITY_TYPE, preset: {} } };
				})
				.observeRouteBuilder((routeBuilder) => {
					this._editPath = routeBuilder({});
				});
		}
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	@state()
	_userHasSectionAccess = false;

	#modalRoute?: any;

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
	}

	#getHref(item: UmbUserItemModel) {
		return `${this._editPath}/edit/${item.unique}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node-user
				name=${this.item.name}
				href=${this.#getHref(this.item)}
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
