/**
 * Lazy-loaded bundle of every first-party Tiptap extension, toolbar, statusbar, modal
 * and table-action API/element. Manifests reference everything here via
 * `() => import('./extension-apis.bundle.js').then((m) => ({ default: m.X }))` thunks so
 * Rollup emits a single shared chunk that is only fetched the first time an RTE is
 * actually rendered. Workspaces that don't host an RTE never load this bundle.
 *
 * Do NOT add a static import of this file from anywhere — that would defeat the lazy
 * boundary and pull all extension code back into the manifest registration bundle.
 */

// Extension APIs
export { default as UmbTiptapAnchorExtensionApi } from './anchor/anchor.tiptap-api.js';
export { default as UmbTiptapBlockElementApi } from './block/block.tiptap-api.js';
export { default as UmbTiptapBlockquoteExtensionApi } from './blockquote/blockquote.tiptap-api.js';
export { default as UmbTiptapBoldExtensionApi } from './bold/bold.tiptap-api.js';
export { default as UmbTiptapBulletListExtensionApi } from './bullet-list/bullet-list.tiptap-api.js';
export { default as UmbTiptapCodeBlockExtensionApi } from './code-block/code-block.tiptap-api.js';
export { default as UmbTiptapEmbeddedMediaExtensionApi } from './embedded-media/embedded-media.tiptap-api.js';
export { default as UmbTiptapFigureExtensionApi } from './figure/figure.tiptap-api.js';
export { default as UmbTiptapHeadingExtensionApi } from './heading/heading.tiptap-api.js';
export { default as UmbTiptapHorizontalRuleExtensionApi } from './horizontal-rule/horizontal-rule.tiptap-api.js';
export { default as UmbTiptapHtmlAttributeClassExtensionApi } from './html-attr-class/html-attr-class.tiptap-api.js';
export { default as UmbTiptapHtmlAttributeDatasetExtensionApi } from './html-attr-dataset/html-attr-dataset.tiptap-api.js';
export { default as UmbTiptapHtmlAttributeIdExtensionApi } from './html-attr-id/html-attr-id.tiptap-api.js';
export { default as UmbTiptapHtmlAttributeStyleExtensionApi } from './html-attr-style/html-attr-style.tiptap-api.js';
export { default as UmbTiptapHtmlTagDivExtensionApi } from './html-tag-div/html-tag-div.tiptap-api.js';
export { default as UmbTiptapHtmlTagSpanExtensionApi } from './html-tag-span/html-tag-span.tiptap-api.js';
export { default as UmbTiptapImageExtensionApi } from './image/image.tiptap-api.js';
export { default as UmbTiptapItalicExtensionApi } from './italic/italic.tiptap-api.js';
export { default as UmbTiptapLinkExtensionApi } from './link/link.tiptap-api.js';
export { default as UmbTiptapMediaUploadExtensionApi } from './media-upload/media-upload.tiptap-api.js';
export { default as UmbTiptapOrderedListExtensionApi } from './ordered-list/ordered-list.tiptap-api.js';
export { default as UmbTiptapRichTextEssentialsExtensionApi } from './core/rich-text-essentials.tiptap-api.js';
export { default as UmbTiptapStrikeExtensionApi } from './strike/strike.tiptap-api.js';
export { default as UmbTiptapSubscriptExtensionApi } from './subscript/subscript.tiptap-api.js';
export { default as UmbTiptapSuperscriptExtensionApi } from './superscript/superscript.tiptap-api.js';
export { default as UmbTiptapTableExtensionApi } from './table/table.tiptap-api.js';
export { default as UmbTiptapTextAlignExtensionApi } from './text-align/text-align.tiptap-api.js';
export { default as UmbTiptapTextDirectionExtensionApi } from './text-direction/text-direction.tiptap-api.js';
export { default as UmbTiptapTextIndentExtensionApi } from './text-indent/text-indent.tiptap-api.js';
export { default as UmbTiptapTrailingNodeExtensionApi } from './trailing-node/trailing-node.tiptap-api.js';
export { default as UmbTiptapUnderlineExtensionApi } from './underline/underline.tiptap-api.js';
export { default as UmbTiptapWordCountExtensionApi } from './word-count/word-count.tiptap-api.js';

