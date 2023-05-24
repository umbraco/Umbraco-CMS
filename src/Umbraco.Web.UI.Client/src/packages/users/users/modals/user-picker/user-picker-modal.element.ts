import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbUserRepository } from '../../repository/user.repository.js';
import { UmbUserPickerModalData, UmbUserPickerModalResult } from '@umbraco-cms/backoffice/modal';
import { createExtensionClass } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';

@customElement('umb-user-picker-modal')
export class UmbUserPickerModalElement extends UmbModalBaseElement<UmbUserPickerModalData, UmbUserPickerModalResult> {
	@state()
	private _users: Array<UserResponseModel> = [];

	#selectionManager = new UmbSelectionManagerBase();
	#userRepository?: UmbUserRepository;

	constructor() {
		super();

		// TODO: this code is reused in multiple places, so it should be extracted to a function
		new UmbObserverController(
			this,
			umbExtensionsRegistry.getByTypeAndAlias('repository', 'Umb.Repository.User'),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<UmbUserRepository>(repositoryManifest, [this]);
					this.#userRepository = result;
					this.#observeUsers();
				} catch (error) {
					throw new Error('Could not create repository with alias: Umb.Repository.User');
				}
			}
		);
	}

	async #observeUsers() {
		if (!this.#userRepository) return;
		// TODO is this the correct end point?
		const { data } = await this.#userRepository.requestCollection();

		if (data) {
			this._users = data.items;
		}
	}

	#submit() {
		this.modalHandler?.submit({ selection: this.#selectionManager.getSelection() });
	}

	#close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Select Users">
				<uui-box>
					${this._users.map(
						(user) => html`
							<uui-menu-item
								label=${user.name}
								selectable
								@selected=${() => this.#selectionManager.select(user.id!)}
								@unselected=${() => this.#selectionManager.deselect(user.id!)}
								?selected=${this.#selectionManager.isSelected(user.id!)}>
								<uui-avatar slot="icon" name=${user.name}></uui-avatar>
							</uui-menu-item>
						`
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this.#close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-avatar {
				border: 2px solid var(--uui-color-surface);
				font-size: 12px;
			}
		`,
	];
}

export default UmbUserPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-picker-modal': UmbUserPickerModalElement;
	}
}
