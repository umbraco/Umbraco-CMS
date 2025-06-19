import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL } from './constants.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDocumentBlueprintEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		const value = await umbOpenModal(this, UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL, {
			data: {
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});

		const documentTypeUnique = value.documentTypeUnique;
		if (!documentTypeUnique) {
			throw new Error('Document type unique is not available');
		}

		// TODO: Lets avoid having such hardcoded URLs. [NL]
		const url = `section/settings/workspace/${UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE}/create/parent/${this.args.entityType}/${this.args.unique ?? 'null'}/${documentTypeUnique}`;
		history.pushState(null, '', url);
	}
}

export default UmbCreateDocumentBlueprintEntityAction;
