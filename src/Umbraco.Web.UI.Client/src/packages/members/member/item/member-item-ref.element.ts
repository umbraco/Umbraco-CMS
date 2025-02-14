import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from '../../section/constants.js';
import type { UmbMemberItemModel } from './repository/types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-member-item-ref')
export class UmbMemberItemRefElement extends UmbLitElement {
	#item?: UmbMemberItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbMemberItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbMemberItemModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (!this.#item) {
			this.#modalRoute?.destroy();
			return;
		}
		if (oldValue?.unique === this.#item.unique) {
			return;
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
					match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._userHasSectionAccess = permitted;
				},
			},
		]);

		this.#modalRoute = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.onSetup(() => {
				return { data: { entityType: UMB_MEMBER_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref(item: UmbMemberItemModel) {
		if (!this._editPath) return;
		return `${this._editPath}/edit/${item.unique}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node-member
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node-member>
		`;
	}

	#renderIcon(item: UmbMemberItemModel) {
		if (!item.memberType.icon) return;
		return html`<umb-icon slot="icon" name=${item.memberType.icon}></umb-icon>`;
	}
}

export { UmbMemberItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-item-ref': UmbMemberItemRefElement;
	}
}
