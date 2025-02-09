import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaItemModel } from '../types.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-media-item-ref')
export class UmbMediaItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbMediaItemModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_MEDIA_ENTITY_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_MEDIA_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref(item: UmbMediaItemModel) {
		return `${this._editPath}/edit/${item.unique}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				id=${this.item.unique}
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)} ${this.#renderIsTrashed(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbMediaItemModel) {
		if (!item.mediaType.icon) return;
		return html`<umb-icon slot="icon" name=${item.mediaType.icon}></umb-icon>`;
	}

	#renderIsTrashed(item: UmbMediaItemModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	static override styles = [];
}

export { UmbMediaItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-item-ref': UmbMediaItemRefElement;
	}
}
