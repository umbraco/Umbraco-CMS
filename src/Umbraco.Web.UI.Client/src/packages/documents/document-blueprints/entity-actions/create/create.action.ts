import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL } from './constants.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDocumentBlueprintEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL, {
			data: {
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});

		await modalContext.onSubmit().catch(() => undefined);

		const documentTypeUnique = modalContext.getValue().documentTypeUnique;
		if (!documentTypeUnique) return;

		const url = `section/settings/workspace/${UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE}/create/parent/${this.args.entityType}/${this.args.unique ?? 'null'}/${documentTypeUnique}`;
		history.pushState(null, '', url);
	}
}

export default UmbCreateDocumentBlueprintEntityAction;
