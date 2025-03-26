// REQUIRED EXTENSIONS
export * from '@tiptap/core';
export { StarterKit } from '@tiptap/starter-kit';
export { Placeholder } from '@tiptap/extension-placeholder';
export { TextStyle } from '@tiptap/extension-text-style';

// OPTIONAL EXTENSIONS
export { Blockquote } from '@tiptap/extension-blockquote';
export { Bold } from '@tiptap/extension-bold';
export { BulletList } from '@tiptap/extension-bullet-list';
export { CharacterCount } from '@tiptap/extension-character-count';
export { Code } from '@tiptap/extension-code';
export { CodeBlock } from '@tiptap/extension-code-block';
export { Heading } from '@tiptap/extension-heading';
export { HorizontalRule } from '@tiptap/extension-horizontal-rule';
export { Image } from '@tiptap/extension-image';
export { Italic } from '@tiptap/extension-italic';
export { Link } from '@tiptap/extension-link';
export { ListItem } from '@tiptap/extension-list-item';
export { OrderedList } from '@tiptap/extension-ordered-list';
export { Strike } from '@tiptap/extension-strike';
export { Subscript } from '@tiptap/extension-subscript';
export { Superscript } from '@tiptap/extension-superscript';
export { Table } from '@tiptap/extension-table';
export { TableCell } from '@tiptap/extension-table-cell';
export { TableHeader } from '@tiptap/extension-table-header';
export { TableRow } from '@tiptap/extension-table-row';
export { TextAlign } from '@tiptap/extension-text-align';
export { Underline } from '@tiptap/extension-underline';

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
