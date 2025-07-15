import { UmbTiptapExtensionApiBase } from '../base.js';
import { css } from '@umbraco-cms/backoffice/external/lit';
import { CharacterCount } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapWordCountExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [CharacterCount];

	override getStyles = () => css``;
}
