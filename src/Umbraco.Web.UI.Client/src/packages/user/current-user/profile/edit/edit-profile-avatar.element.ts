import { UmbCurrentUserRepository } from '../../repository/current-user.repository.js';
import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserModel } from '../../types.js';
import { css, customElement, html, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';

@customElement('umb-current-user-edit-profile-avatar')
export class UmbCurrentUserEditProfileAvatarElement extends UmbLitElement {
	#currentUserRepository = new UmbCurrentUserRepository(this);

	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	@state()
	private _allowedFileTypes = 'image/*';

	@state()
	private _currentUser?: UmbCurrentUserModel;

	@state()
	private _pendingDelete = false;

	@state()
	private _pendingFile?: File;

	@state()
	private _pendingPreviewUrl?: string;

	@query('#AvatarFileField')
	private _avatarFileField?: HTMLInputElement;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context?.currentUser, (currentUser) => (this._currentUser = currentUser), 'umbCurrentUserObserver');
		});

		this.#observeAllowedFileTypes();
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#revokePendingPreviewUrl();
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

	#revokePendingPreviewUrl() {
		if (this._pendingPreviewUrl) {
			URL.revokeObjectURL(this._pendingPreviewUrl);
			this._pendingPreviewUrl = undefined;
		}
	}

	#uploadAvatar = async () => {
		try {
			const selectedFile = await this.#selectAvatar();
			this.#revokePendingPreviewUrl();
			this._pendingFile = selectedFile;
			this._pendingPreviewUrl = URL.createObjectURL(selectedFile);
			this._pendingDelete = false;
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

	#deleteAvatar = () => {
		this.#revokePendingPreviewUrl();
		this._pendingFile = undefined;
		this._pendingDelete = (this._currentUser?.avatarUrls.length ?? 0) > 0;
	};

	async save(): Promise<boolean> {
		let error;
		if (this._pendingFile) {
			({ error } = await this.#currentUserRepository.uploadAvatar(this._pendingFile));
		} else if (this._pendingDelete) {
			({ error } = await this.#currentUserRepository.deleteAvatar());
		}

		if (error) return false;

		this.#revokePendingPreviewUrl();
		this._pendingFile = undefined;
		this._pendingDelete = false;
		return true;
	}

	get #displayAvatarUrls(): string[] {
		if (this._pendingDelete) return [];
		if (this._pendingPreviewUrl) return Array(5).fill(this._pendingPreviewUrl);
		return this._currentUser?.avatarUrls ?? [];
	}

	#hasAvatar() {
		if (this._pendingDelete) return false;
		if (this._pendingFile) return true;
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
						.imgUrls=${this.#displayAvatarUrls}></umb-user-avatar>
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

export default UmbCurrentUserEditProfileAvatarElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-edit-profile-avatar': UmbCurrentUserEditProfileAvatarElement;
	}
}