// Toolbar APIs
export { default as UmbTiptapToolbarAnchorExtensionApi } from './anchor/anchor.tiptap-toolbar-api.js';
export { default as UmbTiptapBlockPickerToolbarExtension } from './block/block.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarBlockquoteExtensionApi } from './blockquote/blockquote.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarBoldExtensionApi } from './bold/bold.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarBulletListExtensionApi } from './bullet-list/bullet-list.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarCharacterMapExtensionApi } from './character-map/character-map.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarClearFormattingExtensionApi } from './clear-formatting/clear-formatting.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarCodeBlockExtensionApi } from './code-block/code-block.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarEmbeddedMediaExtensionApi } from './embedded-media/embedded-media.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarFontFamilyExtensionApi } from './font-family/font-family.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarFontSizeExtensionApi } from './font-size/font-size.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHeading1ExtensionApi } from './heading/heading1.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHeading2ExtensionApi } from './heading/heading2.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHeading3ExtensionApi } from './heading/heading3.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHeading4ExtensionApi } from './heading/heading4.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHeading5ExtensionApi } from './heading/heading5.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHeading6ExtensionApi } from './heading/heading6.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarHorizontalRuleExtensionApi } from './horizontal-rule/horizontal-rule.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarItalicExtensionApi } from './italic/italic.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarLinkExtensionApi } from './link/link.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarUnlinkExtensionApi } from './link/unlink.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarMediaPickerToolbarExtensionApi } from './media-picker/media-picker.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarOrderedListExtensionApi } from './ordered-list/ordered-list.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarRedoExtensionApi } from './undo-redo/redo.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarSourceEditorExtensionApi } from './view-source/source-editor.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarStrikeExtensionApi } from './strike/strike.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarStyleMenuApi } from './style-menu/style-menu.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarSubscriptExtensionApi } from './subscript/subscript.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarSuperscriptExtensionApi } from './superscript/superscript.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTableExtensionApi } from './table/table.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextAlignCenterExtensionApi } from './text-align/text-align-center.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextAlignJustifyExtensionApi } from './text-align/text-align-justify.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextAlignLeftExtensionApi } from './text-align/text-align-left.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextAlignRightExtensionApi } from './text-align/text-align-right.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextColorBackgroundExtensionApi } from './text-color/text-color-background.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextColorForegroundExtensionApi } from './text-color/text-color-foreground.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextDirectionLtrExtensionApi } from './text-direction/text-direction-ltr.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextDirectionRtlExtensionApi } from './text-direction/text-direction-rtl.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextIndentExtensionApi } from './text-indent/text-indent.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarTextOutdentExtensionApi } from './text-indent/text-outdent.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarUndoExtensionApi } from './undo-redo/undo.tiptap-toolbar-api.js';
export { default as UmbTiptapToolbarUnderlineExtensionApi } from './underline/underline.tiptap-toolbar-api.js';

// Statusbar elements
export { UmbTiptapStatusbarElementPathElement } from './element-path/element-path.tiptap-statusbar-element.js';
export { UmbTiptapStatusbarWordCountElement } from './word-count/word-count.tiptap-statusbar-element.js';

// Modal elements
export { UmbTiptapAnchorModalElement } from './anchor/modals/anchor-modal.element.js';
export { UmbCharacterMapModalElement } from './character-map/modals/character-map-modal.element.js';
export { UmbTiptapTablePropertiesModalElement } from './table/modals/table-properties-modal.element.js';

// Toolbar UI elements (referenced from kind manifests)
export { UmbTiptapTableToolbarMenuElement } from './table/components/table-toolbar-menu.element.js';
export { UmbTiptapToolbarButtonActionElement } from '../components/toolbar/tiptap-toolbar-button-action.element.js';
export { UmbTiptapToolbarButtonElement } from '../components/toolbar/tiptap-toolbar-button.element.js';
export { UmbTiptapToolbarColorPickerButtonElement } from '../components/toolbar/tiptap-toolbar-color-picker-button.element.js';
export { UmbTiptapToolbarMenuElement } from '../components/toolbar/tiptap-toolbar-menu.element.js';

// Table menu-item action APIs
export { default as UmbTableCellMergeAction } from './table/actions/table-cell-merge.action.js';
export { default as UmbTableCellMergeSplitAction } from './table/actions/table-cell-merge-split.action.js';
export { default as UmbTableCellSplitAction } from './table/actions/table-cell-split.action.js';
export { default as UmbTableCellToggleHeaderAction } from './table/actions/table-cell-toggle-header.action.js';
export { default as UmbTableColumnAddAfterAction } from './table/actions/table-column-add-after.action.js';
export { default as UmbTableColumnAddBeforeAction } from './table/actions/table-column-add-before.action.js';
export { default as UmbTableColumnDeleteAction } from './table/actions/table-column-delete.action.js';
export { default as UmbTableColumnToggleHeaderAction } from './table/actions/table-column-toggle-header.action.js';
export { default as UmbTableDeleteAction } from './table/actions/table-delete.action.js';
export { default as UmbTablePropertiesAction } from './table/actions/table-properties.action.js';
export { default as UmbTableRowAddAfterAction } from './table/actions/table-row-add-after.action.js';
export { default as UmbTableRowAddBeforeAction } from './table/actions/table-row-add-before.action.js';
export { default as UmbTableRowDeleteAction } from './table/actions/table-row-delete.action.js';
export { default as UmbTableRowToggleHeaderAction } from './table/actions/table-row-toggle-header.action.js';

// Clipboard property-value translators
export { UmbTiptapBlockRteToBlockClipboardCopyPropertyValueTranslator } from '../clipboard/block/copy/block-rte-to-block-copy-translator.js';
export { UmbTiptapBlockToBlockRteClipboardPastePropertyValueTranslator } from '../clipboard/block/paste/block-to-block-rte-paste-translator.js';
