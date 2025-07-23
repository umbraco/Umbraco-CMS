import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_MEDIA_TYPE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbMediaTypePropertyTypeReferenceModel } from './types.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-media-type-property-type-item-ref')
export class UmbMediaTypePropertyTypeItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbMediaTypePropertyTypeReferenceModel;

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
				return { data: { entityType: UMB_MEDIA_TYPE_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref() {
		if (!this.item?.unique) return;
		const path = UMB_EDIT_MEDIA_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({ unique: this.item.mediaType.unique });
		return `${this._editPath}/${path}`;
	}

	#getName() {
		const mediaTypeName = this.item?.mediaType.name ?? 'Unknown';
		return `Media Type: ${mediaTypeName}`;
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
		if (!this.item?.mediaType.icon) return nothing;
		return html`<umb-icon slot="icon" name=${this.item.mediaType.icon}></umb-icon>`;
	}
}

export { UmbMediaTypePropertyTypeItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-property-type-item-ref': UmbMediaTypePropertyTypeItemRefElement;
	}
}
