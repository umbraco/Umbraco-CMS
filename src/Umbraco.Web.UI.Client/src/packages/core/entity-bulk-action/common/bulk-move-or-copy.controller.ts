import { UmbEntityBulkActionProgressController } from '../progress/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/tree';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

export interface UmbBulkMoveOrCopyArgs {
	/**
	 * The entities to move or copy.
	 */
	selection: Array<string>;
	/**
	 * Headline for the destination tree picker (localization alias, e.g. `#actions_move`).
	 */
	pickerHeadline: string;
	/**
	 * Confirm-button label for the destination tree picker (localization alias).
	 */
	pickerConfirmLabel: string;
	/**
	 * Localization term key for the progress dialog headline.
	 */
	progressHeadlineKey: string;
	foldersOnly?: boolean;
	hideTreeRoot?: boolean;
	treeAlias?: string;
	searchProviderAlias?: string;
	/**
	 * Performs the actual bulk operation against the chosen destination (`null` = root). Called once the
	 * user has picked a destination; its returned promise is awaited behind an indeterminate progress dialog.
	 */
	perform: (destinationUnique: string | null) => Promise<unknown>;
}

/**
 * Shared flow for the "move to" and "copy to" bulk actions: pick a destination, run the (single,
 * server-side) bulk operation behind an indeterminate progress dialog, then reload the parent entity.
 */
export class UmbBulkMoveOrCopyController extends UmbControllerBase {
	async run(args: UmbBulkMoveOrCopyArgs): Promise<void> {
		if (args.selection.length === 0) return;

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				headline: args.pickerHeadline,
				confirmLabel: args.pickerConfirmLabel,
				foldersOnly: args.foldersOnly,
				hideTreeRoot: args.hideTreeRoot,
				treeAlias: args.treeAlias,
				search: args.searchProviderAlias ? { providerAlias: args.searchProviderAlias } : undefined,
			},
		}).catch(() => undefined);

		if (!value?.selection?.length) return;

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const localize = new UmbLocalizationController(this);
		await new UmbEntityBulkActionProgressController(this).runIndeterminate({
			headline: localize.term(args.progressHeadlineKey),
			operation: args.perform(destinationUnique),
			delayMs: 400,
		});

		await this.#reloadDestination();
	}

	async #reloadDestination(): Promise<void> {
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) throw new Error('Entity Context is not available');

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) throw new Error('Event Context is not available');

		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (entityType && unique !== undefined) {
			const args = { entityType, unique };
			eventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent(args));
			eventContext.dispatchEvent(new UmbRequestReloadStructureForEntityEvent(args));
		}
	}
}
