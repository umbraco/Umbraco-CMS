import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbDocumentItemDataResolver } from './document-item-data-resolver.js';
import {
	classMap,
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
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
				class=${classMap({ draft: this.#isDraft() })}
				name=${this.#item.getName()}
				href=${ifDefined(this.#getHref())}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon()} ${this.#renderIsTrashed()}
			</uui-ref-node>
		`;
	}

	#isDraft() {
		return this.#item.getState() === 'Draft';
	}

	#renderIcon() {
		const icon = this.#item.getIcon();
		if (!icon) return nothing;
		return html`<umb-icon slot="icon" name=${ifDefined(icon)}></umb-icon>`;
	}

	#renderIsTrashed() {
		if (!this.#item.isTrashed()) return nothing;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	static override styles = [
		css`
			.draft {
				opacity: 0.6;
			}
		`,
	];
}

export { UmbDocumentItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-item-ref': UmbDocumentItemRefElement;
	}
}
