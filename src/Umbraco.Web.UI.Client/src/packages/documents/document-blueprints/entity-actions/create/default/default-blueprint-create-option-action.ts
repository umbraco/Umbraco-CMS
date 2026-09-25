import { UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { UmbBlueprintDocumentTypeStructureRepository } from '../../../repository/structure/index.js';
import type { UmbDocumentBlueprintFolderEntityType, UmbDocumentBlueprintRootEntityType } from '../../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import { umbOpenModal, UMB_ITEM_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbDefaultBlueprintCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	#structureRepository = new UmbBlueprintDocumentTypeStructureRepository(this);
	#localize = new UmbLocalizationController(this);

	override async execute() {
		const parentEntityType = this.args.entityType as
			| UmbDocumentBlueprintRootEntityType
			| UmbDocumentBlueprintFolderEntityType;
		const parentUnique = this.args.unique ?? null;

		const { data } = await this.#structureRepository.requestAllowedChildrenOf(null, parentUnique);

		const value = await umbOpenModal(this, UMB_ITEM_PICKER_MODAL, {
			data: {
				headline: this.#localize.term('blueprints_chooseDocumentType'),
				items: (data?.items ?? []).flatMap((documentType) =>
					documentType.unique
						? [
								{
									label: documentType.name,
									value: documentType.unique,
									description: documentType.description ?? undefined,
									icon: documentType.icon ?? undefined,
								},
							]
						: [],
				),
			},
		});

		const documentTypeUnique = value?.value;

		if (!documentTypeUnique) {
			throw new Error('Document type unique is not available');
		}

		const url = UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
			documentTypeUnique,
		});

		history.pushState(null, '', url);
	}
}

export { UmbDefaultBlueprintCreateOptionAction as api };
