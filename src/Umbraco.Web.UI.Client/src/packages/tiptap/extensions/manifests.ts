import { manifest as buttonKind } from './tiptap-toolbar-button.kind.js';
import { manifest as colorPickerButton } from './tiptap-toolbar-color-picker-button.kind.js';
import { manifest as menuButton } from './tiptap-toolbar-menu.kind.js';
import { manifest as styleMenuKind } from './style-menu/style-menu.kind.js';
import { manifests as anchor } from './anchor/manifests.js';
import { manifests as block } from './block/manifests.js';
import { manifests as blockquote } from './blockquote/manifests.js';
import { manifests as bold } from './bold/manifests.js';
import { manifests as bulletList } from './bullet-list/manifests.js';
import { manifests as characterMap } from './character-map/manifests.js';
import { manifests as clearFormatting } from './clear-formatting/manifests.js';
import { manifests as codeBlock } from './code-block/manifests.js';
import { manifests as core } from './core/manifests.js';
import { manifests as elementPath } from './element-path/manifests.js';
import { manifests as embeddedMedia } from './embedded-media/manifests.js';
import { manifests as figure } from './figure/manifests.js';
import { manifests as fontFamily } from './font-family/manifests.js';
import { manifests as fontSize } from './font-size/manifests.js';
import { manifests as heading } from './heading/manifests.js';
import { manifests as horizontalRule } from './horizontal-rule/manifests.js';
import { manifests as htmlAttrClass } from './html-attr-class/manifests.js';
import { manifests as htmlAttrDataset } from './html-attr-dataset/manifests.js';
import { manifests as htmlAttrId } from './html-attr-id/manifests.js';
import { manifests as htmlAttrStyle } from './html-attr-style/manifests.js';
import { manifests as htmlTagDiv } from './html-tag-div/manifests.js';
import { manifests as htmlTagSpan } from './html-tag-span/manifests.js';
import { manifests as image } from './image/manifests.js';
import { manifests as italic } from './italic/manifests.js';
import { manifests as link } from './link/manifests.js';
import { manifests as mediaPicker } from './media-picker/manifests.js';
import { manifests as mediaUpload } from './media-upload/manifests.js';
import { manifests as orderedList } from './ordered-list/manifests.js';
import { manifests as strike } from './strike/manifests.js';
import { manifests as styleSelect } from './style-select/manifests.js';
import { manifests as subscript } from './subscript/manifests.js';
import { manifests as superscript } from './superscript/manifests.js';
import { manifests as table } from './table/manifests.js';
import { manifests as textAlign } from './text-align/manifests.js';
import { manifests as textColor } from './text-color/manifests.js';
import { manifests as textDirection } from './text-direction/manifests.js';
import { manifests as textIndent } from './text-indent/manifests.js';
import { manifests as trailingNode } from './trailing-node/manifests.js';
import { manifests as underline } from './underline/manifests.js';
import { manifests as undoRedo } from './undo-redo/manifests.js';
import { manifests as viewSource } from './view-source/manifests.js';
import { manifests as wordCount } from './word-count/manifests.js';

const kinds = [buttonKind, colorPickerButton, menuButton, styleMenuKind];

export const manifests = [
	...kinds,
	...anchor,
	...block,
	...blockquote,
	...bold,
	...bulletList,
	...characterMap,
	...clearFormatting,
	...codeBlock,
	...core,
	...elementPath,
	...embeddedMedia,
	...figure,
	...fontFamily,
	...fontSize,
	...heading,
	...horizontalRule,
	...htmlAttrClass,
	...htmlAttrDataset,
	...htmlAttrId,
	...htmlAttrStyle,
	...htmlTagDiv,
	...htmlTagSpan,
	...image,
	...italic,
	...link,
	...mediaPicker,
	...mediaUpload,
	...orderedList,
	...strike,
	...styleSelect,
	...subscript,
	...superscript,
	...table,
	...textAlign,
	...textColor,
	...textDirection,
	...textIndent,
	...trailingNode,
	...underline,
	...undoRedo,
	...viewSource,
	...wordCount,
];
