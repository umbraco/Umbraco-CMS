import { BulletList, ListItem } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

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
