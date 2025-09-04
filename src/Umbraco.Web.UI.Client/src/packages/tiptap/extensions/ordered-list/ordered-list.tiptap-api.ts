import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { ListItem, OrderedList } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapOrderedListExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [ListItem, OrderedList];

	override getStyles = () => css`
		li {
			> p {
				margin: 0;
				padding: 0;
			}
		}
	`;
}
