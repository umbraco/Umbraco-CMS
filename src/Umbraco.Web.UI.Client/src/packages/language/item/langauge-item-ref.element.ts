import type { UmbLanguageItemModel } from '../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbGenerateWorkspaceLink, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
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
	private _editPath = '';

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

	#getLink() {
		if (!this.item?.unique) return;
		return umbGenerateWorkspaceLink({
			pattern: UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN,
			params: { unique: this.item.unique },
			routePath: this._editPath,
		});
	}

	override render() {
		if (!this.item) return nothing;

		const link = this.#getLink();

		return html`
			<uui-ref-node
				name=${this.item.name}
				href=${ifDefined(link?.href)}
				target=${ifDefined(link?.target)}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				<umb-icon slot="icon" name="icon-globe"></umb-icon>
			</uui-ref-node>
			<umb-entity-frame><uui-icon name="link"></uui-icon> ${this.item.name}</umb-entity-frame>
		`;
	}

	static override styles = [
		css`
			:host {
				--umb-entity-frame-opacity: 0;
				--umb-entity-frame-color: var(--umb-color-reference);
				--umb-entity-frame-contrast-color: var(--umb-color-reference-contrast);

				display: block;
				position: relative;
			}

			:host(:hover),
			:host(:focus-within) {
				--umb-entity-frame-opacity: 1;
			}
		`,
	];
}

export { UmbLanguageItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-item-ref': UmbLanguageItemRefElement;
	}
}
