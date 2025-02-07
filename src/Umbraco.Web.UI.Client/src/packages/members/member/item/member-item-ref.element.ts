import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { UmbMemberItemModel } from '../repository/index.js';
import { customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-member-item-ref')
export class UmbMemberItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbMemberItemModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	_editPath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_MEMBER_ENTITY_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_MEMBER_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref(item: UmbMemberItemModel) {
		return `${this._editPath}/edit/${item.unique}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node-member
				id=${this.item.unique}
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly}
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
