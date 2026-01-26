import { UmbCollectionActionBase } from '../../../collection/action/collection-action-base.js';
import type { UmbRecycleBinRepository } from '../../recycle-bin-repository.interface.js';
import type { ManifestCollectionActionEmptyRecycleBinKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Collection action for emptying the recycle bin.
 * @class UmbEmptyRecycleBinCollectionAction
 * @augments {UmbCollectionActionBase}
 */
export class UmbEmptyRecycleBinCollectionAction extends UmbCollectionActionBase {
	#manifest: ManifestCollectionActionEmptyRecycleBinKind;

	constructor(host: UmbControllerHost, manifest: ManifestCollectionActionEmptyRecycleBinKind) {
		super(host);
		this.#manifest = manifest;
	}

	/**
	 * Executes the action.
	 * @memberof UmbEmptyRecycleBinCollectionAction
	 */
	async execute() {
		if (!this.#manifest.meta.recycleBinRepositoryAlias) {
			throw new Error('Recycle Bin Repository Alias is not defined in the manifest meta');
		}

		await umbConfirmModal(this._host, {
			headline: `#actions_emptyrecyclebin`,
			content: `#defaultdialogs_recycleBinWarning`,
			color: 'danger',
			confirmLabel: `#actions_emptyrecyclebin`,
		});

		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.#manifest.meta.recycleBinRepositoryAlias,
		);

		const { error } = await recycleBinRepository.requestEmpty();
		if (error) {
			throw error;
		}

		// Refresh the collection
		const collectionContext = await this.getContext(UMB_COLLECTION_CONTEXT);
		collectionContext?.loadCollection();

		// Refresh the tree (if any)
		await this.#reloadChildrenOfEntity();
	}

	/**
	 * Requests a reload of the children of the current entity in the tree.
	 * Silently returns if the required contexts are not available.
	 */
	async #reloadChildrenOfEntity() {
		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) return;

		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) return;

		const entityType = entityContext.getEntityType();
		if (!entityType) return;

		const unique = entityContext.getUnique();

		const event = new UmbRequestReloadChildrenOfEntityEvent({ entityType, unique });
		eventContext.dispatchEvent(event);
	}
}

export { UmbEmptyRecycleBinCollectionAction as api };
