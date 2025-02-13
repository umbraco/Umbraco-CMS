import type { UmbMediaItemModel } from '../repository/types.js';
import { UMB_MEDIA_SECTION_ALIAS } from '../../media-section/constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-media-item-ref')
export class UmbMediaItemRefElement extends UmbLitElement {
	#item?: UmbMediaItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbMediaItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbMediaItemModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (!this.#item) {
			this.#modalRoute?.destroy();
			return;
		}
		if (oldValue?.unique === this.#item.unique) {
			return;
		}

		this.#modalRoute?.setUniquePathValue('unique', this.#item.unique);
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
			.addAdditionalPath(UMB_MEDIA_ENTITY_TYPE)
			.addUniquePaths(['unique'])
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
