import { UmbDocumentPermissionRepository } from '../../user-permissions/index.js';
import { UmbUserGroupRepository } from '@umbraco-cms/backoffice/user-group';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UMB_ENTITY_USER_PERMISSION_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_USER_GROUP_PICKER_MODAL,
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue,
	UmbModalContext,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/events';

type UmbUserGroupRefData = {
	id: string;
	name?: string;
	icon?: string | null;
	permissions: Array<string>;
};

@customElement('umb-permissions-modal')
export class UmbPermissionsModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbEntityUserPermissionSettingsModalData, UmbEntityUserPermissionSettingsModalValue>;

	@property({ type: Object })
	data?: UmbEntityUserPermissionSettingsModalData;

	@state()
	_userGroups: Array<UmbUserGroupRefData> = [];

	@state()
	_userPermissions: Array<any> = [];

	#userGroupRepository = new UmbUserGroupRepository(this);
	#documentPermissionRepository = new UmbDocumentPermissionRepository(this);
	#modalManagerContext?: UmbModalManagerContext;

	#userGroupPickerModal?: UmbModalContext;

	private _handleConfirm() {
		this.modalContext?.submit();
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	protected async firstUpdated(): Promise<void> {
		if (!this.data?.unique) throw new Error('Could not load permissions, no unique was provided');
		const { data } = await this.#documentPermissionRepository.requestPermissions(this.data.unique);
		if (data) {
			this._userPermissions = data;
		}
	}

	async connectedCallback(): Promise<void> {
		super.connectedCallback();

		const userGroupIds = [...new Set(this._userPermissions.map((permission) => permission.target.userGroupId))];
		const { data } = await this.#userGroupRepository.requestItems(userGroupIds);

		const userGroups = data ?? [];

		this._userGroups = this._userPermissions.map((entry) => {
			const userGroup = userGroups.find((userGroup) => userGroup.id == entry.target.userGroupId);
			return {
				id: entry.target.userGroupId,
				name: userGroup?.name,
				icon: userGroup?.icon,
				permissions: entry.permissions,
			};
		});
	}

	#openUserGroupPickerModal() {
		if (!this.#modalManagerContext) return;

		this.#userGroupPickerModal = this.#modalManagerContext.open(UMB_USER_GROUP_PICKER_MODAL);

		this.#userGroupPickerModal.addEventListener(UmbSelectedEvent.TYPE, (event) =>
			this.#openUserPermissionsModal((event as UmbSelectedEvent).unique),
		);
	}

	#openUserPermissionsModal(id: string) {
		if (!id) throw new Error('Could not open permissions modal, no id was provided');

		const modalContext = this.#modalManagerContext?.open(UMB_ENTITY_USER_PERMISSION_MODAL, {
			unique: id,
			entityType: ['document'],
		});

		modalContext?.onSubmit().then((value) => {
			console.log(value);
		});
	}

	render() {
		return html`
			<umb-body-layout headline="Permissions">
				<uui-box>
					<uui-ref-list>
						${this._userGroups.map(
							(userGroup) =>
								html`<umb-user-group-ref
									name=${ifDefined(userGroup.name)}
									.userPermissionAliases=${userGroup.permissions}
									border>
									<uui-icon slot="icon" .icon=${userGroup.icon}></uui-icon>
								</umb-user-group-ref>`,
						)}
					</uui-ref-list>
					<uui-button style="width: 100%;" @click=${this.#openUserGroupPickerModal} look="placeholder"
						>Select user group</uui-button
					>
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label="Save"
					@click=${this._handleConfirm}></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPermissionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-permissions-modal': UmbPermissionsModalElement;
	}
}
