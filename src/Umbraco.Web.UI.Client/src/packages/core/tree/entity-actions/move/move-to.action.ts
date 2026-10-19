import { UMB_TREE_PICKER_MODAL } from '../../tree-picker-modal/index.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeRepository } from '../../data/tree-repository.interface.js';
import type { UmbMoveRepository } from './move-repository.interface.js';
import type { MetaEntityActionMoveToKind } from './types.js';
import {
	UmbEntityActionBase,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';

export class UmbMoveToEntityAction extends UmbEntityActionBase<MetaEntityActionMoveToKind> {
	protected async _getPickableFilter(unique: string): Promise<((item: UmbTreeItemModel) => boolean) | undefined> {
		return (treeItem) => treeItem.unique !== unique;
	}

	#searchConfig() {
		const alias = this.args.meta.searchProviderAlias;
		return alias ? { providerAlias: alias } : undefined;
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const [ancestors, pickableFilter] = await Promise.all([
			this.#requestAncestors(),
			this._getPickableFilter(this.args.unique),
		]);

		const treeExpansion = ancestors.length ? linkEntityExpansionEntries(ancestors) : undefined;

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				headline: '#actions_move',
				confirmLabel: '#general_move',
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				expandTreeRoot: true,
				treeExpansion,
				pickableFilter,
				search: this.#searchConfig(),
			},
		}).catch(() => undefined);

		// The modal was cancelled.
		if (!value) return;

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const moveRepository = await createExtensionApiByAlias<UmbMoveRepository>(this, this.args.meta.moveRepositoryAlias);
		if (!moveRepository) throw new Error('Move Repository is not available');

		const { error } = await moveRepository.requestMoveTo({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
		});

		if (error) {
			throw error;
		}

		await this.#reloadMenu(destinationUnique);
	}

	async #requestAncestors() {
		try {
			const treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(
				this,
				this.args.meta.treeRepositoryAlias,
			);
			const { data } =
				(await treeRepository?.requestTreeItemAncestors({
					treeItem: { unique: this.args.unique!, entityType: this.args.entityType! },
				})) ?? {};
			// Exclude self — the API returns the descendant as part of the ancestors list, but we only want to expand its parents.
			return data?.filter((item) => item.unique !== this.args.unique) ?? [];
		} catch {
			// Tree pre-expansion is a UX convenience — if it fails the modal still opens normally.
			return [];
		}
	}

	async #reloadMenu(destinationUnique: string | null) {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) throw new Error('Action Event Context is not available');
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		const destination = await this.#requestDestination(destinationUnique);
		if (destination) {
			actionEventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent(destination));
		}
	}

	/**
	 * Resolves the entity to reload the children of, so that the moved item appears under its new parent.
	 * @remarks
	 * The picker only hands back the destination's unique, but the children of an entity are reloaded by unique
	 * *and* entity type. The moved item now sits under the destination, so the destination is one of its
	 * ancestors - which is where the entity type comes from.
	 */
	async #requestDestination(destinationUnique: string | null): Promise<UmbEntityModel | undefined> {
		try {
			const treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(
				this,
				this.args.meta.treeRepositoryAlias,
			);
			if (!treeRepository) return undefined;

			if (destinationUnique === null) {
				const { data } = await treeRepository.requestTreeRoot();
				return data ? { unique: data.unique, entityType: data.entityType } : undefined;
			}

			const { data } = await treeRepository.requestTreeItemAncestors({
				treeItem: { unique: this.args.unique!, entityType: this.args.entityType! },
			});

			const destination = data?.find((item) => item.unique === destinationUnique);
			return destination ? { unique: destination.unique, entityType: destination.entityType } : undefined;
		} catch {
			// The move itself has already succeeded, so a failure to resolve the destination must not surface as
			// a failed move - the destination is simply left to be reloaded manually.
			return undefined;
		}
	}
}

export default UmbMoveToEntityAction;
