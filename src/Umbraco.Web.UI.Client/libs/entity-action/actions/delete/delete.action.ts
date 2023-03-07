import { UmbEntityActionBase } from '@umbraco-cms/entity-action';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from 'libs/modal';

export class UmbDeleteEntityAction<
	T extends { delete(unique: string): Promise<void>; requestItems(uniques: Array<string>): any }
> extends UmbEntityActionBase<T> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalContext) return;

		const { data } = await this.repository.requestItems([this.unique]);

		if (data) {
			const item = data[0];

			const modalHandler = this.#modalContext.confirm({
				headline: `Delete ${item.name}`,
				content: 'Are you sure you want to delete this item?',
				color: 'danger',
				confirmLabel: 'Delete',
			});

			const { confirmed } = await modalHandler.onClose();
			if (confirmed) {
				await this.repository?.delete(this.unique);
			}
		}
	}
}
