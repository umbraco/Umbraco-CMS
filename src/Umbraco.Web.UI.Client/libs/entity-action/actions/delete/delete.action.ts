import { UMB_CONFIRM_MODAL_TOKEN } from '../../../../src/backoffice/shared/modals/confirm';
import { UmbEntityActionBase } from '@umbraco-cms/entity-action';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

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

			const modalHandler = this.#modalContext.open(UMB_CONFIRM_MODAL_TOKEN, {
				headline: `Delete ${item.name}`,
				content: 'Are you sure you want to delete this item?',
				color: 'danger',
				confirmLabel: 'Delete',
			});

			await modalHandler.onSubmit();
			await this.repository?.delete(this.unique);
		}
	}
}
