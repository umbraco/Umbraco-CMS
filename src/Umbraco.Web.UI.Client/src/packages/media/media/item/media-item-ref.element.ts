import type { UmbMediaItemModel } from '../repository/types.js';
import { UMB_MEDIA_SECTION_ALIAS } from '../../media-section/constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UmbMediaItemDataResolver } from './media-item-data-resolver.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_MODAL, umbGenerateWorkspaceLink } from '@umbraco-cms/backoffice/workspace';
import { UmbEntityItemRefFrameMixin } from '@umbraco-cms/backoffice/entity-item';

@customElement('umb-media-item-ref')
export class UmbMediaItemRefElement extends UmbEntityItemRefFrameMixin(UmbLitElement) {
	#item = new UmbMediaItemDataResolver<UmbMediaItemModel>(this);

	@property({ type: Object })
	public get item(): UmbMediaItemModel | undefined {
		return this.#item.getData();
	}
	public set item(value: UmbMediaItemModel | undefined) {
		this.#item.setData(value);
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	private _unique = '';

	@state()
	private _name = '';

	@state()
	private _icon = '';

	@state()
	private _editPath = '';

	@state()
	private _userHasSectionAccess = false;

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

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addUniquePaths(['unique'])
			.onSetup(() => {
				return { data: { entityType: UMB_MEDIA_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});

		this.#item.observe(this.#item.unique, (unique) => (this._unique = unique ?? ''));
		this.#item.observe(this.#item.name, (name) => (this._name = name ?? ''));
		this.#item.observe(this.#item.icon, (icon) => (this._icon = icon ?? ''));
	}

	protected override get isEntityFrameNavigable() {
		return !this.readonly && this._userHasSectionAccess && !!this._unique;
	}

	#getLink() {
		if (!this._unique) return;
		return umbGenerateWorkspaceLink({
			pattern: UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN,
			params: { unique: this._unique },
			routePath: this._editPath,
		});
	}

	override render() {
		if (!this.item) return nothing;

		const link = this.#getLink();

		return html`
			<uui-ref-node
				name=${this._name}
				href=${ifDefined(link?.href)}
				target=${ifDefined(link?.target)}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon()}
			</uui-ref-node>
			${this.renderEntityFrame()}
		`;
	}

	#renderIcon() {
		if (!this._icon) return nothing;
		return html`<umb-icon slot="icon" name=${this._icon}></umb-icon>`;
	}
}

export { UmbMediaItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-item-ref': UmbMediaItemRefElement;
	}
}
