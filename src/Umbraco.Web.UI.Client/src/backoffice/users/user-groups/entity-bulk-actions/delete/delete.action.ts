import { html } from 'lit';
import type { UmbUserGroupRepository } from '../../repository/user-group.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbDeleteUserGroupEntityBulkAction extends UmbEntityBulkActionBase<UmbUserGroupRepository> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.#modalContext || this.selection.length === 0) return;

		const modalHandler = this.#modalContext.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Delete user groups?`,
			content: html`Are you sure you want to delete selected user groups?`,
			confirmLabel: 'Delete',
		});

		await modalHandler.onSubmit();

		//TODO: How should we handle bulk actions? right now we send a request per item we want to change.
		//TODO: For now we have to reload the page to see the update
		for (let index = 0; index < this.selection.length; index++) {
			const element = this.selection[index];
			await this.repository?.delete(element);
		}
	}
}
