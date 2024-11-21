import { UmbInstallerContext } from '../installer.context.js';
import { html, type TemplateResult } from '@umbraco-cms/backoffice/external/lit';

export const installerContextProvider = (
	story: () => Node | string | TemplateResult,
	installerContext = new UmbInstallerContext(),
) => html`
	<umb-context-provider
		style="display: block;margin: 2rem 25%;padding: 1rem;border: 1px solid #ddd;"
		key="umbInstallerContext"
		.value=${installerContext}>
		${story()}
	</umb-context-provider>
`;
