import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from '../context/block-rte-entries.context-token.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '../context/block-rte-manager.context-token.js';
import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/tiny-mce';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

export default class UmbTinyMceMultiUrlPickerPlugin extends UmbTinyMcePluginBase {
	#localize = new UmbLocalizationController(this._host);

	private _blocks?: Array<UmbBlockTypeBaseModel>;
	#entriesContext?: typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		args.editor.ui.registry.addToggleButton('umbblockpicker', {
			icon: 'visualblocks',
			tooltip: this.#localize.term('blockEditor_insertBlock'),
			onAction: () => this.showDialog(),
			onSetup: function (api) {
				const changed = args.editor.selection.selectorChangedWithUnbind(
					'umb-rte-block[data-content-udi], umb-rte-block-inline[data-content-udi]',
					(state) => api.setActive(state),
				);
				return () => changed.unbind();
			},
		});

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			context.setTinyMceEditor(args.editor);

			this.observe(
				context.blockTypes,
				(blockTypes) => {
					this._blocks = blockTypes;
				},
				'blockType',
			);
		});
		this.consumeContext(UMB_BLOCK_RTE_ENTRIES_CONTEXT, (context) => {
			this.#entriesContext = context;
		});
	}

	async showDialog() {
		//const blockEl = this.editor.selection.getNode();

		/*if (blockEl.nodeName === 'UMB-RTE-BLOCK' || blockEl.nodeName === 'UMB-RTE-BLOCK-INLINE') {
			const blockUdi = blockEl.getAttribute('data-content-udi') ?? undefined;
			if (blockUdi) {
				// TODO: Missing a solution to edit a block from this scope. [NL]
				this.#editBlock(blockUdi);
				return;
			}
		}*/

		// If no block is selected, open the block picker:
		this.#createBlock();
	}

	#createBlock() {
		// TODO: Missing solution to skip catalogue if only one type available. [NL]
		let createPath: string | undefined = undefined;

		if (this._blocks?.length === 1) {
			const elementKey = this._blocks[0].contentElementTypeKey;
			createPath = this.#entriesContext?.getPathForCreateBlock() + 'modal/umb-modal-workspace/create/' + elementKey;
		} else {
			createPath = this.#entriesContext?.getPathForCreateBlock();
		}

		if (createPath) {
			window.history.pushState({}, '', createPath);
		}
	}
}
