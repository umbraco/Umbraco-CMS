import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from '../../section/constants.js';
import { UMB_EDIT_MEMBER_GROUP_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbMemberGroupItemModel } from '../repository/item/types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbGenerateWorkspaceLink, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-member-group-item-ref')
export class UmbMemberGroupItemRefElement extends UmbLitElement {
	#item?: UmbMemberGroupItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbMemberGroupItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbMemberGroupItemModel | undefined) {
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
					match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._userHasSectionAccess = permitted;
				},
			},
		]);

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addUniquePaths(['unique'])
			.onSetup(() => {
				return { data: { entityType: UMB_MEMBER_GROUP_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});
	}

	#getLink(item: UmbMemberGroupItemModel) {
		if (!item.unique) return;
		return umbGenerateWorkspaceLink({
			pattern: UMB_EDIT_MEMBER_GROUP_WORKSPACE_PATH_PATTERN,
			params: { unique: item.unique },
			routePath: this._editPath,
		});
	}

	override render() {
		if (!this.item) return nothing;

		const link = this.#getLink(this.item);

		return html`
			<uui-ref-node
				name=${this.item.name}
				href=${ifDefined(link?.href)}
				target=${ifDefined(link?.target)}
				?readonly=${this.readonly || !this._userHasSectionAccess}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				<umb-icon slot="icon" name="icon-users"></umb-icon>
			</uui-ref-node>
			<umb-entity-frame><uui-icon name="link"></uui-icon> ${this.item.name}</umb-entity-frame>
		`;
	}

	static override readonly styles = [
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

export { UmbMemberGroupItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-item-ref': UmbMemberGroupItemRefElement;
	}
}
