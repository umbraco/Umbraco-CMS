import { UmbUserGroupCollectionRepository } from '@umbraco-cms/backoffice/user-group';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UMB_ENTITY_USER_PERMISSION_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_USER_GROUP_PICKER_MODAL,
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalResult,
	UmbModalContext,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/events';

@customElement('umb-permissions-modal')
export class UmbPermissionsModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbEntityUserPermissionSettingsModalData, UmbEntityUserPermissionSettingsModalResult>;

	@property({ type: Object })
	data?: UmbEntityUserPermissionSettingsModalData;

	@state()
	_userGroups: Array<UserGroupItemResponseModel> = [];

	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);
	#modalManagerContext?: UmbModalManagerContext;

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

	#openUserGroupPickerModal() {
		if (!this.#modalManagerContext) return;

		const modalContext = this.#modalManagerContext.open(UMB_USER_GROUP_PICKER_MODAL);

		modalContext.addEventListener(UmbSelectedEvent.TYPE, (event) => {
			const selectedEvent = event as UmbSelectedEvent;
			this.#openUserPermissionsModal(selectedEvent.unique);
		});

		modalContext?.onSubmit().then((result) => {
			console.log(result);
			debugger;
		});
	}

	#openUserPermissionsModal(id: string) {
		if (!id) throw new Error('Could not open permissions modal, no id was provided');

		const modalContext = this.#modalManagerContext?.open(UMB_ENTITY_USER_PERMISSION_MODAL, {
			unique: id,
			entityType: ['user-group'],
		});

		modalContext?.onSubmit().then((result) => {
			console.log(result);
			debugger;
		});
	}

	async firstUpdated() {
		const { data } = await this.#userGroupCollectionRepository.requestCollection();

		if (data) {
			this._userGroups = data.items;
		}
	}

	render() {
		return html`
			<umb-body-layout headline="Permissions">
				<uui-box>
					<uui-button @click=${this.#openUserGroupPickerModal} look="placeholder">Open</uui-button>
				</uui-box>

				<ul>
					${this._userGroups.map((group) => html`<li>${group}</li>`)}
				</ul>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label="Confirm"
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
