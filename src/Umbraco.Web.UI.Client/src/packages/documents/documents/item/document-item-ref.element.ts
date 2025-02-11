import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbDocumentItemDataResolver } from './document-item-data-resolver.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('umb-document-item-ref')
export class UmbDocumentItemRefElement extends UmbLitElement {
	#item = new UmbDocumentItemDataResolver(this);

	@property({ type: Object })
	public get item(): UmbDocumentItemModel | undefined {
		return this.#item.getItem();
	}
	public set item(value: UmbDocumentItemModel | undefined) {
		const oldValue = this.#item.getItem();
		this.#item.setItem(value);

		if (!value) {
			this.#modalRoute?.destroy();
			return;
		}

		if (oldValue?.unique !== value.unique) {
			this.#modalRoute = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
				.addAdditionalPath(UMB_DOCUMENT_ENTITY_TYPE + '/' + value.unique)
				.onSetup(() => {
					return { data: { entityType: UMB_DOCUMENT_ENTITY_TYPE, preset: {} } };
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
	_defaultCulture?: string;

	@state()
	_appCulture?: string;

	@state()
	_propertyDataSetCulture?: UmbVariantId;

	#modalRoute?: any;

	#getHref() {
		return `${this._editPath}/edit/${this.#item.getUnique()}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.#item.getName()}
				href=${ifDefined(this.#getHref())}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon()}${this.#renderIsDraft()} ${this.#renderIsTrashed()}
			</uui-ref-node>
		`;
	}

	#renderIcon() {
		const icon = this.#item.getIcon();
		if (!icon) return nothing;
		return html`<umb-icon slot="icon" name=${ifDefined(icon)}></umb-icon>`;
	}

	#renderIsTrashed() {
		if (!this.#item.getIsTrashed()) return nothing;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	#renderIsDraft() {
		if (!this.#item.getIsDraft()) return nothing;
		return html`<uui-tag size="s" slot="tag" look="secondary" color="default">Draft</uui-tag>`;
	}
}

export { UmbDocumentItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-item-ref': UmbDocumentItemRefElement;
	}
}
