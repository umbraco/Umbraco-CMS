import { UMB_CURRENT_USER_WORKSPACE_CONTEXT } from './current-user-workspace.context-token.js';
import { css, customElement, html, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';

@customElement('umb-current-user-workspace-avatar')
export class UmbCurrentUserWorkspaceAvatarElement extends UmbLitElement {
	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	#workspaceContext?: typeof UMB_CURRENT_USER_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _allowedFileTypes = 'image/*';

	@state()
	private _name?: string;

	@state()
	private _avatarUrls: Array<string> = [];

	@state()
	private _pendingFile?: File;

	@state()
	private _pendingDelete = false;

	@state()
	private _pendingPreviewUrl?: string;

	@query('#AvatarFileField')
	private _avatarFileField?: HTMLInputElement;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			if (!context) return;

			this.observe(context.name, (name) => (this._name = name), 'umbCurrentUserWorkspaceAvatarNameObserver');
			this.observe(
				context.avatarUrls,
				(urls) => (this._avatarUrls = urls ?? []),
				'umbCurrentUserWorkspaceAvatarUrlsObserver',
			);
			this.observe(
				context.pendingAvatarFile,
				(file) => (this._pendingFile = file),
				'umbCurrentUserWorkspaceAvatarFileObserver',
			);
			this.observe(
				context.pendingAvatarDelete,
				(pendingDelete) => (this._pendingDelete = pendingDelete),
				'umbCurrentUserWorkspaceAvatarDeleteObserver',
			);
			this.observe(
				context.pendingAvatarPreviewUrl,
				(url) => (this._pendingPreviewUrl = url),
				'umbCurrentUserWorkspaceAvatarPreviewObserver',
			);
		});

		this.#observeAllowedFileTypes();
	}

	async #observeAllowedFileTypes() {
		await this.#temporaryFileConfigRepository.initialized;
		if (!this.isConnected) return;
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

	#uploadAvatar = async () => {
		try {
			const selectedFile = await this.#selectAvatar();
			this.#workspaceContext?.setAvatarFile(selectedFile);
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
		this.#workspaceContext?.markAvatarForDeletion();
	};

	get #displayAvatarUrls(): Array<string> {
		if (this._pendingDelete) return [];
		if (this._pendingPreviewUrl) return Array(5).fill(this._pendingPreviewUrl);
		return this._avatarUrls;
	}

	#hasAvatar(): boolean {
		if (this._pendingDelete) return false;
		if (this._pendingFile) return true;
		return this._avatarUrls.length > 0;
	}

	override render() {
		if (!this._name) return nothing;
		return html`
			<uui-box>
				<form id="AvatarUploadForm" novalidate>
					<umb-user-avatar id="Avatar" .name=${this._name} .imgUrls=${this.#displayAvatarUrls}></umb-user-avatar>
					<input id="AvatarFileField" type="file" name="avatarFile" accept=${this._allowedFileTypes} required hidden />
					<uui-button label=${this.localize.term('user_changePhoto')} @click=${this.#uploadAvatar}></uui-button>
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
