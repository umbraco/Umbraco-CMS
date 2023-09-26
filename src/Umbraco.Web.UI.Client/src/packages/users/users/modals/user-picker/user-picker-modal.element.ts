import { UmbUserRepository } from '../../repository/user.repository.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbUserPickerModalData, UmbUserPickerModalResult } from '@umbraco-cms/backoffice/modal';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';
import { type UmbUserDetail } from '@umbraco-cms/backoffice/users';

@customElement('umb-user-picker-modal')
export class UmbUserPickerModalElement extends UmbModalBaseElement<UmbUserPickerModalData, UmbUserPickerModalResult> {
	@state()
	private _users: Array<UmbUserDetail> = [];

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
					const result = await createExtensionApi<UmbUserRepository>(repositoryManifest, [this]);
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
		this.modalContext?.submit({ selection: this.#selectionManager.getSelection() });
	}

	#close() {
		this.modalContext?.reject();
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
								@deselected=${() => this.#selectionManager.deselect(user.id!)}
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
		UmbTextStyles,
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
