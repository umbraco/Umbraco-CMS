import { html } from 'lit';
import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbMediaTrashEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: show error
		if (!this.#modalContext || !this.repository) return;

		// TODO: should we subscribe in cases like this?
		const { data } = await this.repository.requestTreeItems(this.selection);

		if (data) {
			// TODO: use correct markup
			const modalHandler = this.#modalContext?.open(UMB_CONFIRM_MODAL, {
				headline: `Deleting ${this.selection.length} items`,
				content: html`
					This will delete the following files:
					<ul style="list-style-type: none; padding: 0; margin: 0; margin-top: var(--uui-size-space-2);">
						${data.map((item) => html`<li>${item.name}</li>`)}
					</ul>
				`,
				color: 'danger',
				confirmLabel: 'Delete',
			});

			await modalHandler.onSubmit();
			await this.repository?.trash(this.selection);
		}
	}
}
