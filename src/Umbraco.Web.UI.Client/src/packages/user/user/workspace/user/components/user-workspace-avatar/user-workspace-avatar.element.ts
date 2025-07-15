import type { UmbUserDetailModel } from '../../../../types.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import { css, html, customElement, query, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';

@customElement('umb-user-workspace-avatar')
export class UmbUserAvatarElement extends UmbLitElement {
	@state()
	private _user?: UmbUserDetailModel;

	@state()
	private _allowedFileTypes = 'image/*';

	@query('#AvatarFileField')
	_avatarFileField?: HTMLInputElement;

	@query('uui-combobox')
	private _selectElement!: HTMLInputElement;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			if (!this.#userWorkspaceContext) return;
			this.#observeUser();
		});

		this.#observeAllowedFileTypes();
	}

	protected getFormElement() {
		return this._selectElement;
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

	#observeUser = () => {
		this.observe(
			this.#userWorkspaceContext!.data,
			async (user) => {
				this._user = user;
			},
			'umbUserObserver',
		);
	};

	#uploadAvatar = async () => {
		try {
			const selectedFile = await this.#selectAvatar();
			this.#userWorkspaceContext?.uploadAvatar(selectedFile);
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

			this._avatarFileField.addEventListener('change', (event) => {
				const target = event?.target as HTMLInputElement;
				const file = target.files?.[0] as File;
				if (!file) {
					reject("Can't find avatar file");
					return;
				}

				resolve(file);
			});

			this._avatarFileField.click();
		});
	}

	#deleteAvatar = async () => {
		if (!this.#userWorkspaceContext) return;
		this.#userWorkspaceContext.deleteAvatar();
	};

	#hasAvatar() {
		if (!this._user) return false;
		return this._user.avatarUrls.length > 0;
	}

	override render() {
		if (!this._user) return nothing;
		return html`
			<uui-box>
				<form id="AvatarUploadForm" novalidate>
					<umb-user-avatar
						id="Avatar"
						.name=${this._user.name}
						.kind=${this._user.kind}
						.imgUrls=${this._user.avatarUrls ?? []}></umb-user-avatar>
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

export default UmbUserAvatarElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-avatar': UmbUserAvatarElement;
	}
}
