import { UmbDocumentPermissionRepository } from '../../user-permissions/index.js';
import { UmbDocumentItemRepository } from '../../repository/index.js';
import { UmbUserGroupItemRepository, UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/user-group';
import { html, customElement, property, state, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue,
	UmbModalContext,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UMB_ENTITY_USER_PERMISSION_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

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
	_entityItem?: any;

	@state()
	_userGroupRefs: Array<UmbUserGroupRefData> = [];

	#userPermissions: Array<any> = [];
	#userGroupIemRepository = new UmbUserGroupItemRepository(this);
	#documentPermissionRepository = new UmbDocumentPermissionRepository(this);
	#documentItemRepository = new UmbDocumentItemRepository(this);
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

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	protected async firstUpdated(): Promise<void> {
		if (!this.data?.unique) throw new Error('Could not load permissions, no unique was provided');
		this.#getEntityItem(this.data.unique);
		this.#getEntityPermissions(this.data.unique);
	}

	async #getEntityItem(unique: string) {
		const { data } = await this.#documentItemRepository.requestItems([unique]);
		if (!data) throw new Error('Could not load item');
		this._entityItem = data[0];
	}

	async #getEntityPermissions(unique: string) {
		const { data } = await this.#documentPermissionRepository.requestPermissions(unique);
		if (data) {
			this.#userPermissions = data;
			this.#mapToUserGroupRefs();
		}
	}

	async #mapToUserGroupRefs() {
		const userGroupIds = [...new Set(this.#userPermissions.map((permission) => permission.target.userGroupId))];
		const { data } = await this.#userGroupIemRepository.requestItems(userGroupIds);

		const userGroups = data ?? [];

		this._userGroupRefs = this.#userPermissions.map((entry) => {
			const userGroup = userGroups.find((userGroup) => userGroup.unique == entry.target.userGroupId);
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

		this.#userGroupPickerModal.addEventListener(UmbSelectedEvent.TYPE, (event) => {
			const selectEvent = event as UmbSelectedEvent;
			const unique = selectEvent.unique;
			if (!unique) return;
			this.#openUserPermissionsModal(unique);
		});
	}

	#openUserPermissionsModal(id: string) {
		if (!id) throw new Error('Could not open permissions modal, no id was provided');
		if (!this.data?.entityType) throw new Error('Could not open permissions modal, no entity type was provided');

		const userGroupRef = this._userGroupRefs.find((userGroup) => userGroup.id == id);

		const modalContext = this.#modalManagerContext?.open(UMB_ENTITY_USER_PERMISSION_MODAL, {
			data: {
				unique: id,
				entityType: this.data.entityType,
				allowedVerbs: userGroupRef?.permissions || [],
				headline: `Permissions for ${userGroupRef?.name}`,
			},
		});

		modalContext?.onSubmit().then((value) => {
			console.log(value);
		});
	}

	render() {
		return html`
			<umb-body-layout headline="Permissions for ${this._entityItem?.name}">
				<uui-box>
					Permissions set for User Groups for document: ${this.data?.entityType}:
					<uui-ref-list>
						${this._userGroupRefs.map(
							(userGroup) =>
								html`<umb-user-group-ref
									name=${ifDefined(userGroup.name)}
									.userPermissionAliases=${userGroup.permissions}
									@open=${() => this.#openUserPermissionsModal(userGroup.id)}
									standalone>
									${userGroup.icon ? html`<uui-icon slot="icon" name=${userGroup.icon}></uui-icon>` : nothing}
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
