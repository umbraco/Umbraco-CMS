import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbUserRepository } from '../../repository/user.repository';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalHandler, UmbUserPickerModalData, UmbUserPickerModalResult } from '@umbraco-cms/backoffice/modal';
import { createExtensionClass, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-picker-modal')
export class UmbUserPickerModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbUserPickerModalData, UmbUserPickerModalResult>;

	@property({ type: Object, attribute: false })
	data?: UmbUserPickerModalData;

	@state()
	_selection: Array<string> = [];

	@state()
	_multiple = false;

	@state()
	private _users: Array<UserResponseModel> = [];

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
		this.modalHandler?.submit({ selection: this._selection });
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
							<uui-menu-item label=${user.name} selectable>
								<uui-avatar slot="icon" name=${user.name}></uui-avatar>
								Hello</uui-menu-item
							>
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
