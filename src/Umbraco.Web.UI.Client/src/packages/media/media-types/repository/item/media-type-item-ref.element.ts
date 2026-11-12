import { UMB_EDIT_MEDIA_TYPE_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_MEDIA_TYPE_WORKSPACE_MODAL } from '../../workspace/constants.js';
import type { UmbMediaTypeItemModel } from './types.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';
import { umbGenerateWorkspaceLink } from '@umbraco-cms/backoffice/workspace';
import { UmbEntityItemRefFrameMixin } from '@umbraco-cms/backoffice/entity-item';

@customElement('umb-media-type-item-ref')
export class UmbMediaTypeItemRefElement extends UmbEntityItemRefFrameMixin(UmbLitElement) {
	@property({ type: Object })
	public item?: UmbMediaTypeItemModel;

	@property({ type: Boolean, reflect: true })
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
					match: UMB_SETTINGS_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._userHasSectionAccess = permitted;
				},
			},
		]);

		new UmbModalRouteRegistrationController(this, UMB_MEDIA_TYPE_WORKSPACE_MODAL)
			.addUniquePaths(['unique'])
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	protected override get isEntityFrameNavigable() {
		return !this.readonly && this._userHasSectionAccess && !!this.item?.unique;
	}

	#getLink(item: UmbMediaTypeItemModel) {
		if (!item.unique) return;
		return umbGenerateWorkspaceLink({
			pattern: UMB_EDIT_MEDIA_TYPE_WORKSPACE_PATH_PATTERN,
			params: { unique: item.unique },
			routePath: this._editPath,
		});
	}

	override render() {
		if (!this.item) return nothing;

		const link = this.#getLink(this.item);

		return html`
			<uui-ref-node-document-type
				name=${this.localize.string(this.item.name)}
				href=${ifDefined(link?.href)}
				target=${ifDefined(link?.target)}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.item.icon ? html`<umb-icon slot="icon" name=${this.item.icon}></umb-icon>` : nothing}
			</uui-ref-node-document-type>
			${this.renderEntityFrame()}
		`;
	}
}

export { UmbMediaTypeItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-item-ref': UmbMediaTypeItemRefElement;
	}
}
