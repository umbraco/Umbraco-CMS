import { UMB_BLOCK_RTE_WORKSPACE_MODAL } from '../workspace/index.js';
import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/tiny-mce';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export default class UmbTinyMceMultiUrlPickerPlugin extends UmbTinyMcePluginBase {
	#localize = new UmbLocalizationController(this._host);

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
	}

	async showDialog() {
		const blockEl = this.editor.selection.getNode();
		let blockUdi: string | undefined;

		if (blockEl.nodeName === 'UMB-RTE-BLOCK' || blockEl.nodeName === 'UMB-RTE-BLOCK-INLINE') {
			blockUdi = blockEl.getAttribute('data-content-udi') ?? undefined;
		}

		this.#openBlockPicker(blockUdi);
	}

	async #openBlockPicker(blockUdi?: string) {
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
