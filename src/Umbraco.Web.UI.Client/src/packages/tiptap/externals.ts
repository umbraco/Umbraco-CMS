// Tiptap v3 Docs:
// https://tiptap.dev/docs/guides/upgrade-tiptap-v2
// https://tiptap.dev/docs/resources/whats-new
// https://github.com/ueberdosis/tiptap/issues

// REQUIRED EXTENSIONS
export * from '@tiptap/core';
export * from '@tiptap/extensions';
export { Document } from '@tiptap/extension-document';
export { HardBreak } from '@tiptap/extension-hard-break';
export { Paragraph } from '@tiptap/extension-paragraph';
export { Text } from '@tiptap/extension-text';

// PROSEMIRROR TYPES
export { NodeSelection } from '@tiptap/pm/state';
export type { Node as ProseMirrorNode } from '@tiptap/pm/model';

// OPTIONAL EXTENSIONS
export { Blockquote } from '@tiptap/extension-blockquote';
export { Bold } from '@tiptap/extension-bold';
export { BulletList, OrderedList, ListItem } from '@tiptap/extension-list';
export { Code } from '@tiptap/extension-code';
export { CodeBlock } from '@tiptap/extension-code-block';
export { Heading } from '@tiptap/extension-heading';
export { HorizontalRule } from '@tiptap/extension-horizontal-rule';
export { Image } from '@tiptap/extension-image';
export { Italic } from '@tiptap/extension-italic';
export { Link } from '@tiptap/extension-link';
export { Strike } from '@tiptap/extension-strike';
export { Subscript } from '@tiptap/extension-subscript';
export { Superscript } from '@tiptap/extension-superscript';
export { Table, TableCell, TableHeader, TableRow } from '@tiptap/extension-table';
export { TextAlign } from '@tiptap/extension-text-align';
export { TextStyle } from '@tiptap/extension-text-style';
export { Underline } from '@tiptap/extension-underline';
