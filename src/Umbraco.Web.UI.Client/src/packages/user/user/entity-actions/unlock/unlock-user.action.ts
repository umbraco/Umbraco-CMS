import type { UmbUnlockUserRepository } from '../../repository/index.js';
import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbUnlockUserEntityAction extends UmbEntityActionBase<UmbUnlockUserRepository> {
	#itemRepository: UmbUserItemRepository;

	constructor(host: UmbControllerHost, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.#itemRepository = new UmbUserItemRepository(this);
	}

	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) return;

		const { data } = await this.#itemRepository.requestItems([this.unique]);

		if (data) {
			const item = data[0];

			await umbConfirmModal(this._host, {
				headline: `Unlock ${item.name}`,
				content: 'Are you sure you want to unlock this user?',
				confirmLabel: 'Unlock',
			});

			await this.repository?.unlock([this.unique]);
		}
	}
}
