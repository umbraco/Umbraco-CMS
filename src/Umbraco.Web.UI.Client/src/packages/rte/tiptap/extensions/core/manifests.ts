import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { manifest as blockquote } from './blockquote.extension.js';
import { manifest as bold } from './bold.extension.js';
import { manifest as bulletList } from './bullet-list.extension.js';
import { manifest as codeBlock } from './code-block.extension.js';
import { manifest as image } from './image.extension.js';
import { manifest as italic } from './italic.extension.js';
import { manifest as heading1 } from './heading1.extension.js';
import { manifest as heading2 } from './heading2.extension.js';
import { manifest as heading3 } from './heading3.extension.js';
import { manifest as horizontalRule } from './horizontal-rule.extension.js';
import { manifest as orderedList } from './ordered-list.extension.js';
import { manifest as strike } from './strike.extension.js';
import { manifest as textAlignLeft } from './text-align-left.extension.js';
import { manifest as textAlignCenter } from './text-align-center.extension.js';
import { manifest as textAlignRight } from './text-align-right.extension.js';
import { manifest as textAlignJustify } from './text-align-justify.extension.js';
import { manifest as underline } from './underline.extension.js';

export const manifests: Array<ManifestTiptapExtension> = [
	blockquote,
	bold,
	bulletList,
	codeBlock,
	image,
	italic,
	heading1,
	heading2,
	heading3,
	horizontalRule,
	orderedList,
	strike,
	textAlignLeft,
	textAlignCenter,
	textAlignRight,
	textAlignJustify,
	underline,
];
