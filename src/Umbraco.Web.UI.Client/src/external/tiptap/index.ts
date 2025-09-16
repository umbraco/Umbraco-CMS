// REQUIRED EXTENSIONS
export * from '@tiptap/core';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Document,
} from '@tiptap/extension-document';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Dropcursor,
} from '@tiptap/extension-dropcursor';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Gapcursor,
} from '@tiptap/extension-gapcursor';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	HardBreak,
} from '@tiptap/extension-hard-break';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	History,
} from '@tiptap/extension-history';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Paragraph,
} from '@tiptap/extension-paragraph';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Placeholder,
} from '@tiptap/extension-placeholder';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Text,
} from '@tiptap/extension-text';

// OPTIONAL EXTENSIONS
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Blockquote,
} from '@tiptap/extension-blockquote';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Bold,
} from '@tiptap/extension-bold';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	BulletList,
} from '@tiptap/extension-bullet-list';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	CharacterCount,
} from '@tiptap/extension-character-count';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Code,
} from '@tiptap/extension-code';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	CodeBlock,
} from '@tiptap/extension-code-block';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Heading,
} from '@tiptap/extension-heading';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	HorizontalRule,
} from '@tiptap/extension-horizontal-rule';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Image,
} from '@tiptap/extension-image';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Italic,
} from '@tiptap/extension-italic';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Link,
} from '@tiptap/extension-link';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	ListItem,
} from '@tiptap/extension-list-item';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	OrderedList,
} from '@tiptap/extension-ordered-list';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Strike,
} from '@tiptap/extension-strike';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Subscript,
} from '@tiptap/extension-subscript';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Superscript,
} from '@tiptap/extension-superscript';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Table,
} from '@tiptap/extension-table';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	TableCell,
} from '@tiptap/extension-table-cell';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	TableHeader,
} from '@tiptap/extension-table-header';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	TableRow,
} from '@tiptap/extension-table-row';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	TextAlign,
} from '@tiptap/extension-text-align';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	TextStyle,
} from '@tiptap/extension-text-style';
export {
	/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
	Underline,
} from '@tiptap/extension-underline';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export { StarterKit } from '@tiptap/starter-kit';

// CUSTOM EXTENSIONS
export * from './extensions/tiptap-anchor.extension.js';
export * from './extensions/tiptap-div.extension.js';
export * from './extensions/tiptap-figcaption.extension.js';
export * from './extensions/tiptap-figure.extension.js';
export * from './extensions/tiptap-span.extension.js';
export * from './extensions/tiptap-html-global-attributes.extension.js';
export * from './extensions/tiptap-text-direction-extension.js';
export * from './extensions/tiptap-text-indent-extension.js';
export * from './extensions/tiptap-trailing-node.extension.js';
export * from './extensions/tiptap-umb-bubble-menu.extension.js';
export * from './extensions/tiptap-umb-embedded-media.extension.js';
export * from './extensions/tiptap-umb-image.extension.js';
export * from './extensions/tiptap-umb-link.extension.js';
export * from './extensions/tiptap-umb-table.extension.js';
