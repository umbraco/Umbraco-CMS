import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentItemModel } from './types.js';
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

@customElement('umb-document-item-ref')
export class UmbDocumentItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDocumentItemModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_DOCUMENT_ENTITY_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_DOCUMENT_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#isDraft(item: UmbDocumentItemModel) {
		return item.variants[0]?.state === 'Draft';
	}

	#getHref(item: UmbDocumentItemModel) {
		return `${this._editPath}/edit/${item.unique}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				id=${this.item.unique}
				class=${classMap({ draft: this.#isDraft(this.item) })}
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)} ${this.#renderIsTrashed(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDocumentItemModel) {
		if (!item.documentType.icon) return;
		return html`<umb-icon slot="icon" name=${item.documentType.icon}></umb-icon>`;
	}

	#renderIsTrashed(item: UmbDocumentItemModel) {
		if (!item.isTrashed) return;
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
