import { UMB_BLOCK_RTE_WORKSPACE_MODAL } from '../workspace/index.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from '../context/block-rte-entries.context-token.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '../context/block-rte-manager.context.js';
import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/tiny-mce';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

export default class UmbTinyMceMultiUrlPickerPlugin extends UmbTinyMcePluginBase {
	#localize = new UmbLocalizationController(this._host);

	private _blocks?: Array<UmbBlockTypeBaseModel>;
	#entriesContext?: typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		args.editor.on('preInit', () => {
			args.editor.serializer.addRules('umb-rte-block');

			/** This checks if the div is a block element*/
			args.editor.serializer.addNodeFilter('umb-rte-block', function (nodes) {
				for (let i = 0; i < nodes.length; i++) {
					const blockEl = nodes[i];
					/* if the block is set to display inline, checks if its wrapped in a p tag and then unwraps it (removes p tag) */
					if (blockEl.parent && blockEl.parent.name.toUpperCase() === 'P') {
						blockEl.parent.unwrap();
					}
				}
			});
		});

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
		const blockEl = this.editor.selection.getNode();

		if (blockEl.nodeName === 'UMB-RTE-BLOCK' || blockEl.nodeName === 'UMB-RTE-BLOCK-INLINE') {
			const blockUdi = blockEl.getAttribute('data-content-udi') ?? undefined;
			if (blockUdi) {
				this.#editBlock(blockUdi);
				return;
			}
		}

		// If no block is selected, open the block picker:
		this.#createBlock();
	}

	async #editBlock(blockUdi?: string) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalHandler = modalManager.open(this, UMB_BLOCK_RTE_WORKSPACE_MODAL, {
			data: {
				preset: {
					blockUdi,
				},
			},
		});

		if (!modalHandler) return;

		const blockPickerData = await modalHandler.onSubmit().catch(() => undefined);
		if (!blockPickerData) return;

		for (const block of blockPickerData) {
			this.#insertBlockInEditor(block.layout?.contentUdi ?? '', (block.layout as any)?.displayInline ?? false);
		}
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

	#insertBlockInEditor(blockContentUdi: string, displayInline = false) {
		if (!blockContentUdi) {
			return;
		}
		if (displayInline) {
			this.editor.selection.setContent(
				'<umb-rte-block-inline data-content-udi="' + blockContentUdi + '"><!--Umbraco-Block--></umb-rte-block-inline>',
			);
		} else {
			this.editor.selection.setContent(
				'<umb-rte-block data-content-udi="' + blockContentUdi + '"><!--Umbraco-Block--></umb-rte-block>',
			);
		}

		// angularHelper.safeApply($rootScope, function () {
		// 	editor.dispatch("Change");
		// });
	}
}
