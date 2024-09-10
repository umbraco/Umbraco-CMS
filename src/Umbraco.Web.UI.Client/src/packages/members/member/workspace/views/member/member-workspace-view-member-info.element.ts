// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../member-workspace.context-token.js';
import { TimeFormatOptions } from './utils.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbMemberTypeItemRepository } from '@umbraco-cms/backoffice/member-type';

@customElement('umb-member-workspace-view-member-info')
export class UmbMemberWorkspaceViewMemberInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _memberTypeUnique = '';
	@state()
	private _memberTypeName = '';
	@state()
	private _memberTypeIcon = '';

	@state()
	private _editMemberTypePath = '';

	@state()
	private _createDate = this.localize.term('general_unknown');

	@state()
	private _updateDate = this.localize.term('general_unknown');

	@state()
	private _unique = '';

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;
	#memberTypeItemRepository: UmbMemberTypeItemRepository = new UmbMemberTypeItemRepository(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('member-type')
			.onSetup(() => {
				return { data: { entityType: 'member-type', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMemberTypePath = routeBuilder({});
			});

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, async (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext.contentTypeUnique, (unique) => (this._memberTypeUnique = unique || ''));
			this.observe(this.#workspaceContext.createDate, (date) => (this._createDate = this.#setDateFormat(date)));
			this.observe(this.#workspaceContext.updateDate, (date) => (this._updateDate = this.#setDateFormat(date)));
			this.observe(this.#workspaceContext.unique, (unique) => (this._unique = unique || ''));

			const memberType = (await this.#memberTypeItemRepository.requestItems([this._memberTypeUnique])).data?.[0];
			if (!memberType) return;
			this._memberTypeName = memberType.name;
			this._memberTypeIcon = memberType.icon;
		});
	}

	#setDateFormat(date: string | undefined | null): string {
		if (!date) return this.localize.term('general_unknown');
		return this.localize.date(date, TimeFormatOptions);
	}

	override render() {
		return this.#renderGeneralSection();
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
				<div>
					<h4><umb-localize key="content_membertype">Member Type</umb-localize></h4>
					<uui-ref-node
						standalone
						.name=${this._memberTypeName}
						.href=${this._editMemberTypePath + 'edit/' + this._memberTypeUnique}>
						<umb-icon slot="icon" .name=${this._memberTypeIcon}></umb-icon>
					</uui-ref-node>
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
		`,
	];
}

export default UmbMemberWorkspaceViewMemberInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-view-member-info': UmbMemberWorkspaceViewMemberInfoElement;
	}
}
