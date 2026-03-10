import { Heading } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

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
			margin-bottom: 1rem;

			&:first-child {
				margin-top: 0.25rem;
			}
		}
	`;
}
