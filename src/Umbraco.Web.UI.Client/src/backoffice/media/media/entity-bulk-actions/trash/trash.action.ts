import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbActionBase } from '../../../../shared/action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { html } from 'lit-html';

export class UmbMediaTrashEntityBulkAction extends UmbActionBase<UmbMediaRepository> {
	#selection: Array<string>;
	#modalService?: UmbModalService;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.#selection = selection;

		new UmbContextConsumerController(host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	setSelection(selection: Array<string>) {
		this.#selection = selection;
	}

	async execute() {
		// TODO: show error
		if (!this.#modalService || !this.repository) return;

		// TODO: should we subscribe in cases like this?
		const { data } = await this.repository.requestTreeItems(this.#selection);

		if (data) {
			// TODO: use correct markup
			const modalHandler = this.#modalService?.confirm({
				headline: `Deleting ${this.#selection.length} items`,
				content: html`
					This will delete the following files:
					<ul style="list-style-type: none; padding: 0; margin: 0; margin-top: var(--uui-size-space-2);">
						${data.map((item) => html`<li>${item.name}</li>`)}
					</ul>
				`,
				color: 'danger',
				confirmLabel: 'Delete',
			});

			const { confirmed } = await modalHandler.onClose();
			if (confirmed) {
				await this.repository?.trash(this.#selection);
			}
		}
	}
}
