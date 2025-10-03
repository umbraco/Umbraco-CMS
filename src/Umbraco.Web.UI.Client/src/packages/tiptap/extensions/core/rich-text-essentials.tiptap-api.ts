import { Document, Dropcursor, Gapcursor, HardBreak, Paragraph, Text, UndoRedo } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Document, Dropcursor, Gapcursor, HardBreak, Paragraph, Text, UndoRedo];
}
