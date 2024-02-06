import { UMB_BLOCK_CATALOGUE_MODAL } from '../../block/index.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from '../manager/block-grid-manager.context.js';
import type { UmbBlockGridLayoutModel } from '../types.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from './block-grid-entries.context-token.js';
import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from './block-grid-entry.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockGridEntriesContext extends UmbContextBase<UmbBlockGridEntriesContext> {
	//
	#blockManager?: typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE;
	#catalogueModal: UmbModalRouteRegistrationController<typeof UMB_BLOCK_CATALOGUE_MODAL.DATA, undefined>;
	#catalogueRouteBuilder?: UmbModalRouteBuilder;

	#layoutEntries = new UmbArrayState<UmbBlockGridLayoutModel>([], (x) => x.contentUdi);
	layoutEntries = this.#layoutEntries.asObservable();

	setLayoutEntries(layoutEntries: Array<UmbBlockGridLayoutModel>) {
		this.#layoutEntries.setValue(layoutEntries);
	}
	getLayoutEntries() {
		return this.#layoutEntries.value;
	}

	setParentKey(contentUdi: string) {
		this.#catalogueModal.setUniquePathValue('parentUnique', contentUdi);
	}
	getParentKey() {
		return '';
	}

	setAreaKey(areaKey: string) {
		this.#catalogueModal.setUniquePathValue('areaKey', areaKey);
	}
	getAreaKey() {
		return '';
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_ENTRIES_CONTEXT.toString());

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias', 'parentUnique', 'areaKey'])
			.addAdditionalPath(':view/:index')
			.onSetup((routingInfo) => {
				// Idea: Maybe on setup should be async, so it can retrieve the values when needed? [NL]
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				return {
					data: {
						blocks: [],
						blockGroups: [],
						openClipboard: routingInfo.view === 'clipboard',
						blockOriginData: { index: index },
					},
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#catalogueRouteBuilder = routeBuilder;
			});

		// TODO: Observe Blocks of the layout entries of this component.
		this.consumeContext(UMB_BLOCK_GRID_MANAGER_CONTEXT, (blockGridManager) => {
			this.#blockManager = blockGridManager;
			this.#gotBlockManager();
		});
	}

	#gotBlockManager() {
		if (!this.#blockManager) return;

		this.observe(
			this.#blockManager.propertyAlias,
			(alias) => {
				this.#catalogueModal.setUniquePathValue('propertyAlias', alias);
			},
			'observePropertyAlias',
		);
	}

	getPathForCreateBlock(index: number) {
		return this.#catalogueRouteBuilder?.({ view: 'create', index: index });
	}

	getPathForClipboard(index: number) {
		return this.#catalogueRouteBuilder?.({ view: 'clipboard', index: index });
	}

	// create Block?

	deleteBlock() {}
}
