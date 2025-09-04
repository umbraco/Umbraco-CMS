import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHeadingExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Heading];

	override getStyles = () => css`
		h1,
		h2,
		h3,
		h4,
		h5,
		h6 {
			margin-top: 0;
			margin-bottom: 0.5em;
		}
	`;
}
