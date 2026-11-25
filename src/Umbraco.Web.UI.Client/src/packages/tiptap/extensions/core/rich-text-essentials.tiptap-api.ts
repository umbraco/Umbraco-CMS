import { Document, Dropcursor, Gapcursor, HardBreak, Paragraph, Text, UndoRedo } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { UmbTrimMarkWhitespace } from './trim-mark-whitespace.tiptap-extension.js';

export default class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		Document,
		Dropcursor,
		Gapcursor,
		HardBreak,
		Paragraph,
		Text,
		UndoRedo,
		UmbTrimMarkWhitespace,
	];
}
