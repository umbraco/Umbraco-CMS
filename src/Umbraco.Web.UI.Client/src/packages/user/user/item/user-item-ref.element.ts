import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import type { UmbUserItemModel } from '../repository/index.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-user-item-ref')
export class UmbUserItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbUserItemModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_USER_ENTITY_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_USER_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
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
				?readonly=${this.readonly}
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
