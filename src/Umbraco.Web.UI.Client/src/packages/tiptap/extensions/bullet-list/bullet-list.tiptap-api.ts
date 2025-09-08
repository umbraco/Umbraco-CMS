import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { BulletList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBulletListExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [ListItem, BulletList];

	override getStyles = () => css`
		li {
			> p {
				margin: 0;
				padding: 0;
			}
		}
	`;
}
