import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbDocumentTypePropertyTypeReferenceModel } from './types.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-document-type-property-type-item-ref')
export class UmbDocumentTypePropertyTypeItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDocumentTypePropertyTypeReferenceModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addUniquePaths(['unique'])
			.onSetup(() => {
				return { data: { entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref() {
		if (!this.item?.unique) return;
		const path = UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({ unique: this.item.documentType.unique });
		return `${this._editPath}/${path}`;
	}

	#getName() {
		const documentTypeName = this.item?.documentType.name ?? 'Unknown';
		return `Document Type: ${documentTypeName}`;
	}

	#getDetail() {
		const propertyTypeDetails = this.item?.name ? this.item.name + ' (' + this.item.alias + ')' : 'Unknown';
		return `Property Type: ${propertyTypeDetails}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.#getName()}
				detail=${this.#getDetail()}
				href=${ifDefined(this.#getHref())}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon()}
			</uui-ref-node>
		`;
	}

	#renderIcon() {
		if (!this.item?.documentType.icon) return nothing;
		return html`<umb-icon slot="icon" name=${this.item.documentType.icon}></umb-icon>`;
	}
}

export { UmbDocumentTypePropertyTypeItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-property-type-item-ref': UmbDocumentTypePropertyTypeItemRefElement;
	}
}
