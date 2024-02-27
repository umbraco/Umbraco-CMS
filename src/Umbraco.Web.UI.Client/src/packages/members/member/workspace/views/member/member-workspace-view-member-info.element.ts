// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbMemberWorkspaceContext } from '../../member-workspace.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbMemberTypeItemRepository } from '@umbraco-cms/backoffice/member-type';

@customElement('umb-member-workspace-view-member-info')
export class UmbMemberWorkspaceViewMemberInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _memberTypeUnique = '';
	@state()
	private _memberTypeName = '';
	@state()
	private _memberTypeIcon = '';

	private _workspaceContext?: UmbMemberWorkspaceContext;
	private _memberTypeItemRepository: UmbMemberTypeItemRepository = new UmbMemberTypeItemRepository(this);

	@state()
	private _editMemberTypePath = '';

	@state()
	private _createDate = 'Unknown';
	@state()
	private _updateDate = 'Unknown';

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

		this.consumeContext(UMB_WORKSPACE_CONTEXT, async (nodeContext) => {
			this._workspaceContext = nodeContext as UmbMemberWorkspaceContext;
			this.observe(this._workspaceContext.contentTypeUnique, (unique) => (this._memberTypeUnique = unique || ''));
			this.observe(this._workspaceContext.createDate, (date) => (this._createDate = date || 'Unknown'));
			this.observe(this._workspaceContext.updateDate, (date) => (this._updateDate = date || 'Unknown'));

			const memberType = (await this._memberTypeItemRepository.requestItems([this._memberTypeUnique])).data?.[0];
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
				<span>${this._memberTypeUnique}</span>
			</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.headline {
				font-weight: bold;
			}

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

			//General section

			#general-section {
				display: flex;
				flex-direction: column;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
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
