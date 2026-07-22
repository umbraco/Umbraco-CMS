import { UmbEntityBulkActionProgressController } from '../progress/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/tree';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

export interface UmbBulkTreePickerActionArgs {
	/**
	 * The entities the operation is performed for.
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
	 * Headline for the progress dialog (localization alias, e.g. `#actions_moveInProgress`).
	 */
	progressHeadline: string;
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
 * Shared flow for bulk actions that pick a destination from a tree and then run a single, server-side
 * operation against it (e.g. "move to" and "duplicate to"): open the destination tree picker, await the
 * operation behind an indeterminate progress dialog, then reload the parent entity.
 */
export class UmbBulkTreePickerActionController extends UmbControllerBase {
	async run(args: UmbBulkTreePickerActionArgs): Promise<void> {
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

		await new UmbEntityBulkActionProgressController(this).runIndeterminate({
			headline: args.progressHeadline,
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
