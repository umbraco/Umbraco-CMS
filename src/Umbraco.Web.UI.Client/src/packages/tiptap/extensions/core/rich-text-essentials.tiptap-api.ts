import { Document, Dropcursor, Gapcursor, HardBreak, History, Paragraph, Text } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Document, Dropcursor, Gapcursor, HardBreak, History, Paragraph, Text];
}
