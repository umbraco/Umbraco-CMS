import { UmbTiptapExtensionApiBase } from '../types.js';
import { BulletList, ListItem, OrderedList } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapListExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [BulletList, OrderedList, ListItem];
}
