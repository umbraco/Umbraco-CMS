import type { UmbLanguageItemModel } from '../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-language-item-ref')
export class UmbLanguageItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbLanguageItemModel;

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
				return { data: { entityType: UMB_LANGUAGE_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref() {
		if (!this.item?.unique) return;
		const path = UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN.generateLocal({ unique: this.item.unique });
		return `${this._editPath}/${path}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.item.name}
				href=${ifDefined(this.#getHref())}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				<umb-icon slot="icon" name="icon-globe"></umb-icon>
			</uui-ref-node>
		`;
	}
}

export { UmbLanguageItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-item-ref': UmbLanguageItemRefElement;
	}
}
