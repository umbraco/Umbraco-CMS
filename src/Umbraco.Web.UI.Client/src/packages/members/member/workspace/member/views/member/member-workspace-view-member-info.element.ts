import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../member-workspace.context-token.js';
import { UmbMemberKind, type UmbMemberKindType } from '../../../../utils/index.js';
import { TimeFormatOptions } from './utils.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UmbMemberTypeItemRepository } from '@umbraco-cms/backoffice/member-type';
import type { UmbMemberTypeItemModel } from '@umbraco-cms/backoffice/member-type';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-member-workspace-view-member-info')
export class UmbMemberWorkspaceViewMemberInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _memberTypeUnique?: string;

	@state()
	private _memberType?: UmbMemberTypeItemModel;

	@state()
	private _createDate = this.localize.term('general_unknown');

	@state()
	private _updateDate = this.localize.term('general_unknown');

	@state()
	private _unique = '';

	@state()
	private _memberKind?: UmbMemberKindType;

	@state()
	private _hasSettingsAccess: boolean = false;

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;
	#memberTypeItemRepository: UmbMemberTypeItemRepository = new UmbMemberTypeItemRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, async (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext?.contentTypeUnique, async (unique) => {
				if (unique === this._memberTypeUnique) return;
				this._memberTypeUnique = unique;

				if (this._memberTypeUnique) {
					this._memberType = (await this.#memberTypeItemRepository.requestItems([this._memberTypeUnique])).data?.[0];
				}
			});
			this.observe(this.#workspaceContext?.createDate, (date) => (this._createDate = this.#setDateFormat(date)));
			this.observe(this.#workspaceContext?.updateDate, (date) => (this._updateDate = this.#setDateFormat(date)));
			this.observe(this.#workspaceContext?.unique, (unique) => (this._unique = unique || ''));
			this.observe(this.#workspaceContext?.kind, (kind) => (this._memberKind = kind));
		});

		createExtensionApiByAlias(this, UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					match: UMB_SETTINGS_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._hasSettingsAccess = permitted;
				},
			},
		]);
	}

	#setDateFormat(date: string | undefined | null): string {
		if (!date) return this.localize.term('general_unknown');
		return this.localize.date(date, TimeFormatOptions);
	}

	override render() {
		return this.#renderGeneralSection();
	}

	#isExternalOnly() {
		return this._memberKind === UmbMemberKind.EXTERNAL_ONLY;
	}

	#renderGeneralSection() {
		return html`
			<umb-stack look="compact">
				<div>
					<h4><umb-localize key="content_createDate">Created</umb-localize></h4>
					<span> ${this._createDate} </span>
				</div>
				<div>
					<h4><umb-localize key="content_updateDate">Last edited</umb-localize></h4>
					<span> ${this._updateDate} </span>
				</div>
				${this.#isExternalOnly()
					? nothing
					: html`<div>
							<h4><umb-localize key="content_membertype">Member Type</umb-localize></h4>
							<umb-entity-item-ref
								standalone
								?readonly=${!this._hasSettingsAccess}
								.item=${this._memberType}></umb-entity-item-ref>
						</div>`}
				<div>
					<h4><umb-localize key="member_kind"></umb-localize></h4>
					<span
						>${this._memberKind === UmbMemberKind.API
							? this.localize.term('member_memberKindApi')
							: this._memberKind === UmbMemberKind.EXTERNAL_ONLY
								? this.localize.term('member_memberKindExternalOnly')
								: this.localize.term('member_memberKindDefault')}</span
					>
				</div>
				<div>
					<h4><umb-localize key="template_id">Id</umb-localize></h4>
					<span>${this._unique}</span>
				</div>
			</umb-stack>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			h4 {
				margin: 0;
			}

			uui-ref-node[readonly] {
				padding-top: 7px;
				padding-bottom: 7px;
			}
		`,
	];
}

export default UmbMemberWorkspaceViewMemberInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-view-member-info': UmbMemberWorkspaceViewMemberInfoElement;
	}
}
