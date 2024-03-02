import type { UmbMediaDetailRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';

export class UmbMediaDeleteEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaDetailRepository> {
	async execute() {
		console.log(`execute delete for: ${this.selection}`);

		// TODO: show error
		if (!this.repository) return;

		// TODO: should we subscribe in cases like this?
		/*
		const { data } = await this.repository.requestItemsLegacy(this.selection);

		if (data) {
			// TODO: use correct markup
			const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const modalContext = modalManager.open(this, UMB_CONFIRM_MODAL, {
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

			await modalContext.onSubmit();
			await this.repository?.trash(this.selection);
		}
		*/
	}
}
