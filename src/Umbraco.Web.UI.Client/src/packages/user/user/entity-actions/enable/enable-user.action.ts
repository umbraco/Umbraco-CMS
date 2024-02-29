import type { UmbEnableUserRepository } from '../../repository/enable/enable-user.repository.js';
import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbEnableUserEntityAction extends UmbEntityActionBase<UmbEnableUserRepository> {
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
				headline: `Enable ${item.name}`,
				content: 'Are you sure you want to enable this user?',
				confirmLabel: 'Enable',
			});

			await this.repository?.enable([this.unique]);
		}
	}
}
