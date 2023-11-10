import { type UmbEnableUserRepository } from '../../repository/enable/enable-user.repository.js';
import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbEnableUserEntityAction extends UmbEntityActionBase<UmbEnableUserRepository> {
	#modalManager?: UmbModalManagerContext;
	#itemRepository: UmbUserItemRepository;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.#itemRepository = new UmbUserItemRepository(this);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManager = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalManager) return;

		const { data } = await this.#itemRepository.requestItems([this.unique]);

		if (data) {
			const item = data[0];

			const modalContext = this.#modalManager.open(UMB_CONFIRM_MODAL, {
				headline: `Enable ${item.name}`,
				content: 'Are you sure you want to enable this user?',
				confirmLabel: 'Enable',
			});

			await modalContext.onSubmit();
			await this.repository?.enable([this.unique]);
		}
	}
}
