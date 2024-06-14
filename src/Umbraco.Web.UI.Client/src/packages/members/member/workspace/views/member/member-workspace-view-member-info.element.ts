// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../member-workspace.context-token.js';
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
	private _createDate = 'Unknown';
	@state()
	private _updateDate = 'Unknown';

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
			this.observe(this.#workspaceContext.createDate, (date) => (this._createDate = date || 'Unknown'));
			this.observe(this.#workspaceContext.updateDate, (date) => (this._updateDate = date || 'Unknown'));
			this.observe(this.#workspaceContext.unique, (unique) => (this._unique = unique || ''));

			const memberType = (await this.#memberTypeItemRepository.requestItems([this._memberTypeUnique])).data?.[0];
			if (!memberType) return;
			this._memberTypeName = memberType.name;
			this._memberTypeIcon = memberType.icon;
		});
	}

	render() {
		return this.#renderGeneralSection();
	}

	#renderGeneralSection() {
		return html`
			<div class="general-item">
				<umb-localize class="headline" key="content_createDate"></umb-localize>
				<span>
					<umb-localize-date .date=${this._createDate}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<umb-localize class="headline" key="content_updateDate"></umb-localize>
				<span>
					<umb-localize-date .date=${this._updateDate}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<span class="headline">Member Type</span>
				<div class="member-type-edit">
					<uui-icon .name=${this._memberTypeIcon}></uui-icon>
					<span>${this._memberTypeName}</span>
					<uui-button
						look="secondary"
						href=${this._editMemberTypePath + 'edit/' + this._memberTypeUnique}
						label=${this.localize.term('general_edit')}></uui-button>
				</div>
			</div>
			<div class="general-item">
				<umb-localize class="headline" key="template_id"></umb-localize>
				<span>${this._unique}</span>
			</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.member-type-edit {
				display: flex;
				align-items: center;
			}
			.member-type-edit uui-icon {
				margin-right: var(--uui-size-space-1);
			}
			.member-type-edit uui-button {
				margin-left: auto;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}
			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}
			.general-item .headline {
				font-weight: bold;
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
