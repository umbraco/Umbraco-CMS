import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { CharacterCount } from '../../externals.js';

export default class UmbTiptapWordCountExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [CharacterCount.configure()];

	override getStyles = () => css``;
}
