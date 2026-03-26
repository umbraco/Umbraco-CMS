import type { UmbCurrentUserModel } from '../../types.js';
import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import { UmbCurrentUserRepository } from '../../repository/current-user.repository.js';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';
import { css, html, customElement, query, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../../../../user/user/components/user-avatar/user-avatar.element.js';

@customElement('umb-current-user-workspace-avatar')
export class UmbCurrentUserWorkspaceAvatarElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbCurrentUserModel;

	@state()
	private _allowedFileTypes = 'image/*';

	@query('#AvatarFileField')
	private _avatarFileField?: HTMLInputElement;

	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#currentUserRepository = new UmbCurrentUserRepository(this);
	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			if (!this.#currentUserContext) return;
			this.#observeCurrentUser();
		});

		this.#observeAllowedFileTypes();
	}

	async #observeAllowedFileTypes() {
		await this.#temporaryFileConfigRepository.initialized;
		this.observe(
			this.#temporaryFileConfigRepository.part('imageFileTypes'),
			(fileTypes) => {
				if (fileTypes.length) {
					this._allowedFileTypes = fileTypes.map((t) => `.${t}`).join(',');
				} else {
					this._allowedFileTypes = 'image/*';
				}
			},
			'_imageFileTypes',
		);
	}

	#observeCurrentUser = () => {
		this.observe(
			this.#currentUserContext!.currentUser,
			(user) => {
				this._currentUser = user;
			},
			'umbCurrentUserObserver',
		);
	};

	#uploadAvatar = async () => {
		try {
			const selectedFile = await this.#selectAvatar();
			await this.#currentUserRepository.uploadAvatar(selectedFile);
		} catch (error) {
			console.error(error);
		}
	};

	#selectAvatar() {
		return new Promise<File>((resolve, reject) => {
			if (!this._avatarFileField) {
				reject("Can't find avatar file field");
				return;
			}

			// Reset value to allow selecting the same file again
			this._avatarFileField.value = '';

			this._avatarFileField.addEventListener(
				'change',
				(event) => {
					const target = event?.target as HTMLInputElement;
					const file = target.files?.[0];
					if (!file) {
						reject("Can't find avatar file");
						return;
					}
					resolve(file);
				},
				{ once: true },
			);

			this._avatarFileField.click();
		});
	}

	#deleteAvatar = async () => {
		await this.#currentUserRepository.deleteAvatar();
	};

	#hasAvatar() {
		if (!this._currentUser) return false;
		return this._currentUser.avatarUrls.length > 0;
	}

	override render() {
		if (!this._currentUser) return nothing;
		return html`
			<uui-box>
				<form id="AvatarUploadForm" novalidate>
					<umb-user-avatar
						id="Avatar"
						.name=${this._currentUser.name}
						.imgUrls=${this._currentUser.avatarUrls ?? []}></umb-user-avatar>
					<input id="AvatarFileField" type="file" name="avatarFile" accept=${this._allowedFileTypes} required hidden />
					<uui-button label="${this.localize.term('user_changePhoto')}" @click=${this.#uploadAvatar}></uui-button>
					${this.#hasAvatar()
						? html`
								<uui-button
									type="button"
									label=${this.localize.term('user_removePhoto')}
									@click=${this.#deleteAvatar}></uui-button>
							`
						: nothing}
				</form>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}

			#Avatar {
				font-size: 75px;
				place-self: center;
			}

			form {
				text-align: center;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbCurrentUserWorkspaceAvatarElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-workspace-avatar': UmbCurrentUserWorkspaceAvatarElement;
	}
}
