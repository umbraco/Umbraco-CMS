import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { umbRteBlock, umbRteBlockInline, umbRteBlockPaste } from './block.tiptap-extension.js';
import { distinctUntilChanged } from '@umbraco-cms/backoffice/external/rxjs';
import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteManagerContext, UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	#managerContext?: UmbBlockRteManagerContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			this.#managerContext = context;
			this.observe(
				context?.contents.pipe(
					distinctUntilChanged((prev, curr) => prev.map((y) => y.key).join() === curr.map((y) => y.key).join()),
				),
				(contents) => {
					if (!contents || !context) {
						return;
					}
					this.#updateBlocks(contents, context.getLayouts());
				},
				'contents',
			);
		});
	}

	/**
	 * Handles paste events containing RTE blocks.
	 * Clones block data with new content keys to avoid duplicate references.
	 * @param {ClipboardEvent} event - The clipboard event containing pasted content.
	 * @returns {boolean} True if the paste was handled, false to let default paste proceed.
	 */
	#handleBlockPaste = (event: ClipboardEvent): boolean => {
		// Ensure we have a manager context and editor
		if (!this.#managerContext || !this._editor) {
			return false;
		}

		// Check if the HTML contains block elements
		const html = event.clipboardData?.getData('text/html');
		if (!html || !html.includes('umb-rte-block')) {
			return false;
		}

		// Sanitize the HTML immediately to prevent XSS attacks while preserving RTE block elements
		const sanitizedHtml = DOMPurify.sanitize(html, {
			ADD_TAGS: ['umb-rte-block', 'umb-rte-block-inline'],
			ADD_ATTR: [UMB_BLOCK_RTE_DATA_CONTENT_KEY],
		});

		// Parse the sanitized HTML
		const parser = new DOMParser();
		const doc = parser.parseFromString(sanitizedHtml, 'text/html');
		const blockElements = doc.querySelectorAll('umb-rte-block, umb-rte-block-inline');

		if (blockElements.length === 0) {
			return false;
		}

		// Process each block element
		let hasValidBlocks = false;

		blockElements.forEach((blockElement) => {
			const oldContentKey = blockElement.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY);
			if (!oldContentKey) {
				// Remove block elements without content keys
				blockElement.remove();
				return;
			}

			// Check if block data exists in the manager context
			const originalContent = this.#managerContext!.getContentOf(oldContentKey);
			if (!originalContent) {
				// Block data doesn't exist (stale paste or cross-RTE), remove the element
				blockElement.remove();
				return;
			}

			// Generate new content key
			const newContentKey = UmbId.new();

			// Clone content data with new key
			const clonedContent: UmbBlockDataModel = structuredClone(originalContent);
			clonedContent.key = newContentKey;
			this.#managerContext!.setOneContent(clonedContent);
			this.#managerContext!.setOneExpose(newContentKey, UmbVariantId.CreateInvariant());

			// Find and clone layout
			const layouts = this.#managerContext!.getLayouts();
			const originalLayout = layouts.find((l) => l.contentKey === oldContentKey);
			if (originalLayout) {
				let newSettingsKey: string | undefined = undefined;

				// Clone settings if present
				if (originalLayout.settingsKey) {
					const originalSettings = this.#managerContext!.getSettingsOf(originalLayout.settingsKey);
					if (originalSettings) {
						newSettingsKey = UmbId.new();
						const clonedSettings: UmbBlockDataModel = structuredClone(originalSettings);
						clonedSettings.key = newSettingsKey;
						this.#managerContext!.setOneSettings(clonedSettings);
					}
				}

				// Create new layout entry
				const clonedLayout: UmbBlockRteLayoutModel = structuredClone(originalLayout);
				clonedLayout.contentKey = newContentKey;
				clonedLayout.settingsKey = newSettingsKey;
				this.#managerContext!.setOneLayout(clonedLayout);
			}

			// Update the DOM element with the new content key
			blockElement.setAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY, newContentKey);
			hasValidBlocks = true;
		});

		// If no valid blocks remain and body is empty, let default paste handle it
		if (!hasValidBlocks && doc.body.innerHTML.trim() === '') {
			return false;
		}

		// Insert the modified HTML (already sanitized at function entry)
		this._editor.commands.insertContent(doc.body.innerHTML, { parseOptions: { preserveWhitespace: 'full' } });

		// Return true to indicate we handled the paste
		return true;
	};

	#updateBlocks(blocks: UmbBlockDataModel[], layouts: Array<UmbBlockRteLayoutModel>) {
		const editor = this._editor;
		if (!editor) return;

		const existingBlocks = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY),
		);
		const newBlocks = blocks.filter((x) => !existingBlocks.find((contentKey) => contentKey === x.key));

		newBlocks.forEach((block) => {
			// Find layout for block
			const layout = layouts.find((x) => x.contentKey === block.key);
			const inline = layout?.displayInline ?? false;

			if (inline) {
				editor.commands.setBlockInline({ contentKey: block.key });
			} else {
				editor.commands.setBlock({ contentKey: block.key });
			}
		});
	}

	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline, umbRteBlockPaste(this.#handleBlockPaste)];
	}
}
