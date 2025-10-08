import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import {
	Document,
	Dropcursor,
	Gapcursor,
	HardBreak,
	History,
	Paragraph,
	Text,
} from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapRichTextEssentialsExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Document, Dropcursor, Gapcursor, HardBreak, History, Paragraph, Text];
}
