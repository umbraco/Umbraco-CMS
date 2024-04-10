import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_CREATE_OPTIONS_MODAL } from './modal/index.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DOCUMENT_BLUEPRINT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});

		await modalContext.onSubmit().catch(() => undefined);

		const documentTypeUnique = modalContext.getValue().documentTypeUnique;

		const url = `section/settings/workspace/${UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE}/create/parent/${this.args.entityType}/${
			documentTypeUnique || 'null'
		}`;
		history.pushState(null, '', url);
	}
}
