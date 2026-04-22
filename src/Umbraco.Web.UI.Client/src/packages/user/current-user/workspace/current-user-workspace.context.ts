import type { UmbCurrentUserModel } from '../types.js';
import { UMB_CURRENT_USER_ENTITY_TYPE } from '../entity.js';
import { UmbCurrentUserRepository } from '../repository/current-user.repository.js';
import { UMB_CURRENT_USER_WORKSPACE_ALIAS } from './constants.js';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbSubmittableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

interface UmbCurrentUserWorkspacePendingAvatar {
	file?: File;
	delete?: boolean;
	previewUrl?: string;
}

export class UmbCurrentUserWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbCurrentUserModel>
	implements UmbSubmittableWorkspaceContext
{
	readonly #data = new UmbObjectState<UmbCurrentUserModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly avatarUrls = this.#data.asObservablePart((data) => data?.avatarUrls ?? []);
	readonly languageIsoCode = this.#data.asObservablePart((data) => data?.languageIsoCode);
	readonly unique = this.#data.asObservablePart((data) => data?.unique ?? null);

	readonly #pendingAvatar = new UmbObjectState<UmbCurrentUserWorkspacePendingAvatar>({});
	readonly pendingAvatarFile = this.#pendingAvatar.asObservablePart((p) => p.file);
	readonly pendingAvatarDelete = this.#pendingAvatar.asObservablePart((p) => p.delete ?? false);
	readonly pendingAvatarPreviewUrl = this.#pendingAvatar.asObservablePart((p) => p.previewUrl);

	#persistedLanguageIsoCode?: string;

	readonly #repository = new UmbCurrentUserRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_WORKSPACE_ALIAS);
		this.#load();
	}

	async #load() {
		const { data, asObservable } = await this.#repository.requestCurrentUser();

		if (data) {
			this.#data.setValue(data);
			this.#persistedLanguageIsoCode = data.languageIsoCode;
			this.view.setTitle(data.name);
		}

		if (asObservable) {
			this.observe(
				asObservable(),
				(currentUser) => {
					if (!currentUser) return;
					// Track the last known persisted language so submit() can tell whether the editable copy has diverged.
					this.#persistedLanguageIsoCode = currentUser.languageIsoCode;
					const current = this.#data.getValue();
					// Preserve any in-progress language edit made by the user when the store pushes a refresh (e.g. after an avatar save).
					const preservedLanguage =
						current && current.languageIsoCode !== currentUser.languageIsoCode ? current.languageIsoCode : undefined;
					this.#data.setValue(preservedLanguage ? { ...currentUser, languageIsoCode: preservedLanguage } : currentUser);
					this.view.setTitle(currentUser.name);
				},
				'umbCurrentUserWorkspaceObserver',
			);
		}
	}

	getEntityType(): string {
		return UMB_CURRENT_USER_ENTITY_TYPE;
	}

	getUnique(): string | null | undefined {
		return this.#data.getValue()?.unique;
	}

	getData(): UmbCurrentUserModel | undefined {
		return this.#data.getValue();
	}

	getName(): string | undefined {
		return this.#data.getValue()?.name;
	}

	setLanguageIsoCode(languageIsoCode: string) {
		const current = this.#data.getValue();
		if (!current) return;
		this.#data.setValue({ ...current, languageIsoCode });
	}

	setAvatarFile(file: File) {
		this.#revokePendingAvatarPreviewUrl();
		this.#pendingAvatar.setValue({ file, previewUrl: URL.createObjectURL(file), delete: false });
	}

	markAvatarForDeletion() {
		this.#revokePendingAvatarPreviewUrl();
		const hasExistingAvatar = (this.#data.getValue()?.avatarUrls.length ?? 0) > 0;
		this.#pendingAvatar.setValue({ delete: hasExistingAvatar });
	}

	clearPendingAvatar() {
		this.#revokePendingAvatarPreviewUrl();
		this.#pendingAvatar.setValue({});
	}

	#revokePendingAvatarPreviewUrl() {
		const previewUrl = this.#pendingAvatar.getValue().previewUrl;
		if (previewUrl) URL.revokeObjectURL(previewUrl);
	}

	protected async submit(): Promise<void> {
		const data = this.#data.getValue();
		if (!data) throw new Error('Current user data not loaded');

		const errors: unknown[] = [];

		const pending = this.#pendingAvatar.getValue();
		if (pending.file) {
			const { error } = await this.#repository.uploadAvatar(pending.file);
			if (error) errors.push(error);
		} else if (pending.delete) {
			const { error } = await this.#repository.deleteAvatar();
			if (error) errors.push(error);
		}

		if (data.languageIsoCode && data.languageIsoCode !== this.#persistedLanguageIsoCode) {
			const { error } = await this.#repository.updateProfile(data.languageIsoCode);
			if (error) errors.push(error);
		}

		if (errors.length) {
			throw new Error('current-user-save-failed');
		}

		this.clearPendingAvatar();
	}

	override destroy(): void {
		this.#revokePendingAvatarPreviewUrl();
		this.#data.destroy();
		this.#pendingAvatar.destroy();
		super.destroy();
	}
}

export { UmbCurrentUserWorkspaceContext as api };
