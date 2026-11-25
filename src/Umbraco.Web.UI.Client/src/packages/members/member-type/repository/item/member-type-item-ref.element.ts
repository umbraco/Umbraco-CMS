import { UMB_EDIT_MEMBER_TYPE_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_MEMBER_TYPE_WORKSPACE_MODAL } from '../../workspace/constants.js';
import type { UmbMemberTypeItemModel } from './types.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';
import { umbGenerateWorkspaceLink } from '@umbraco-cms/backoffice/workspace';
import { UmbEntityItemRefFrameMixin } from '@umbraco-cms/backoffice/entity-item';
import type { UUISelectableEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

/**
 * Renders a member type item as a reference node, with an optional entity frame.
 * The reference links to the member type workspace, opened in a modal, when the user has access to the Settings section.
 * @element umb-member-type-item-ref
 * @slot actions - Actions rendered alongside the reference.
 */
@customElement('umb-member-type-item-ref')
export class UmbMemberTypeItemRefElement extends UmbEntityItemRefFrameMixin(UmbLitElement) {
	/** The member type item to render. Nothing is rendered when undefined. */
	@property({ type: Object })
	public item?: UmbMemberTypeItemModel;

	/** When true, the reference is not navigable. */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/** Renders the reference as a standalone node. */
	@property({ type: Boolean })
	standalone = false;

	/** When true, the reference can only be selected, not navigated. */
	@property({ type: Boolean, attribute: 'select-only', reflect: true })
	selectOnly = false;

	@property({ type: Boolean, reflect: true })
	selectable = false;

	@property({ type: Boolean, reflect: true })
	selected = false;

	@property({ type: Boolean, reflect: true })
	disabled = false;

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

		new UmbModalRouteRegistrationController(this, UMB_MEMBER_TYPE_WORKSPACE_MODAL)
			.addUniquePaths(['unique'])
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	protected override get isEntityFrameNavigable() {
		return !this.readonly && !this.selectOnly && !this.disabled && this._userHasSectionAccess && !!this.item?.unique;
	}

	#getLink(item: UmbMemberTypeItemModel) {
		if (!item.unique) return;
		return umbGenerateWorkspaceLink({
			pattern: UMB_EDIT_MEMBER_TYPE_WORKSPACE_PATH_PATTERN,
			params: { unique: item.unique },
			routePath: this._editPath,
		});
	}

	#onSelected(event: UUISelectableEvent) {
		event.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this.item?.unique ?? ''));
	}

	#onDeselected(event: UUISelectableEvent) {
		event.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this.item?.unique ?? ''));
	}

	override render() {
		if (!this.item) return nothing;

		const link = this.#getLink(this.item);

		return html`
			<uui-ref-node
				name=${this.localize.string(this.item.name)}
				href=${ifDefined(link?.href)}
				target=${ifDefined(link?.target)}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}
				?select-only=${this.selectOnly}
				?selectable=${this.selectable}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}>
				<slot name="actions" slot="actions"></slot>
				${this.item.icon ? html`<umb-icon slot="icon" name=${this.item.icon}></umb-icon>` : nothing}
			</uui-ref-node>
			${this.renderEntityFrame()}
		`;
	}
}

export { UmbMemberTypeItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-item-ref': UmbMemberTypeItemRefElement;
	}
}
