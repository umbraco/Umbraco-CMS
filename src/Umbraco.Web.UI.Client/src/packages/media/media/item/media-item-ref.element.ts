import type { UmbMediaItemModel } from '../repository/types.js';
import { UMB_MEDIA_SECTION_ALIAS } from '../../media-section/constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';

@customElement('umb-media-item-ref')
export class UmbMediaItemRefElement extends UmbLitElement {
	#item?: UmbMediaItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbMediaItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbMediaItemModel | undefined) {
		this.#item = value;
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	@state()
	_userHasSectionAccess = false;

	#modalRoute?: any;

	constructor() {
		super();

		createExtensionApiByAlias(this, UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					match: UMB_MEDIA_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._userHasSectionAccess = permitted;
				},
			},
		]);

		this.#modalRoute = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.onSetup(() => {
				return { data: { entityType: UMB_MEDIA_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getHref(item: UmbMediaItemModel) {
		if (!this._editPath) return undefined;
		const path = UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({ unique: item.unique });
		return `${this._editPath}/${path}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbMediaItemModel) {
		if (!item.mediaType.icon) return;
		return html`<umb-icon slot="icon" name=${item.mediaType.icon}></umb-icon>`;
	}
}

export { UmbMediaItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-item-ref': UmbMediaItemRefElement;
	}
}
