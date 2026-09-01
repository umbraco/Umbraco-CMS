import type { UmbMediaItemModel } from '../repository/types.js';
import { UMB_MEDIA_SECTION_ALIAS } from '../../media-section/constants.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
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
		this.#item = value;
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

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
	}

	#getHref(item: UmbMediaItemModel) {
		// No `_editPath` means the modal route registration couldn't reach a parent route context
		// (e.g. this ref is rendered inside a non-routable modal). The workspace is still reachable, just
		// not as a route relative to this host, so link to it by its absolute path instead.
		if (!this._editPath) return UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
		const path = UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({ unique: item.unique });
		return `${this._editPath}/${path}`;
	}

	// An absolute href leaves whatever the host is showing (a modal, a workspace with unsaved changes), so
	// open it in a new tab. A route-relative href stays within the current view and navigates in place.
	#getTarget() {
		return this._editPath ? undefined : '_blank';
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.item.name}
				href=${ifDefined(this.#getHref(this.item))}
				target=${ifDefined(this.#getTarget())}
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
