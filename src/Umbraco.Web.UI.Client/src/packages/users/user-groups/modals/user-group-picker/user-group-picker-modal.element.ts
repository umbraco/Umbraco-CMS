import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbUserGroupRepository } from '../../repository/user-group.repository.js';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { createExtensionClass } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<any, any> {
	@state()
	private _userGroups: Array<UserGroupResponseModel> = [];

	#selectionManager = new UmbSelectionManagerBase();
	#userGroupRepository?: UmbUserGroupRepository;

	connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.data?.selection ?? []);

		// TODO: this code is reused in multiple places, so it should be extracted to a function
		new UmbObserverController(
			this,
			umbExtensionsRegistry.getByTypeAndAlias('repository', 'Umb.Repository.UserGroup'),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<UmbUserGroupRepository>(repositoryManifest, [this]);
					this.#userGroupRepository = result;
					this.#observeUserGroups();
				} catch (error) {
					throw new Error('Could not create repository with alias: Umb.Repository.User');
				}
			}
		);
	}

	async #observeUserGroups() {
		if (!this.#userGroupRepository) return;
		// TODO is this the correct end point?
		const { data } = await this.#userGroupRepository.requestCollection();

		if (data) {
			this._userGroups = data.items;
		}
	}

	#submit() {
		this.modalHandler?.submit({
			selection: this.#selectionManager.getSelection(),
		});
	}

	#close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-workspace-editor headline="Select user groups">
				<uui-box>
					${this._userGroups.map(
						(item) => html`
							<uui-menu-item
								label=${item.name}
								selectable
								@selected=${() => this.#selectionManager.select(item.id!)}
								@deselected=${() => this.#selectionManager.deselect(item.id!)}
								?selected=${this.#selectionManager.isSelected(item.id!)}>
								<uui-icon .name=${item.icon} slot="icon"></uui-icon>
							</uui-menu-item>
						`
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this.#close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
